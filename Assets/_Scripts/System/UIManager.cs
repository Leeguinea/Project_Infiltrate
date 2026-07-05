using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI 요소 연결")]
    [SerializeField] private GameObject GameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverText;

    void Awake()
    {
        if(Instance == null) { Instance = this; DontDestroyOnLoad(gameObject);  }
        else { Destroy(gameOverText); }

        //게임오버 시 패널 off
        if(GameOverPanel!= null) GameOverPanel.SetActive(false);
    }

    public void ShowGameOverUI()
    {
        if (GameOverPanel != null)
        {
            GameOverPanel.SetActive(true);
            Debug.Log("[UIManager] 게임오버 UI 화면 표시 성공");
        }
    }

}
