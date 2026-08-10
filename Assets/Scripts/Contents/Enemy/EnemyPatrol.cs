using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    private EnemyController _controller;

    [Header("이동 및 감시 대상")]
    public Transform[] waypoints; // 웨이포인트 리스트

    [Header("순찰 설정")]
    [SerializeField] private float _patrolWaitDuration = 5f;

    private int _currentWaypointIndex = 0; // 초기 웨이포인트
    private float _patrolWaitTimer = 0f; // 대기 시간 타이머
    private bool _isWaitingAtWaypoint = false; // 현재 멈춰서 대기중인가?

    private void Awake()
    {
        _controller = GetComponent<EnemyController>();
    }

    // EnemyController에서 호출해 주는 함수
    public void Patrol()
    {
        // 순찰 로직 수행
        if (waypoints.Length == 0) return;

        if (_controller.enemyData == null)
        {
            Debug.LogError($"[{name}] EnemyData 에셋이 할당되지 않았습니다! 인스펙터를 확인해주세요.");
            return;
        }

        //만약 웨이포인트에서 대기중인 상태라면?
        if (_isWaitingAtWaypoint)
        {
            _patrolWaitTimer += Time.deltaTime;

            if (_patrolWaitTimer >= _patrolWaitDuration)
            {
                _isWaitingAtWaypoint = false;
                _patrolWaitTimer = 0f;

                _currentWaypointIndex++;
                if (_currentWaypointIndex >= waypoints.Length)
                {
                    _currentWaypointIndex = 0;
                }
            }
            return;
        }

        // 현재 목적지의 위치 좌표
        Vector3 targetPositions = waypoints[_currentWaypointIndex].position;
        targetPositions.y = transform.position.y;

        // 움직일 방향
        Vector3 direction = targetPositions - transform.position;

        transform.Translate(direction.normalized * _controller.enemyData.speed * Time.deltaTime, Space.World);

        // 앞을 보고 순찰
        if (direction != Vector3.zero)
        {
            transform.forward = direction.normalized;
        }

        // 웨이포인트와의 거리
        float distanceToTarget = Vector3.Distance(transform.position, targetPositions);

        if (distanceToTarget < 0.5f)
        {
            _isWaitingAtWaypoint = true;
            _patrolWaitTimer = 0f;
            Debug.Log($"[{name}]: 웨이포인트 도착! 대기 시작");
        }
    }
}