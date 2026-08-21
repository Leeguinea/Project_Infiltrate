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

            //Resource/Icons/폴더에서 스프라이트를 찾아옴
            Sprite icon = Resources.Load<Sprite>($"Icons/{itemType}");
            iconImage.sprite = icon;
            Debug.Log("아이콘!");

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