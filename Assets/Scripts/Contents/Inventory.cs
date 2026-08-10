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

    //UI
    [SerializeField] 
    private Transform slotParent; // InventoryPanel 오브젝트
    private List<SlotUI> slotUIs = new List<SlotUI>();

    //데이터
    [SerializeField] 
    private int slotCapacity = 20; // 인벤토리 칸 수
    public List<Slot> slots = new List<Slot>();


    private void Awake()
    {
        // 1. 패널 밑에 있는 모든 SlotUI 컴포넌트들을 가져와서 배열로 받은 뒤 리스트로 변환하거나 담기
        SlotUI[] foundSlots = slotParent.GetComponentsInChildren<SlotUI>();
        
        // 게임 시작할 때 빈 슬록들로 미리 칸 채우기
        for (int i = 0; i < slotCapacity; i++)
        {
            slots.Add(new Slot { itemType = ItemType.None, count = 0 });

            // UI 주머니에도 찾아온 슬롯 넣어주기
            if (i < foundSlots.Length)
            {
                slotUIs.Add(foundSlots[i]);
            }
        }
    }

    private void OnValidate()
    {
        // 게임이 실행 중일 때만 업데이트 수행
        if (Application.isPlaying)
        {
            UpdateUI();
        }
    }


    //데이터 ADD
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

                UpdateUI();
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

                UpdateUI();
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

                UpdateUI();
                return;
            }
        }
    }

    //UI 띄우기
    public void UpdateUI()
    {
        for (int i = 0; i < slotUIs.Count; i++)
        {
            // 내 인벤토리 데이터(slots)에 있는 정보와, 화면에 있는 UI(_slots)를 1:1로 연결!
            if (i < slots.Count && slots[i].itemType != ItemType.None)
            {
                slotUIs[i].SetSlot(slots[i].itemType, slots[i].count);
            }
            else
            {
                slotUIs[i].ClearSlot(); // 데이터 개수보다 UI가 더 많다면 빈 칸으로 만들기
            }
        }
    }

}