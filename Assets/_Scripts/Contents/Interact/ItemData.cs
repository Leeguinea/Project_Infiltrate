using UnityEngine;

//TODO: 확장
public enum ItemType
{
    None,
    Knife,
    Coin
}

public class ItemData : MonoBehaviour
{
    // 인스펙터 창에서 선택 
    public ItemType itemType;
}
