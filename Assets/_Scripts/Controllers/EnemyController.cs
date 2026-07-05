using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public Transform[] waypoints; //웨이포인트 리스트
    public Transform player; // 감시 대상
    public float viewDistance = 5.0f; //시야 거리
    public float viewAngle = 90.0f; //시야각 
    public float patrolSpeed = 2.8f;
    public float chaseSpeed = 3.5f;

    private int _currentWaypointIndex = 0; //초기 웨이포인트
    private enum EnemyState { Patrol, Chase }
    private EnemyState _currentState = EnemyState.Patrol; //기본값

    void Update()
    {
        if(GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            return;
        }

        CheckForPlayer(); //플레이어 적발
        
        switch (_currentState)
        {
            case EnemyState.Patrol:
                Patrol();
                break;

            case EnemyState.Chase:
                Chase();
                break;
        }
           
    }

    //순찰 
    //1. 웨이포인트 
    void Patrol()
    {
        if (waypoints.Length == 0) return;

        // 현재 목적지의 위치 좌표
        Vector3 targetPositions = waypoints[_currentWaypointIndex].position;

        // 움직일 방향 (목적지 - 현재 위치)
        Vector3 direction = targetPositions - transform.position;

        targetPositions.y = transform.position.y;
        direction = targetPositions - transform.position;

        transform.Translate(direction.normalized * patrolSpeed * Time.deltaTime, Space.World);

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


    //플레이어 적발
    void CheckForPlayer()
    {
        if (player == null) return;

        //emeny와 player 거리
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        //시야거리 내 플레이어가 들어왔다면?
        if(distanceToPlayer < viewDistance)  
        {
            Vector3 directionToPlayer = player.position - transform.position;
            directionToPlayer.y = 0; //평면상의 각도만 계산하기 위해 y무시

            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

            //시야각의 절반보다 작다면?
            if(angleToPlayer < viewAngle * 0.5f) 
            {
                Debug.Log("플레이어 적발!");

                _currentState = EnemyState.Chase;
            }
           
        }
    }

    //추적
    void Chase()
    {
        if (player == null) return;

        Vector3 direction = player.position - transform.position;
        direction.y = 0;

        Vector3 normorlizedDirection = direction.normalized;

        if(normorlizedDirection != Vector3.zero)
        {
            transform.forward = normorlizedDirection;
        }

        transform.Translate(normorlizedDirection * chaseSpeed * Time.deltaTime, Space.World);

        //잡혔을 때 게임오버
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if(distanceToPlayer < 1.2f)
        {
            GameManager.Instance.TriggerGameOver();
        }
    }

}
