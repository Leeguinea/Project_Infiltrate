using UnityEngine;

//개별 아이템마다 붙는 스크립트
public class ItemObject : MonoBehaviour
{
    private ItemData _itemData;

    void Awake()
    {
        _itemData = GetComponent<ItemData>();
    }

    // 플레이어가 상호작용 키를 눌렀을 때 직접 호출할 함수
    public void PickUp()
    {
        if (_itemData != null)
        {
            Debug.Log($"{_itemData.itemType} 획득!");
            // TODO: 인벤토리에 아이템 추가하는 로직
        }

        Destroy(gameObject); // 월드에서 제거
    }

}

