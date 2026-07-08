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

    private int _currentWaypointIndex = 0; // 초기 웨이포인트

    private enum EnemyState { Patrol, Chase, Doubt }
    private EnemyState _currentState = EnemyState.Patrol; // 기본값

    private float _currentDoubtValue = 0f; //의심지수 (0~100)
    private bool _isPlayerInSight = false;
    private Transform _playerTransform;

    private PlayerController _playerController;

    void Start()
    {
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

        switch (_currentState)
        {
            case EnemyState.Patrol:
                Patrol();
                break;

            case EnemyState.Doubt:
                LookAtPlayer(); //추척하지 않고, 자리에 멈춰 플레이어 주시
                break;

            case EnemyState.Chase:
                Chase();
                break;
            
        }
    }

    // [상태1] 순찰 Patrol
    // 1. 웨이포인트 
    private void Patrol()
    {
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
            _currentWaypointIndex++;

            if (_currentWaypointIndex >= waypoints.Length)
                _currentWaypointIndex = 0;
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
                if (Physics.Raycast(transform.position, directionToPlayer, out hit, enemyData.viewDistance, _targetAndObstacleMask))
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        if (_playerController != null)
                        {
                            //아직 추격(Chase) 상태가 아니라면?
                            if (_currentState != EnemyState.Chase)
                            {
                                // 못 본 척 넘어가야 하므로 false
                                _isPlayerInSight = false;
                                
                            }
                        }
                    }
                }
            }
        }

        _isPlayerInSight = false;
    }


    // 의심 게이지 계산 및 상태 머신 흐름 통제 
    private void HandleDoubtGauge()
    {
        //이미 들켜서 추적 중일 때는 의심 계산 안함
        if (_currentState == EnemyState.Chase) return;

        // 시야에 있으면?
        if (_isPlayerInSight)
        {
            _currentState = EnemyState.Doubt;
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
                _currentState = EnemyState.Patrol;
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
        _currentState = EnemyState.Chase;
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

    private void OnDrawGizmos()
    {
        if (_playerTransform != null)
        {
            Gizmos.color = _isPlayerInSight ? Color.yellow : Color.blue;
            Gizmos.DrawLine(transform.position, _playerTransform.position);
        }
    }

   

}