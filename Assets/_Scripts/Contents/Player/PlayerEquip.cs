using UnityEngine;

public class PlayerEquip : MonoBehaviour
{
    private PlayerController _controller;

    [Header("컴포넌트 연결")]
    [SerializeField] private Inventory _inventory;

    public ItemType CurrentItemType { get; private set; } = ItemType.None;
    public bool Using = false;

    [Header("References")]
    public GameObject _interactableItem; //상호작용 가능 아이템
    public GameObject _handItem; //손에 쥐고 있는 아이템
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
                    // 1. CurrentItemType을 Inventory에 보내주기.
                    _inventory.AddItem(CurrentItemType);
                    //2._handItem(false); //눈에 보이지 않게

                    EquipNewItem(_interactableItem);
                }

            }
        }
    }

    //Z버튼으로 아이템 드롭하기
    public void DropItemInput()
    {
        if (CurrentItemType != ItemType.None && Input.GetKeyDown(KeyCode.Z))
        {
            DropCurrentItem();
        }
    }


    //새로은 아이템을 겟하는 함수
    private void EquipNewItem(GameObject targetItem)
    {
        _handItem = targetItem;

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

        //ItemData 스크립트 참조 
        ItemData itemData = _handItem.GetComponent<ItemData>();
        if (itemData != null)
        {
            CurrentItemType = itemData.itemType; // 아이템이 Knife면 Knife로, Coin이면 Coin으로 자동 세팅
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

        _handItem = null;
        CurrentItemType = ItemType.None;
        Debug.Log("기존 아이템 버리기 성공");

    }

}
