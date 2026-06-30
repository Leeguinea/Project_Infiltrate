using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public Transform[] waypoints; //웨이포인트 리스트

    private int _currentWaypointIndex = 0;

    void Update()
    {
        Patrol();        
    }

    void Patrol()
    {
        if(waypoints.Length == 0) return;

        //현재 위치
        Vector3 targetPositions = waypoints[_currentWaypointIndex].position;

        //움직일 방향 (목적지 - 현재 위치)
        Vector3 direction = targetPositions - transform.position;
        direction.y = 0;

        float speed = 4.0f;
        transform.Translate(direction.normalized * speed * Time.deltaTime, Space.World);

        //next
    }
    
}
