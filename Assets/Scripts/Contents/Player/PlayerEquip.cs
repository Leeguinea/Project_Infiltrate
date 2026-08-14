using UnityEngine;
using System.Collections.Generic;
//플레이어가 아이템을 줍고, 버림.
//플레이어에 붙어있는 스크립트 
public class PlayerEquip : MonoBehaviour
{
    private PlayerController _controller;

    [Header("컴포넌트 연결")]
    [SerializeField] private Inventory _inventory;
    [SerializeField] private ItemDatabase _itemDatabase; // 데이터베이스 연결 추가
    [SerializeField] private Transform _handTransform;   // 손 위치

    public ItemType CurrentItemType { get; private set; } = ItemType.None;
    public bool Using = false;

    [Header("References")]
    public ItemObject _interactableItem;
    public GameObject _handItem;         // 실제 손에 들린 오브젝트


    private void Update()
    {
        GetItemInput();
        DropItemInput();
    }

    // 인벤토리에서 장착할 때 호출되는 함수
    public void EquipWeapon(ItemType itemType)
    {
        // 기존 아이템 처리
        if (_handItem != null)
        {
            Destroy(_handItem);
        }

        // 데이터베이스에서 프리팹 찾기
        GameObject prefabToSpawn = _itemDatabase.GetHandPrefab(itemType);

        if (prefabToSpawn != null)
        {
            // 프리팹 생성 및 장착
            _handItem = Instantiate(prefabToSpawn, _handTransform);
            _handItem.transform.localPosition = Vector3.zero;
            _handItem.transform.localRotation = Quaternion.identity;

            SetupHandItemPhysics(_handItem);
            CurrentItemType = itemType;
        }
        else
        {
            Debug.LogWarning("프리팹을 찾을 수 없습니다.");
            CurrentItemType = ItemType.None;
            _handItem = null;
        }
    }

    // E 버튼으로 아이템 줍기 (월드 상호작용)
    public void GetItemInput()
    {
        if (_interactableItem != null && Input.GetKeyDown(KeyCode.E))
        {
            EquipNewItem(_interactableItem);
        }
    }

    // 새로운 아이템을 장착하는 함수 (상호작용)
    private void EquipNewItem(ItemObject targetItem)
    {
        // 기존에 들고 있던 게 있다면 인벤토리로 보내기
        if (_handItem != null)
        {
            ItemData oldData = _handItem.GetComponent<ItemData>();
            if (oldData != null) _inventory.AddItem(oldData.itemType, 1);
            Destroy(_handItem);
        }

        // 데이터베이스를 활용하여 새 아이템 장착
        ItemData newData = targetItem.GetComponent<ItemData>();
        if (newData != null)
        {
            GameObject prefab = _itemDatabase.GetHandPrefab(newData.itemType);

            // 프리팹이 있다면 생성
            if (prefab != null)
            {
                _handItem = Instantiate(prefab, _handTransform);
                Destroy(targetItem.gameObject); // 바닥 아이템 삭제
            }
            //없다면 그냥 오브젝트 사용
            else
            {
                _handItem = targetItem.gameObject;
                _handItem.transform.SetParent(_handTransform);
            }

            _handItem.transform.localPosition = Vector3.zero;
            _handItem.transform.localRotation = Quaternion.identity;

            SetupHandItemPhysics(_handItem);
            CurrentItemType = newData.itemType;
        }
    }

    // 공통 물리 설정 함수
    private void SetupHandItemPhysics(GameObject itemObj)
    {
        Rigidbody rb = itemObj.GetComponent<Rigidbody>();
        if (rb != null) { rb.isKinematic = true; rb.detectCollisions = false; }

        Collider col = itemObj.GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    // Z 버튼으로 아이템 드롭 (DropCurrentItem과 연동)
    public void DropItemInput()
    {
        if (CurrentItemType != ItemType.None && Input.GetKeyDown(KeyCode.Z))
        {
            DropCurrentItem();
        }
    }

    //현재 들고 있는 아이템 드롭
    private void DropCurrentItem()
    {
        if (_handItem == null) return;

        _handItem.transform.SetParent(null);

        Rigidbody rb = _handItem.GetComponent<Rigidbody>();
        if (rb != null) { rb.isKinematic = false; rb.detectCollisions = true; }

        Collider col = _handItem.GetComponent<Collider>();
        if (col != null) col.isTrigger = false;

        _handItem = null;
        CurrentItemType = ItemType.None;
    }

    //감지된 오브젝트 리스트를 ItemSensor로 받고
    //가장 가장 가까운오브젝트를 추출
    public void SetInteractableItem(List<ItemObject> nearItems)
    {
        if (nearItems != null && nearItems.Count == 0)
        {
            _interactableItem = null;
            return;
        }

        Vector3 playerPos = transform.position;
        Vector3 playerDir = transform.forward;

        ItemObject closestItem = null;
        float highestScore = 0;

        foreach (ItemObject item in nearItems)
        {
            Vector3 toItemDir = item.transform.position - playerPos;
            float distance = toItemDir.magnitude;
            float dot = Vector3.Dot(playerDir, toItemDir.normalized);

            // 정면일 수록 높고, 가까울수록 가산점
            float score = dot - (distance * 0.1f);

            if (score > highestScore)
            {
                highestScore = score; //갱신
                closestItem = item;
            }
        }

        _interactableItem = closestItem;

        if (_interactableItem != null)
        {
            Debug.Log($"현재 상호작용 대상: {_interactableItem}");
        }
    }

    public void ClearInteractableItem(ItemObject item)
    {
        if (_interactableItem == item)
        {
            _interactableItem = null;
            Debug.Log("아이템 범위 벗어남");
        }
    }
}