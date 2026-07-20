using UnityEngine;

public class EnemySensor : MonoBehaviour
{
    private EnemyController _controller;

    [SerializeField] private LayerMask _targetAndObstacleMask; // 플레이어 및 장애물 레이어

    private bool _isPlayerInSight = false; //c초기값
    public bool IsPlayerInSight { get; private set; } //EnemyController가 참조

    private Transform _playerTransform;

    private void Awake()
    {
        _controller = GetComponent<EnemyController>();
    }

    private void Update()
    {
        CheckForPlayerVisibilty();
    }

    // 시야 체크(플레이어 적발 기준) + 레이캐스트(장애물)
    private void CheckForPlayerVisibilty()
    {
        if (_controller.player == null || _controller.enemyData == null)
        {
            IsPlayerInSight = false;
            return;
        }

        // enemy와 player 거리
        float distanceToPlayer = Vector3.Distance(transform.position, _controller.player.position);

        //플레이어 시야에 들어옴.
        if (distanceToPlayer < _controller.enemyData.viewDistance)
        {
            Vector3 directionToPlayer = (_controller.player.position - transform.position).normalized;
            directionToPlayer.y = 0; // 평면상의 각도만 계산하기 위해 y무시

            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

            if (angleToPlayer < _controller.enemyData.viewAngle * 0.5f)
            {
                RaycastHit hit;
                Vector3 rayOrigin = transform.position + Vector3.up * 1f; //레이저 출발점
                Vector3 rayDirection = (_controller.player.position + Vector3.up * 1f - rayOrigin).normalized; //레이저 방향

                if (Physics.Raycast(rayOrigin, rayDirection, out hit, _controller.enemyData.viewDistance, _targetAndObstacleMask))
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        IsPlayerInSight = true;
                        return;
                    }
                }
            }
        }

        //위의 모든 검사(거리, 각도, 레이캐스트)를 통과하지 못했을 때만 플레이어를 놓친 것으로 판단
        IsPlayerInSight = false;

    }

    // [Trigger] 트리거 영역 센서 진입
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerTransform = other.transform;
        }
    }

    // [Trigger] 트리거 영역 센서 이탈
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerTransform = null;
            _isPlayerInSight = false;
        }
    }

    //기즈모 (시야체크용)
    private void OnDrawGizmos()
    {
        if (_playerTransform != null)
        {
            Gizmos.color = _isPlayerInSight ? Color.yellow : Color.blue;
            Gizmos.DrawLine(transform.position, _playerTransform.position);
        }
    }


}
