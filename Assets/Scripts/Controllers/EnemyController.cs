using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private PlayerController _playerController;
    public Transform player;

    [Header("컴포넌트 연결")]
    [SerializeField] public EnemyStateManager _stateManager;
    [SerializeField] private EnemySensor _enemySensor;
    [SerializeField] private EnemyPatrol _enemyPatrol;
    [SerializeField] private EnemyChase _enemyChase;
    [SerializeField] private EnemyStealthAction _enemySA;

    [Header("정의된 기획 데이터 에셋")]
    public EnemyData enemyData;

    [Header("의심 시스템 설정")]
    [SerializeField] private float _maxdoubtValue = 100f; // 최대 의심 수치
    [SerializeField] private float _increaseSpeed = 50f; // 시야에 있을 때 초당 게이지 상승량 (2초면 풀)
    [SerializeField] private float _decreaseSpeed = 30f; // 시야에서 벗어났을 때 초당 게이지 감소량

    [Header("추격 포기 및 대기 설정")]
    [SerializeField] private float _loseSightDuration = 3f; // 시야에서 사라진 후 추격을 포기할 때까지 시간
    [SerializeField] private float _waitDuration = 3f; //추격을 멈추고 현장에서 대기하는 시간 
    private float _loseSightTimer = 0f;
    private float _waitTimer = 0f;

    [Header("자식 UI 연결")]
    [SerializeField] private EnemyDoubtUI _myDoubtUI;
    [SerializeField] private GameObject _surpriseUI;

    public GameObject SurpriseUI => _surpriseUI;
    public float SurpriseTimer { get => _surpriseTimer; set => _surpriseTimer = value; }


    [Header("경직 시스템 설정")]
    [SerializeField] private float _surpriseDuration = 3.0f; // 경직 시간
    private float _surpriseTimer = 0f; // 경직 누적 타이머
    private float _currentDoubtValue = 0f; //의심지수 (0~100)

    [Header("상호작용 UI")]
    [SerializeField] private GameObject _actionPromptCanvas; //Enemy > ActionPromptCanvas 연결용


    [Header("시체 운반용 컴포넌트")]
    private Rigidbody _enemyRigidbody;

    void Awake()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }

    void Start()
    {
        _stateManager = GetComponent<EnemyStateManager>();
        if (_stateManager == null)
        {
            Debug.LogError("[{gameObject.name}에 EnemyStateManager 컴포넌트가 없음!]");
        }

        if (player != null)
        {
            _playerController = player.GetComponent<PlayerController>();
            
        }
    }


    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            return;
        }

        
        // 결과는 센서의 프로퍼티로 확인
        bool isSeen = _enemySensor.IsPlayerInSight;

        HandleDoubtGauge(isSeen);     // 결과를 바탕으로 상태 및 게이지 계산 

        switch (_stateManager.CurrentState)
        {
            case EnemyStateManager.EnemyState.Patrol:
                _enemyPatrol.Patrol();
                break;

            case EnemyStateManager.EnemyState.Doubt:
                LookAtPlayer(); //추척하지 않고, 자리에 멈춰 플레이어 주시
                break;

            case EnemyStateManager.EnemyState.Surprise:
                HandleSurpriseState();
                break;

            //단독 _enemyChase.Chase() 대신 시야 확인 및 타이머가 포함된 함수로 변경[나중 주석 삭제]
            case EnemyStateManager.EnemyState.Chase:
                HandleChaseState(isSeen);
                break;

            //추격을 포기한 후 그 자리에서 몇 초간 대기하는 상태 처리
            case EnemyStateManager.EnemyState.Wait:
                HandleWaitState(isSeen);
                break;

        }
    }

    // 시야에서 벗어났을 때 추격 포기 시간을 계산하는 함수
    private void HandleChaseState(bool isSeen)
    {
        //시야에 있으면
        if(isSeen)
        {
            // 플레이어가 눈에 보이면 타이머 초기화 후 추격 계속 진행
            _loseSightTimer = 0f;
            _enemyChase.Chase();
        }
        // 시야에서 벗어나면
        else
        {
            /// 타이머 누적하며 추격 진행 (마지막 본 위치 이동 등)
            _loseSightTimer += Time.deltaTime;
            _enemyChase.Chase();

            // 설정한 시간(_loseSightDuration) 동안 놓치면 Wait(대기) 상태로 전환
            if (_loseSightTimer >= _loseSightDuration)
            {
                _loseSightTimer = 0f;
                _waitTimer = 0f;

                // 추격 동작 멈춤 처리 (NavMeshAgent 정지)
                _enemyChase.StopChase();
                _stateManager.ChangeState(EnemyStateManager.EnemyState.Wait);
            }
        }

    }


    //  놓친 장소에서 대기 후 순찰로 복귀하는 함수
    private void HandleWaitState(bool isSeen)
    {
        // 대기 중이라도 플레이어를 다시 발견하면 즉시 재추격
        if (isSeen)
        {
            _waitTimer = 0f;
            _stateManager.ChangeState(EnemyStateManager.EnemyState.Chase);
            return;
        }

        _waitTimer += Time.deltaTime;

        // 지정된 대기 시간(_waitDuration)이 지나면 의심도 리셋 후 순찰로 복귀
        if (_waitTimer >= _waitDuration)
        {
            _waitTimer = 0f;
            _currentDoubtValue = 0f;
            UpdateDoubtUI();

            _stateManager.ChangeState(EnemyStateManager.EnemyState.Patrol);
        }
    }


    // CCTV가 경비원(Enemy, 나) 지목해서 호출할 때 실행되는 수신 함수
    // 외부(CCTVObject)에서 호출
    public void CCTVCommandChase()
    {
        // 현재 경비원이 이미 추격(Chase) 중이 아니라면?
        if (_stateManager.CurrentState != EnemyStateManager.EnemyState.Chase && _stateManager.CurrentState != EnemyStateManager.EnemyState.Surprise)
        {
            Debug.Log($"[{name}]: CCTV 무전을 받았다! 엇?! 무슨 일이지? (n초간 정지)");

            _surpriseTimer = 0f;
            _stateManager.ChangeState(EnemyStateManager.EnemyState.Surprise);

            //느낌표 UI
            if (_surpriseUI != null)
            {
                _surpriseUI.SetActive(true);
            }
        }
    }


    // cctv 호출을 받고 경직되는 시간을 재는 함수
    private void HandleSurpriseState()
    {
        _surpriseTimer += Time.deltaTime;

        // n초가 지나면?
        if (_surpriseTimer >= _surpriseDuration)
        {
            Debug.Log($"[{name}]: 침입자를 추격한다!");
            _surpriseTimer = 0f; // 타이머 초기화
            _stateManager.ChangeState(EnemyStateManager.EnemyState.Chase);

            if(_surpriseUI != null)
            {
                _surpriseUI.SetActive(false);
            }
        }
    }


    // 의심 게이지 계산 및 상태 머신 흐름 통제 
    private void HandleDoubtGauge(bool isPlayerInSight)
    {
        if (_stateManager == null) return;

        // Chase, Surprise 외에도 Wait(대기) 상태일 때 의심 게이지 계산을 스킵하도록 예외 추가
        if (_stateManager.CurrentState == EnemyStateManager.EnemyState.Chase || _stateManager.CurrentState == EnemyStateManager.EnemyState.Surprise || _stateManager.CurrentState == EnemyStateManager.EnemyState.Wait) 
            return;

        // 시야에 있으면?
        if (isPlayerInSight)
        {
            _stateManager.ChangeState(EnemyStateManager.EnemyState.Doubt);
            _currentDoubtValue += _increaseSpeed * Time.deltaTime;

            //현재 의심지수가 최대 의심지수와 같거나 크면?
            if (_currentDoubtValue >= _maxdoubtValue)
            {
                _currentDoubtValue = _maxdoubtValue;
                TriggerAlert(); //발각, 추적 시작함.
            }
        }
        // 시야에 없으면?
        else
        {
            _currentDoubtValue -= _decreaseSpeed * Time.deltaTime;
            if (_currentDoubtValue <= 0f)
            {
                _currentDoubtValue = 0f;
                _stateManager.ChangeState(EnemyStateManager.EnemyState.Patrol);
            }
        }

        // UI와 연동 
        UpdateDoubtUI();
    }


    // 의심 상태일 때 플레이어를 제자리에 서서 플레이어를 바라보는 로직
    private void LookAtPlayer()
    {
        if(player == null) return;

        Vector3 direction = (player.position - transform.position);
        direction.y = 0;

        if(direction != Vector3.zero)
        {
            transform.forward = direction.normalized;
        }
    }

    // 의심 게이지 UI 갱신 담당
    private void UpdateDoubtUI()
    {
        if (_myDoubtUI != null)
        {
            _myDoubtUI.UpdateDoubtProgress(_currentDoubtValue, _maxdoubtValue);
        }
    }

    // 의심 지수 100% 도달한다면?
    private void TriggerAlert()
    {
        Debug.Log("발각!경비원이 침입자를 완전히 알아챔!");
        _stateManager.ChangeState(EnemyStateManager.EnemyState.Chase);
        //TODO (Frisk든 뭐든)
    }

    

    // 플레이어가 암살 범위에 들어오면 UI를 켜고 끄는 함수
    public void ToggleActionPrompt(bool isActive)
    {
        Debug.Log($"[UI 디버그] 대상: {gameObject.name}, 상태: {isActive}"); // 명령이 가는지 확인

        if (_actionPromptCanvas != null)
        {
            _actionPromptCanvas.SetActive(isActive);
            Debug.Log($"[UI 디버그] UI 상태 변경 성공: {_actionPromptCanvas.activeSelf}"); // 진짜 켜졌는지 확인
        }
        else
        {
            Debug.LogError("[UI 디버그] _actionPromptCanvas 변수가 비어있습니다! 인스펙터를 확인하세요!");
        }
    }

    //// EnemyController.cs 안에 추가해 둘 수 있는 중계(3)
    public void TakeAssassination()
    {
        if (_enemySA != null)
        {
            _enemySA.TakeAssassination();
        }
    }

    public void CarryBody(Transform playerTransform)
    {
        if (_enemySA != null)
        {
            _enemySA.CarryBody(playerTransform);
        }
    }

    public void DropBody()
    {
        if (_enemySA != null)
        {
            _enemySA.DropBody();
        }
    }
}