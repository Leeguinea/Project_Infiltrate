using System;
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct Slot
{
    public ItemType itemType;
    public int count;

    // 빈 슬롯 확인용
    public bool IsEmpty => itemType == ItemType.None;
}

public class Inventory : MonoBehaviour
{
    public static bool isInventoryOpen = false; // 인벤토리가 열릴 때만 A, D 우선권 부여 
    private PlayerEquip _playerEquip;

    // UI
    [SerializeField] private Transform slotParent; // InventoryPanel 오브젝트
    [SerializeField] private GameObject inventoryPanel; // 인벤토리 전체창
    [SerializeField] private GameObject selectionHighlight; // 인스펙터에서 테두리 이미지 오브젝트 연결
    [SerializeField] private Transform[] slotTransforms;    // 5개 슬롯의 위치를 담을 배열

    [Header("Inventory Settings")]
    [SerializeField] private int slotCapacity = 20;          // 전체 인벤토리 칸 수 (데이터 총량)
    private int maxVisibleSlots = 5;    // 화면에 보이는 슬롯 개수

    // 데이터 및 상태 관리
    public List<Slot> slots = new List<Slot>();            // 전체 아이템 데이터 주머니 (20칸)
    private List<SlotUI> slotUIs = new List<SlotUI>();      // UI 슬롯 컴포넌트 리스트

    private int selectedIndex = 0;                          // 화면에 보이는 5개 슬롯 중 선택된 위치 (0 ~ 4)
    private int dataStartIndex = 0;

    private void Awake()
    {
        // 1. 패널 밑에 있는 모든 SlotUI 컴포넌트들을 가져와서 리스트에 담기
        SlotUI[] foundSlots = slotParent.GetComponentsInChildren<SlotUI>();

        // slotTransforms 배열 자동 채우기 
        slotTransforms = new Transform[foundSlots.Length];
        for (int i = 0; i < foundSlots.Length; i++)
        {
            slotTransforms[i] = foundSlots[i].transform;
        }

        // 게임 시작할 때 빈 슬롯들로 미리 칸 채우기
        for (int i = 0; i < slotCapacity; i++)
        {
            slots.Add(new Slot { itemType = ItemType.None, count = 0 });

            // UI 주머니에도 찾아온 슬롯 넣어주기
            if (i < foundSlots.Length)
            {
                slotUIs.Add(foundSlots[i]);
            }
        }

        _playerEquip = GetComponent<PlayerEquip>();
        if (_playerEquip == null)
        {
            _playerEquip = FindAnyObjectByType<PlayerEquip>();
        }
    }

    private void Update()
    {
        // I 키로 인벤토리 열고 닫기
        if (Input.GetKeyDown(KeyCode.I))
        {
            isInventoryOpen = !isInventoryOpen;
            inventoryPanel.SetActive(isInventoryOpen); // 패널 켜고 끄기

            if (isInventoryOpen)
            {
                selectedIndex = 0;
                UpdateUI();
                UpdateSelectionUI();
            }
        }

        // 인벤토리가 안 켜져 있으면 아래 코드를 실행하지 않음!
        if (!isInventoryOpen) return;

        // D 키를 누르면 오른쪽 이동 함수 호출
        if (Input.GetKeyDown(KeyCode.D))
        {
            MoveRight();
        }

        // A 키를 누르면 왼쪽 이동 함수 호출
        if (Input.GetKeyDown(KeyCode.A))
        {
            MoveLeft();
        }

        // 스페이스바를 누르면 현재 선택된 아이템 장착 시도
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryEquipSelected();
        }
    }

    // 선택한 슬롯의 아이템을 PlayerEquip으로 전달하여 장착
    void TryEquipSelected()
    {
        int targetIndex = dataStartIndex + selectedIndex;

        // 데이터 범위를 벗어나는지 예외 처리
        if (targetIndex < 0 || targetIndex >= slots.Count) return;

        Slot targetSlot = slots[targetIndex];

        // 빈 슬롯이면 장착 불가
        if (targetSlot.IsEmpty)
        {
            Debug.Log("비어 있는 슬롯입니다.");
            return;
        }

        // 플레이어 에퀴프 컴포넌트가 없다면 캐싱 시도
        if (_playerEquip == null)
        {
            _playerEquip = FindAnyObjectByType<PlayerEquip>();
        }

        // PlayerEquip으로 데이터 전달 및 인벤토리 소모 처리
        if (_playerEquip != null)
        {
            _playerEquip.EquipWeapon(targetSlot.itemType);
            Debug.Log($"무기 장착 요청: {targetSlot.itemType}");

            // 사용 후 인벤토리에서 1개 소모 또는 제거
            DropCurrentItem(targetSlot.itemType, 1);
        }
    }

    // UI 띄우기 (데이터 슬라이딩)
    public void UpdateUI()
    {
        if (slotUIs == null || slotUIs.Count == 0) return;

        for (int i = 0; i < maxVisibleSlots; i++) // 화면에 보이는 5개 슬롯만큼만 반복
        {
            int targetIndex = dataStartIndex + i; // 시작 위치 + 현재 슬롯 번호

            // 화면에 보이는 UI(slotUIs[i])에 전체 데이터(slots[targetIndex])를 맵핑
            if (targetIndex < slots.Count && slots[targetIndex].itemType != ItemType.None)
            {
                slotUIs[i].SetSlot(slots[targetIndex].itemType, slots[targetIndex].count);
            }
            else
            {
                slotUIs[i].ClearSlot(); // 아이템이 없으면 빈 칸으로 비우기
            }
        }
    }

    void MoveRight()
    {
        selectedIndex++;

        // 테두리가 맨 오른쪽을 넘어가려할 때
        if (selectedIndex >= maxVisibleSlots)
        {
            selectedIndex = maxVisibleSlots - 1; // 테두리는 마지막에 고정
            dataStartIndex++; // 칸을 오른쪽으로 민다.

            // 데이터 범위를 넘지 않게 예외처리
            if (dataStartIndex > slotCapacity - maxVisibleSlots)
            {
                dataStartIndex = slotCapacity - maxVisibleSlots;
            }
        }
        UpdateUI();
        UpdateSelectionUI();
    }

    void MoveLeft()
    {
        selectedIndex--;

        if (selectedIndex < 0)
        {
            selectedIndex = 0;
            dataStartIndex--;

            if (dataStartIndex < 0)
            {
                dataStartIndex = 0;
            }
        }
        UpdateUI();
        UpdateSelectionUI();
    }

    // 선택된 위치로 하이라이트 테두리 이동
    void UpdateSelectionUI()
    {
        if (selectionHighlight != null && slotTransforms.Length > selectedIndex)
        {
            selectionHighlight.transform.position = slotTransforms[selectedIndex].position;
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

    public void ToggleInventory()
    {
        bool isActive = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(isActive);
    }

    // 데이터 ADD
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
        // 인벤토리에 들어 있는 아이템 개수 차감
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
                slots[i] = targetSlot; // 덮어씌우기

                UpdateUI();
                return;
            }
        }
    }
}