using UnityEngine;

public class EnemyChase : MonoBehaviour
{
    private EnemyController _controller;
    [SerializeField] private LayerMask _obstacleMask; // 벽 감지용 레이어

    private void Awake()
    {
        _controller = GetComponent<EnemyController>();
    }

    public void Chase()
    {
        if (_controller.player == null || _controller.enemyData == null) return;

        Vector3 direction = _controller.player.position - transform.position;
        direction.y = 0;

        Vector3 normalizedDirection = direction.normalized;

        if (normalizedDirection != Vector3.zero)
        {
            transform.forward = normalizedDirection;
        }

        //  전방 0.6m 안에 Wall(장애물)이 있는지 체크
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
        bool isBlockedByWall = Physics.Raycast(rayOrigin, normalizedDirection, 0.6f, _obstacleMask);

        // 벽에 막히지 않았을 때만 이동 진행 (벽 뚫기 방지)
        if (!isBlockedByWall)
        {
            transform.Translate(normalizedDirection * _controller.enemyData.speed * Time.deltaTime, Space.World);
        }

        // 잡혔을 때 게임오버
        float distanceToPlayer = Vector3.Distance(transform.position, _controller.player.position);

        if (distanceToPlayer < 1.2f)
        {
            GameManager.Instance.TriggerGameOver();
        }
    }

    public void StopChase()
    {
    }
}