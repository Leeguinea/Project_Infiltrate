using UnityEngine;

public class EnemySensor : MonoBehaviour
{
    [Header("레이어 설정")]
    [SerializeField] private LayerMask _playerMask;   // Player
    [SerializeField] private LayerMask _obstacleMask; // Wall, Interactable 등

    private EnemyController _controller;

    public bool IsPlayerInSight { get; private set; }

    private void Awake()
    {
        _controller = GetComponent<EnemyController>();
    }

    private void Update()
    {
        CheckForPlayerVisibilty();
    }

    private void CheckForPlayerVisibilty()
    {
        if (_controller.player == null || _controller.enemyData == null)
        {
            IsPlayerInSight = false;
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, _controller.player.position);

        // 1. 시야 거리 체크
        if (distanceToPlayer < _controller.enemyData.viewDistance)
        {
            Vector3 directionToPlayer = (_controller.player.position - transform.position);
            directionToPlayer.y = 0;
            directionToPlayer = directionToPlayer.normalized;

            // 2. 시야 각도 체크
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

            if (angleToPlayer < _controller.enemyData.viewAngle * 0.5f)
            {
                // 적 몸통보다 약간 앞(0.5f)에서 레이저 발사 (자기 자신 감지 방지)
                Vector3 rayOrigin = transform.position + Vector3.up * 1f + transform.forward * 0.5f;
                Vector3 targetPos = _controller.player.position + Vector3.up * 1f;
                Vector3 rayDirection = (targetPos - rayOrigin).normalized;
                float targetDistance = Vector3.Distance(rayOrigin, targetPos);

                LayerMask combinedMask = _playerMask | _obstacleMask;

                // 3. 레이캐스트 발사 (플레이어와 장애물 마스크만 체크)
                if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, targetDistance, combinedMask))
                {
                    // 가장 먼저 맞은 것이 Player인 경우에만 발각!
                    if (hit.collider.CompareTag("Player"))
                    {
                        IsPlayerInSight = true;
                        return;
                    }
                }
            }
        }

        IsPlayerInSight = false;
    }

    private void OnDrawGizmos()
    {
        if (_controller != null && _controller.player != null)
        {
            Gizmos.color = IsPlayerInSight ? Color.yellow : Color.blue;
            Gizmos.DrawLine(transform.position + Vector3.up * 1f, _controller.player.position + Vector3.up * 1f);
        }
    }
}