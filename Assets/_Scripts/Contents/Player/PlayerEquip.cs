using UnityEngine;

//플레이어가 아이템을 줍고, 버림.
//플레이어에 붙어있는 스크립트 
public class PlayerEquip : MonoBehaviour
{
    private PlayerController _controller;

    [Header("컴포넌트 연결")]
    [SerializeField] private Inventory _inventory;

    public ItemType CurrentItemType { get; private set; } = ItemType.None;
    public bool Using = false;

    [Header("References")]
    public ItemObject _interactableItem; //상호작용 가능 아이템 (ItemObject 타입)
    public GameObject _handItem; //손에 쥐고 있는 아이템 (실제 게임 오브젝트)
    public Transform _handTransform; //무기 위치가 될 곳 (새로운 아이템)


    private void Update()
    {
        GetItemInput();
        DropItemInput();
    }

    //E버튼으로 아이템 줍기
    public void GetItemInput()
    {
        if (_interactableItem != null)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                //아이템이 없으면 그냥 줍기
                if (CurrentItemType == ItemType.None)
                {
                    EquipNewItem(_interactableItem);
                }

                //손에 아이템을 쥐고 있으면 버리고, 새거 줍기
                else
                {
                    EquipNewItem(_interactableItem);
                }

            }
        }
    }

    //새로운 아이템을 장착하는 함수 
    private void EquipNewItem(ItemObject targetItem)
    {
        //이미 손에 쥐고 있는 아이템이 있다면 새 아이템 쥐기 전에 처리
        if (_handItem != null && _handItem != targetItem.gameObject)
        {
            // 기존에 들고 있던 아이템의 타입을 알아내서 인벤토리 넣기.
            ItemData oldItemData = _handItem.GetComponent<ItemData>();
            if (oldItemData != null)
            {
                _inventory.AddItem(oldItemData.itemType, 1);
            }


            Destroy(_handItem);
        }

        //새로 주운 아이템을 손에 쥐여줌
        _handItem = targetItem.gameObject; 

        //부모자식 설정 및 위치 초기화
        _handItem.transform.SetParent(_handTransform);
        _handItem.transform.localPosition = Vector3.zero;
        _handItem.transform.localRotation = Quaternion.identity;

        Rigidbody rb = _handItem.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }

        // [손에 쥐고 있는 동안 플레이어가 밟고 뜨는 현상(발판 버그) 방지용 트리거 켜기
        Collider newCol = _handItem.GetComponent<Collider>();
        if (newCol != null)
        {
            newCol.isTrigger = true;
        }

        //ItemData 스크립트 참조 
        ItemData itemData = _handItem.GetComponent<ItemData>();
        if (itemData != null)
        {
            CurrentItemType = itemData.itemType; // 아이템이 Knife면 Knife로, Coin이면 Coin으로 자동 세팅
        }
    }


    //Z버튼으로 아이템 드롭하기
    public void DropItemInput()
    {
        if (CurrentItemType != ItemType.None && Input.GetKeyDown(KeyCode.Z))
        {
            DropCurrentItem(); //물리적 드롭

            CurrentItemType = ItemType.None;
            _handItem = null;
        }
    }


    //들고 있는 아이템을 드롭하는 함수
    private void DropCurrentItem()
    {
        //부모 해제
        _handItem.transform.SetParent(null);

        //물리 원래 상태로
        Rigidbody rbOld = _handItem.GetComponent<Rigidbody>();
        if (rbOld != null)
        {
            rbOld.isKinematic = false;
            rbOld.detectCollisions = true;
        }

        // 버릴 때는 트리거를 꺼서 바닥에 안착하게 함
        Collider colOld = _handItem.GetComponent<Collider>();
        if (colOld != null)
        {
            colOld.isTrigger = false;
        }

        _handItem = null;
        CurrentItemType = ItemType.None;
        Debug.Log("기존 아이템 버리기 성공");

    }

    public void SetInteractableItem(ItemObject item)
    {
        _interactableItem = item;
        Debug.Log($"아이템 감지됨: {item.name}");
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