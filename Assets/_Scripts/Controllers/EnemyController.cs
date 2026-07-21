using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private PlayerController _playerController;
    public Transform player;

    [Header("컴포넌트 연결")]
    [SerializeField] public EnemyStateManager _stateManager;
    [SerializeField] private EnemySensor _enemySensor;
    [SerializeField] private EnemyPatrol _enemyPatrol;

    [Header("정의된 기획 데이터 에셋")]
    public EnemyData enemyData;

    [Header("의심 시스템 설정")]
    [SerializeField] private float _maxdoubtValue = 100f; // 최대 의심 수치
    [SerializeField] private float _increaseSpeed = 50f; // 시야에 있을 때 초당 게이지 상승량 (2초면 풀)
    [SerializeField] private float _decreaseSpeed = 30f; // 시야에서 벗어났을 때 초당 게이지 감소량
    
    [Header("자식 UI 연결")]
    [SerializeField] private EnemyDoubtUI _myDoubtUI;
    [SerializeField] private GameObject _surpriseUI;

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

            case EnemyStateManager.EnemyState.Chase:
                Chase();
                break;
            
        }
    }


    public void TakeAssassination()
    {
        Debug.Log($"[{name}]: 적이 뒤에서 기습당해 제압되었다!");

        //TODO: 애니메이션으로 교체하고 아래 코드 삭제
        transform.rotation = Quaternion.Euler(90f, transform.eulerAngles.y, transform.eulerAngles.z);
        transform.position = new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z);

        //이 스크립트 자체를 꺼버림
        this.enabled = false;

        if (_surpriseUI != null) _surpriseUI.SetActive(false);
    }

    //플레이어가 시체를 붙잡을 떄
    public void CarryBody(Transform playerTransform)
    {
        //플레이어의 자식으로 들어가게 함.
        transform.SetParent(playerTransform);
        transform.localPosition = new Vector3(0f, -0.5f, -0.8f);

        //정면이 하늘을 보게 함
        transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        //리지드바디 잠깐 꺼주기 (끌려다니는 동안 물리 충돌로 버벅거리지 않게 하려고)
        if(GetComponent<Rigidbody>() != null)
        {
            GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    //플레이어가 시체를 놓을 때
    public void DropBody()
    {
        transform.SetParent(null);

        if (GetComponent<Rigidbody>() != null)
        {
            GetComponent<Rigidbody>().isKinematic = false;
        }

        transform.position = new Vector3(transform.position.x, 0.1f, transform.position.z);
    }


    


    // [상태3] 추적 Chase
    private void Chase()
    {
        if (player == null || enemyData == null) return;

        Vector3 direction = player.position - transform.position;
        direction.y = 0;

        Vector3 normorlizedDirection = direction.normalized;

        if (normorlizedDirection != Vector3.zero)
        {
            transform.forward = normorlizedDirection;
        }

        transform.Translate(normorlizedDirection * enemyData.speed * Time.deltaTime, Space.World);

        // 잡혔을 때 게임오버
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer < 1.2f)
        {
            GameManager.Instance.TriggerGameOver();
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

        // 이미 추적 중 or 경직 상태면 HandleDoubtGauge()함수 패스
        if (_stateManager.CurrentState == EnemyStateManager.EnemyState.Chase || _stateManager.CurrentState == EnemyStateManager.EnemyState.Surprise) return;

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
            if(_surpriseUI != null)
            {
                _surpriseUI.SetActive(true);
            }
        }
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

}