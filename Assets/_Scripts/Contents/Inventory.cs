using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct Slot
{
    public ItemType itemType; 
    public int count;       

    // 빈 슬롯 확인용
    public bool IsEmpty => itemType == ItemType.None; // (만약 ItemType에 None이 없다면 적절히 수정)
}


public class Inventory : MonoBehaviour
{
    private PlayerEquip _playerEquip;

    [SerializeField] private int slotCapacity = 20; // 인벤토리 칸 수
    public List<Slot> slots = new List<Slot>();

    private void Awake()
    {
        // 게임 시작할 때 빈 슬록들로 미리 칸 채우기
        for (int i = 0; i < slotCapacity; i++)
        {
            slots.Add(new Slot { itemType = ItemType.None, count = 0 });
        }
    }

    public void AddItem(ItemType newItemType, int count)
    {
        // 이미 인벤토리에 같은 아이템이 있는지 찾고, 있다면 count를 더하기
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].itemType == newItemType)
            {
                Slot targetSlot = slots[i];
                targetSlot.count += count;
                slots[i] = targetSlot;

                return;
            }
        }
        // 같은 아이템이 없다면, 빈 슬롯(IsEmpty)을 찾아 새로 아이템 채우기
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].IsEmpty)
            {
                slots[i] = new Slot
                {
                    itemType = newItemType,
                    count = count
                };

                return;
            }
        }

        Debug.Log("인벤토리가 꽉 찼습니다!");
    }


    public void DropCurrentItem(ItemType currentItemType, int count)
    {
        //인벤토리에 들어 있는 아이템(기존에 있다면 -1)
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].itemType == currentItemType)
            {
                Slot targetSlot = slots[i];
                targetSlot.count -= count;

                if (targetSlot.count <= 0)
                {
                    targetSlot.itemType = ItemType.None;
                    targetSlot.count = 0;
                }
                slots[i] = targetSlot; //덮어씌우기
                return;
            }
        }


    }

}