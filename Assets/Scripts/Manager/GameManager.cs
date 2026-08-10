using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set;}
    
    public enum GameState { Playing, GameOver, Victory }
    public bool IsGameOver => _currentState == GameState.GameOver;

    private GameState _currentState = GameState.Playing;

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 게임 오버
    /// </summary>
    public void TriggerGameOver()
    {
        if (_currentState == GameState.GameOver) return;
        
        _currentState = GameState.GameOver;
        Debug.Log("게임오버");

        //UI 매니저
        if( UIManager.Instance != null )
        {
            UIManager.Instance.ShowGameOverUI();
        }
        else
        {
            Debug.LogWarning("[GameManager] UIManager를 찾을 수 없습니다! 씬에 할당했는지 확인하세요.");
        }

        Time.timeScale = 0f;

        //게임오버 UI, 사운드재생 등 코드 
    }
}
