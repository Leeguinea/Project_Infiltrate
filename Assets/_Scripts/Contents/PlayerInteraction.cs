using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("감지 설정")]
    [SerializeField] private float detectionRadius = 2.0f; // 상호작용 가능한 거리
    [SerializeField] private LayerMask interactionLayer; // 미션 오브젝트를 구별할 레이어

    private Interactable _currentInteractable; // 지금 내 앞에 있는 미션 오브젝트
    private float _interactionTimer = 0f; // 키를 누르고 있는 시간 체크용 타이머
    private bool _isInteracting = false; // 현재 상호작용 중인가?

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) Debug.Log("키보드 E 입력은 정상 작동 중!");

        CheckForInteractable();

        // 앞에 상호작용 오브젝트가 있을 때만 키 입력을 받습니다.
        if (_currentInteractable != null)
        {
            // E 키를 누르기 시작했을 때
            if (Input.GetKeyDown(KeyCode.E))
            {
                _isInteracting = true;
                _interactionTimer = 0f;
                Debug.Log($"{_currentInteractable.name} 상호작용 시작...");
            }

            // E 키를 꾹 누르고 있는 동안
            if (Input.GetKey(KeyCode.E) && _isInteracting)
            {
                _interactionTimer += Time.deltaTime;

                // [이후 구현 예정] UIManager를 통해 화면에 슬라이더 게이지
                Debug.Log($"진행률: {(_interactionTimer / _currentInteractable.RequiredInteractionTime) * 100f}%");

                // 정해진 시간을 다 채웠다면?
                if (_interactionTimer >= _currentInteractable.RequiredInteractionTime)
                {
                    _currentInteractable.OnInteractComplete();
                    ResetInteraction();
                }
            }
        }

        // 키에서 손을 떼거나 멀어지면 초기화
        if (Input.GetKeyUp(KeyCode.E))
        {
            ResetInteraction();
        }
    }

    // 주변에 상호작용 가능한 오브젝트가 있는지 구체(Sphere) 형태로 레이더 감지
    void CheckForInteractable()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius, interactionLayer);

        if (colliders.Length > 0)
        {
            // 가장 먼저 감지된 콜라이더에서 Interactable 컴포넌트를 추출합니다.
            Interactable interactable = colliders[0].GetComponent<Interactable>();

            if (interactable != _currentInteractable)
            {
                _currentInteractable = interactable;
                Debug.Log($"상호작용 가능 대상 발견: {_currentInteractable.interactionPrompt}");
            }
        }
        else
        {
            // 범위 내에 아무것도 없다면 초기화
            if (_currentInteractable != null) ResetInteraction();
            _currentInteractable = null;
        }
    }

    void ResetInteraction()
    {
        if (_isInteracting) 
            Debug.Log("상호작용 취소/종료");

        _isInteracting = false;
        _interactionTimer = 0f;
    }

    // 에디터 뷰에서 레이더 범위를 시각적으로 보기 위한 함수
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}