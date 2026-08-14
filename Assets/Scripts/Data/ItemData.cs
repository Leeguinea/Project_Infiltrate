using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [System.Serializable]
    public struct ItemData
    {
        public ItemType itemType;
        public GameObject handPrefab; // 손에 들릴 실제 무기 프리팹
    }

    public List<ItemData> itemDatas;

    // ItemType을 넣으면 해당하는 프리팹을 찾아주는 편의 함수
    public GameObject GetHandPrefab(ItemType itemType)
    {
        foreach (var data in itemDatas)
        {
            if (data.itemType == itemType)
            {
                return data.handPrefab;
            }
        }
        return null;
    }
}