using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI 요소 연결")]
    [SerializeField] private GameObject GameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverText;

    [Header("상호작용 UI (원 게이지 버전)")]
    [SerializeField] private GameObject interactionPanel;
    [SerializeField] private Image circularGaugeImage;
    [SerializeField] private TextMeshProUGUI promptText;

    void Awake()
    {
        if(Instance == null) { Instance = this; DontDestroyOnLoad(gameObject);  }
        else { Destroy(gameOverText); }

        if (interactionPanel != null) interactionPanel.SetActive(false);

        //게임오버 시 패널 off
        if (GameOverPanel!= null) GameOverPanel.SetActive(false);
    }

    // 게임오버
    public void ShowGameOverUI()
    {
        if (GameOverPanel != null)
        {
            GameOverPanel.SetActive(true);
            Debug.Log("[UIManager] 게임오버 UI 화면 표시 성공");
        }
    }

    // 원 게이지 상호작용 UI ON
    public void OpenInteractionUI(string prompt)
    {
        if (interactionPanel == null) return;
        
        promptText.text = prompt;
        circularGaugeImage.fillAmount = 0f; //원 게이지 0%로 초기화
        interactionPanel.SetActive(true);
    }

    //실시간 원 게이지 채우기(PlayerInteraction에서 호출함)
    public void UpdateInteractionProgress(float progress)
    {
        if(circularGaugeImage != null)
        {
            //0.0(비어있음) ~ 1.0(100%)
            circularGaugeImage.fillAmount = progress;
        }
    }

    //원 게이지 상호작용 UI OFF
    public void CloseInteractionUI()
    {
        if(interactionPanel != null) interactionPanel.SetActive(false);
    }


}
