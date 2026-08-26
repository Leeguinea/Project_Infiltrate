using UnityEngine;
using UnityEngine.AI;

public class EnemyChase : MonoBehaviour
{
    private EnemyController _controller;
    private NavMeshAgent _agent;

    private void Awake()
    {
        _controller = GetComponent<EnemyController>();
        _agent = GetComponent<NavMeshAgent>();
    }

    private void OnEnable()
    {
        //추격 시 ai
        if(_agent != null )
        {
            _agent.isStopped = false;
            _agent.speed = _controller.enemyData.speed;
        }
            
    }

    public void Chase()
    {
        if (_controller.player == null || _controller.enemyData == null) return;

        //플레이어를 향해 벽을 우회하는 장애물 길찾기 수행
        _agent.SetDestination(_controller.player.position);

        // 잡혔을 때 게임오버
        float distanceToPlayer = Vector3.Distance(transform.position, _controller.player.position);
        if (distanceToPlayer < 1.2f)
        {
            GameManager.Instance.TriggerGameOver();
        }
    }

    public void StopChase()
    {
        //추격 중단 시 ai도 정지
        if (_agent != null && _agent.isActiveAndEnabled)
        {
            _agent.isStopped = true;
            _agent.velocity = Vector3.zero;
        }
    }
}