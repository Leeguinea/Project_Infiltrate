using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotUI : MonoBehaviour
{
    private Inventory _inventory;

    [SerializeField] private Image iconImage;       // 아이템 아이콘 이미지
    [SerializeField] private TextMeshProUGUI countText; // 개수 텍스트

    // 슬롯에 아이템 정보를 업데이트하는 함수
    public void SetSlot(ItemType itemType, int count)
    {
        if (itemType == ItemType.None || count <= 0)
        {
            // 빈 슬롯일 때
            ClearSlot();
        }
        else
        {
            // 아이템이 있을 때
            iconImage.gameObject.SetActive(true);

            // TODO: 나중에 ItemType에 맞는 아이콘 이미지를 로드
            // iconImage.sprite = Resources.Load<Sprite>($"Icons/{itemType}");

            if (count > 1)
            {
                countText.text = count.ToString();
                countText.gameObject.SetActive(true);
            }
            else
            {
                countText.gameObject.SetActive(false); // 1개면 숫자 생략
            }
        }
    }

    // 슬롯을 비우는 함수
    public void ClearSlot()
    {
        iconImage.sprite = null;
        iconImage.gameObject.SetActive(false);
        countText.gameObject.SetActive(false);
    }
}