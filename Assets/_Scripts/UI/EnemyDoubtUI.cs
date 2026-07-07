using UnityEngine;
using UnityEngine.UI;

// 플레이어 머리 위에 고정된 전용 UI (적의 의심지수를 UI로)
public class EnemyDoubtUI : MonoBehaviour
{

    [Header("의심 UI 내부 요소 연결")]
    [SerializeField] private GameObject _doubtPanel;
    [SerializeField] private Image _circularGaugeImage;
    [SerializeField] private Image _eyeIconImage;

    private Transform _mainCameraTransform;

    private void Awake()
    {
        if(_doubtPanel != null)
        {
            _doubtPanel.SetActive(false);
        }
    }

    private void Start()
    {
        if(Camera.main != null)
        {
            _mainCameraTransform = Camera.main.transform;
        }
    }

    // UI가 카메라 각도 상관없이 항상 카메라를 정면으로 바라보게 함
    private void LateUpdate()
    {
        if (_mainCameraTransform != null && _doubtPanel != null && _doubtPanel.activeSelf)
        {
            transform.LookAt(transform.position + _mainCameraTransform.forward);
        }
    }

    // 적(EnemyController)들이 매 프레임 이 함수를 호출하여 수치를 전달
    public void UpdateDoubtProgress(float currentDoubt, float maxDoubt)
    {
        if (_doubtPanel == null || _circularGaugeImage == null || _eyeIconImage == null) return;

        if (currentDoubt > 0f)
        {
            // 의심 수치가 존재하면 UI 활성화
            if (!_doubtPanel.activeSelf) _doubtPanel.SetActive(true);

            // 게이지 양 계산 (0.0 ~ 1.0)
            float ratio = currentDoubt / maxDoubt;
            _circularGaugeImage.fillAmount = ratio;

            // 실시간 색상 보간
            Color targetColor = Color.Lerp(Color.yellow, Color.red, ratio);
            _circularGaugeImage.color = targetColor;

            // 눈동자 아이콘도 의심 수치가 올라갈수록 점점 진해지게 투명도(Alpha) 조절
            _eyeIconImage.color = new Color(targetColor.r, targetColor.g, targetColor.b, 0.4f + (ratio * 0.6f));
        }
        else
        {
            // 의심 수치가 0이 되면 UI를 깔끔하게 비활성화
            if (_doubtPanel.activeSelf) _doubtPanel.SetActive(false);
        }
}

    


    
}
