using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public Transform[] waypoints; //웨이포인트 리스트
    public Transform player; // 감시 대상
    public float viewDistance = 5.0f; //시야 거리
    public float veiwAngle = 90.0f; //시야각 

    private int _currentWaypointIndex = 0; //초기 웨이포인트


    void Update()
    {
        Patrol(); //순찰
        CheckForPlayer(); //플레이어 적발
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

        float speed = 4.0f;
        transform.Translate(direction.normalized * speed * Time.deltaTime, Space.World);

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

        if(distanceToPlayer < viewDistance)  //적발 기준
        {
            //각도계산
            Debug.Log("플레이어 적발!");
        }
    }

}
