using UnityEngine;
using TMPro;
public class NotebookUI : MonoBehaviour
{
    [Header("UI 구성요소")]
    [SerializeField] private GameObject _notebookPanel;  // 수첩 전체 패널
    [SerializeField] private TextMeshProUGUI _clue1Text; // 단서 1 (외형)
    [SerializeField] private TextMeshProUGUI _clue2Text; // 단서 2 (습관)
    [SerializeField] private TextMeshProUGUI _clue3Text; // 단서 3 (위치/지역)

    [Header("사운드 피드백")]
    [SerializeField] private AudioSource _audioSource; 
    [SerializeField] private AudioClip _openSound;  //메모장 열때
    [SerializeField] private AudioClip _closeSound; //메모장 닫을 떄

    private bool _isOpen = false;

    private void Start()
    {
        //시작할 때는 UI 닫기
        if(_notebookPanel != null)
        {
            _notebookPanel.SetActive(false);
        }
        _isOpen = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleNoteBook();    
        }
    }

    public void ToggleNoteBook()
    {
        _isOpen = !_isOpen;

        if (_notebookPanel != null)
        {
            _notebookPanel.SetActive(_isOpen);
        }

        // 사운드 재생
        if (_audioSource != null)
        {
            AudioClip clipToPlay = _isOpen ? _openSound : _closeSound;
            if (clipToPlay != null)
            {
                _audioSource.PlayOneShot(clipToPlay);
            }
        }

        // 수첩이 열릴 때 단서 정보 갱신
        if (_isOpen)
        {
            RefreshClueTexts();
        }
    }


    // TargetGenerator에서 생성된 타겟의 단서를 불러와 텍스트 출력
    public void RefreshClueTexts()
    {
        // TargetGenerator에서 타겟 단서 가져오기
        if(TargetGenerator.Instance != null && TargetGenerator.Instance.targetNPC != null)
        {
            ClueSet clue = TargetGenerator.Instance.currentTargetClue;

            // NotebookUI.cs - RefreshClueTexts() 내 임시 포맷
            if (_clue1Text != null) _clue1Text.text = $"- Appearance: {clue.appearance}";
            if (_clue2Text != null) _clue2Text.text = $"- Habit: {clue.habit}";
            if (_clue3Text != null) _clue3Text.text = $"- Location: {clue.location}";
        }
        // 데이터가 없을 경우 기본 텍스트
        else
        {
            if (_clue1Text != null) _clue1Text.text = "- Appearance: No Info";
            if (_clue2Text != null) _clue2Text.text = "- Habit: No Info";
            if (_clue3Text != null) _clue3Text.text = "- Location: No Info";
        }
    }

}
