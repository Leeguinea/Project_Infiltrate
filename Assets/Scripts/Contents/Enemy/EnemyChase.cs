using UnityEngine;

public class EnemyChase : MonoBehaviour
{
    private EnemyController _controller;

    private void Awake()
    {
        _controller = GetComponent<EnemyController>();
    }

    // [상태3] 추적 Chase
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

        transform.Translate(normalizedDirection * _controller.enemyData.speed * Time.deltaTime, Space.World);

        // 잡혔을 때 게임오버
        float distanceToPlayer = Vector3.Distance(transform.position, _controller.player.position);

        if (distanceToPlayer < 1.2f)
        {
            GameManager.Instance.TriggerGameOver();
        }
    }

   
}
