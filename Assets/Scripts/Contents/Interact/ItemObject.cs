using UnityEngine;

//개별 아이템마다 붙는 스크립트
public class ItemObject : Interactable
{
    private ItemData _itemData;

    void Awake()
    {
        _itemData = GetComponent<ItemData>();
    }

    //상호작용 결론 
    public override void OnInteractComplete()
    {
        base.OnInteractComplete();

        if (_itemData != null)
        {
            
            Debug.Log($"{_itemData.itemType} 획득!");
            //TODO: 인벤토리나 PlayerEquip에 _itemData.itemType을 전달하는 로직 추가
        }
        Destroy(gameObject);
    }       
    
  


}


