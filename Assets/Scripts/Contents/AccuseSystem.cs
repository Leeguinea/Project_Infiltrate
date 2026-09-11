using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// 수첩 UI를 통해 얻은 단서를 기반으로 현장에서 범인을 직접 '식별하고 검거하는' 상호작용 시스템
// 컴포넌트는 Player에 적용함.
public class AccuseSystem : MonoBehaviour
{
    // AccuseSystem.Instance로 접근 가능하게 함
    public static AccuseSystem Instance { get; private set; }

    [Header("Proximity Settings")]
    [SerializeField] private float _interactionRadius = 3.5f; // NPC 상호작용 가능 거리
    [SerializeField] private LayerMask _npcLayer; // NPC 레이어

    [Header("Camera Zoom Settings")]
    [SerializeField] private Transform _playerCamera;
    [SerializeField] private float _zoomSpeed = 5.0f; // 카메라 이동 속도
    [SerializeField] private Vector3 _cameraOffset = new Vector3(0.8f, 1.4f, 1.8f); // NPC 기준 카메라 포커스 위치

    [Header("UI Popup References")]
    [SerializeField] private GameObject _confirmPopupPanel;
    [SerializeField] private Button _yesButton;
    [SerializeField] private Button _noButton;

    [Header("Strike System")]
    [SerializeField] private int _maxStrikes = 2;
    private int _currentStrikes = 0;

    // 대화 상태 및 카메라 연출 변수
    private GameObject _currentTargetNPC;
    private Vector3 _savedCamLocalPos;
    private Quaternion _savedCamLocalRot;
    private bool _isInteracting = false;
    private Coroutine _cameraCoroutine;

    public bool IsInteracting => _isInteracting; // 외부 참조용 프로퍼티 (적 기절 스크립트 등에서 활용)


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    

    private void Start()
    {
        // 카메라 자동 할당
        if (_playerCamera == null)
        {
            if (Camera.main != null)
            {
                _playerCamera = Camera.main.transform;
            }
            else
            {
                Camera anyCam = FindAnyObjectByType<Camera>();
                if (anyCam != null)
                {
                    _playerCamera = anyCam.transform;
                }
            }
        }

        // 버튼 이벤트 연동
        if (_yesButton != null) 
            _yesButton.onClick.AddListener(OnYesClicked);

        if (_noButton != null) 
            _noButton.onClick.AddListener(OnNoClicked);

        // UI 초기화
        if (_confirmPopupPanel != null) 
            _confirmPopupPanel.SetActive(false);
    }



    private void Update()
    {
        // 상호작용 중이 아니고 팝업이 닫혀있을 때만 E키 감지
        if (!_isInteracting && (_confirmPopupPanel == null || !_confirmPopupPanel.activeSelf))
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                TryProximityInteract();
            }
        }
    }


    // 범위 내 가장 가까운 NPC 검색 및 상호작용 개시
    private void TryProximityInteract()
    {
        // 플레이어 주변 범위 내의 NPC 콜라이더 수색
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, _interactionRadius, _npcLayer);

        if (hitColliders.Length == 0)
        {
            Debug.Log("[AccuseSystem] 주변 상호작용 범위 내에 NPC가 없습니다.");
            return;
        }

        // 가장 가까운 NPC 탐색
        Collider closestNPCCollider = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider col in hitColliders)
        {
            float dist = Vector3.Distance(transform.position, col.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closestNPCCollider = col;
            }
        }

        if (closestNPCCollider != null)
        {
            _currentTargetNPC = closestNPCCollider.gameObject;
            StartInteraction();
        }
    }


    // 상호작용 시작 (카메라 줌인 & NPC 정지)
    private void StartInteraction()
    {
        _isInteracting = true;

        // NPC 멈춰 서서 플레이어 바라보게 설정
        NPCController npc = _currentTargetNPC.GetComponent<NPCController>();

        if (npc != null)
        {
            npc.StopAndLookAt(transform);
        }

        // 원래 카메라 위치/회전값 저장
        if (_playerCamera != null)
        {
            _savedCamLocalPos = _playerCamera.localPosition;
            _savedCamLocalRot = _playerCamera.localRotation;

            // 카메라 클로즈업 연출 시작
            if (_cameraCoroutine != null)
            {
                StopCoroutine(_cameraCoroutine);
            }

            // NPC를바라보는 목표 위치 계산
            Vector3 targetCamPos = _currentTargetNPC.transform.position + (_currentTargetNPC.transform.forward * _cameraOffset.z) + (Vector3.up * _cameraOffset.y) + (_currentTargetNPC.transform.right * _cameraOffset.x);
            Quaternion targetCamRot = Quaternion.LookRotation(_currentTargetNPC.transform.position + Vector3.up * 1.3f - targetCamPos);

            _cameraCoroutine = StartCoroutine(MoveCameraRoutine(targetCamPos, targetCamRot, true));
        }
        else
        {
            if (_confirmPopupPanel != null) _confirmPopupPanel.SetActive(true);
        }
    }

    // [YES] 지목한다 클릭 시
    private void OnYesClicked()
    {
        if (_confirmPopupPanel != null)
            _confirmPopupPanel.SetActive(false);

        if (_currentTargetNPC == null) 
            return;

        // 정답 검증
        if (TargetGenerator.Instance != null && TargetGenerator.Instance.targetNPC != null)
        {
            if (_currentTargetNPC == TargetGenerator.Instance.targetNPC.gameObject)
            {
                OnAccuseSuccess();
            }
            else
            {
                OnAccuseFail();
            }
        }
    }

    // [NO] 돌아선다 클릭 시
    private void OnNoClicked()
    {
        if (_confirmPopupPanel != null) _confirmPopupPanel.SetActive(false);

        if (_currentTargetNPC != null)
        {
            Debug.Log($"[NPC 대사] {_currentTargetNPC.name}: \"What is wrong with this guy...?\"");

            NPCController npc = _currentTargetNPC.GetComponent<NPCController>();
            if (npc != null) npc.ResumeMovement();
        }

        EndInteraction();
    }

    private void OnAccuseSuccess()
    {
        Debug.Log("<color=green>[AccuseSystem] 지목 성공! \"You took the Golden Duck, didn't you?!\" (Mission Complete)</color>");
        // TODO: 미션 성공 연출 호출
    }

    private void OnAccuseFail()
    {
        _currentStrikes++;
        Debug.LogWarning($"[AccuseSystem] 지목 실패! (스트라이크: {_currentStrikes}/{_maxStrikes})");

        if (_currentTargetNPC != null)
        {
            Debug.Log($"[NPC 대사] {_currentTargetNPC.name}: \"It wasn't me!\"");

            NPCController npc = _currentTargetNPC.GetComponent<NPCController>();
            if (npc != null) npc.ResumeMovement();
        }

        EndInteraction();

        if (_currentStrikes >= _maxStrikes)
        {
            OnGameOver();
        }
    }

    // 상호작용 종료 (카메라 원래 위치 복귀)
    private void EndInteraction()
    {
        if (_playerCamera != null)
        {
            if (_cameraCoroutine != null) StopCoroutine(_cameraCoroutine);

            // 카메라 원위치 복귀 (월드 좌표 전환)
            Vector3 worldOriginalPos = transform.TransformPoint(_savedCamLocalPos);
            Quaternion worldOriginalRot = transform.rotation * _savedCamLocalRot;

            _cameraCoroutine = StartCoroutine(MoveCameraRoutine(worldOriginalPos, worldOriginalRot, false));
        }
        else
        {
            _isInteracting = false;
            _currentTargetNPC = null;
        }
    }

    private void OnGameOver()
    {
        Debug.LogError("[AccuseSystem] 2-스트라이크 달성! 미션 실패 (Game Over)");
        // TODO: 실패 연출 호출
    }

    // 카메라 부드러운 이동 코루틴
    private IEnumerator MoveCameraRoutine(Vector3 targetPos, Quaternion targetRot, bool showUIAtEnd)
    {
        float t = 0f;
        Vector3 startPos = _playerCamera.position;
        Quaternion startRot = _playerCamera.rotation;

        while (t < 1.0f)
        {
            t += Time.deltaTime * _zoomSpeed;
            _playerCamera.position = Vector3.Lerp(startPos, targetPos, t);
            _playerCamera.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        _playerCamera.position = targetPos;
        _playerCamera.rotation = targetRot;

        if (showUIAtEnd)
        {
            if (_confirmPopupPanel != null) _confirmPopupPanel.SetActive(true);
        }
        else
        {
            // 부모 자식 관계 좌표 재정렬
            _playerCamera.localPosition = _savedCamLocalPos;
            _playerCamera.localRotation = _savedCamLocalRot;
            _isInteracting = false;
            _currentTargetNPC = null;
        }
    }

    // 에디터 씬 뷰에서 감지 범위 시각적 표시
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _interactionRadius);
    }
}