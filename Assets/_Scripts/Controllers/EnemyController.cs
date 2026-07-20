using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("정의된 기획 데이터 에셋")]
    public EnemyData enemyData;

    [Header("이동 및 감시 대상")]
    public Transform[] waypoints; // 웨이포인트 리스트
    public Transform player; // 감시 대상
    [SerializeField] private LayerMask _targetAndObstacleMask; // 플레이어 및 장애물 레이어

    [Header("의심 시스템 설정")]
    [SerializeField] private float _maxdoubtValue = 100f; // 최대 의심 수치
    [SerializeField] private float _increaseSpeed = 50f; // 시야에 있을 때 초당 게이지 상승량 (2초면 풀)
    [SerializeField] private float _decreaseSpeed = 30f; // 시야에서 벗어났을 때 초당 게이지 감소량
    
    [Header("자식 UI 연결")]
    [SerializeField] private EnemyDoubtUI _myDoubtUI;
    [SerializeField] private GameObject _surpriseUI;

    [Header("순찰 설정")]
    [SerializeField] private float _patrolWaitDuration = 5f;

    private int _currentWaypointIndex = 0; // 초기 웨이포인트
    private float _patrolWaitTimer = 0f; //대기 시간 타이머
    private bool _isWaitingAtWaypoint = false; //현재 멈춰서 대기중인가?

    [SerializeField] private EnemyStateManager _stateManager;

    [Header("경직 시스템 설정")]
    [SerializeField] private float _surpriseDuration = 3.0f; // 경직 시간
    private float _surpriseTimer = 0f; // 경직 누적 타이머

    private float _currentDoubtValue = 0f; //의심지수 (0~100)
    private bool _isPlayerInSight = false;
    private Transform _playerTransform;

    private PlayerController _playerController;

    [Header("상호작용 UI")]
    [SerializeField] private GameObject _actionPromptCanvas; //Enemy > ActionPromptCanvas 연결용


    [Header("시체 운반용 컴포넌트")]
    private Rigidbody _enemyRigidbody;

 
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

        CheckForPlayerVisibilty();  // 시야 체크 결과 생성(_isPlayerInSight)
        HandleDoubtGauge();         // 결과를 바탕으로 상태 및 게이지 계산 

        switch (_stateManager.CurrentState)
        {
            case EnemyStateManager.EnemyState.Patrol:
                Patrol();
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


    // [상태1] 순찰 Patrol
    // 1. 웨이포인트 
    private void Patrol()
    {
        //만약 웨이포인트에서 대기중인 상태라면?
        if(_isWaitingAtWaypoint)
        {
            _patrolWaitTimer += Time.deltaTime;

            //계획한 시간이 지나면?
            if(_patrolWaitTimer >= _patrolWaitDuration)
            {
                _isWaitingAtWaypoint = false;
                _patrolWaitTimer = 0f;

                _currentWaypointIndex++;
                if (_currentWaypointIndex >= waypoints.Length)
                {
                    _currentWaypointIndex = 0;
                }
            }

            return; //대기 중일 때는 아래의 순찰과정x
        }

        //순찰
        if (waypoints.Length == 0) return;

        if (enemyData == null)
        {
            Debug.LogError($"[{name}] EnemyData 에셋이 할당되지 않았습니다! 인스펙터를 확인해주세요.");
            return;
        }

        // 현재 목적지의 위치 좌표
        Vector3 targetPositions = waypoints[_currentWaypointIndex].position;

        // 움직일 방향 (목적지 - 현재 위치)
        Vector3 direction = targetPositions - transform.position;

        targetPositions.y = transform.position.y;
        direction = targetPositions - transform.position;

        transform.Translate(direction.normalized * enemyData.speed * Time.deltaTime, Space.World);

        //자연스럽게 앞을 보고 순찰하게함.
        if(direction != Vector3.zero)
        {
            transform.forward = direction.normalized;
        }

        // 웨이포인트와의 거리
        float distanceToTarget = Vector3.Distance(transform.position, targetPositions);

        // 도착 판정 범위
        if (distanceToTarget < 0.5f)
        {
            _isWaitingAtWaypoint = true;
            _patrolWaitTimer = 0f;

            Debug.Log($"[{name}]: 웨이포인트에 도착! {{_patrolWaitDuration}}초 동안 멈춰서 정찰합니다.");
        }
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


    // 시야 체크(플레이어 적발 기준) + 레이캐스트(장애물)
    private void CheckForPlayerVisibilty()
    {
        if (player == null || enemyData == null)
        {
            _isPlayerInSight = false;
            return;
        }

        // enemy와 player 거리
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        //플레이어 시야에 들어옴.
        if (distanceToPlayer < enemyData.viewDistance)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            directionToPlayer.y = 0; // 평면상의 각도만 계산하기 위해 y무시

            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

            if (angleToPlayer < enemyData.viewAngle * 0.5f)
            {
                RaycastHit hit;
                Vector3 rayOrigin = transform.position + Vector3.up * 1f; //레이저 출발점
                Vector3 rayDirection = (player.position + Vector3.up * 1f - rayOrigin).normalized; //레이저 방향

                if(Physics.Raycast(rayOrigin, rayDirection, out hit, enemyData.viewDistance, _targetAndObstacleMask))
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        _isPlayerInSight = true;
                        return;
                    }
                }
            }
        }

        //위의 모든 검사(거리, 각도, 레이캐스트)를 통과하지 못했을 때만 플레이어를 놓친 것으로 판단
        _isPlayerInSight = false;

    }


    // 의심 게이지 계산 및 상태 머신 흐름 통제 
    private void HandleDoubtGauge()
    {
        if (_stateManager == null) return;

        // 이미 추적 중 or 경직 상태면 HandleDoubtGauge()함수 패스
        if (_stateManager.CurrentState == EnemyStateManager.EnemyState.Chase || _stateManager.CurrentState == EnemyStateManager.EnemyState.Surprise) return;

        // 시야에 있으면?
        if (_isPlayerInSight)
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

    // [Trigger]
    // 트리거 영역 센서 진입
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerTransform = other.transform;
        }
    }

    // 트리거 영역 센서 이탈
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerTransform = null;
            _isPlayerInSight = false;
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
            if(_surpriseUI != null)
            {
                _surpriseUI.SetActive(true);
            }
        }
    }


    private void OnDrawGizmos()
    {
        if (_playerTransform != null)
        {
            Gizmos.color = _isPlayerInSight ? Color.yellow : Color.blue;
            Gizmos.DrawLine(transform.position, _playerTransform.position);
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