using UnityEngine;

// [공용] 눈이 달린 모든 캐릭터(적, 시민, 경비원 등)에 붙이는 시야 센서
// 플레이어가 지정한 시야 거리에 있는지 판단 (IsPlayerInSight (true/false) 값을 구함)

public class VisionSensor : MonoBehaviour
{
    [Header("시야 기본 설정")]
    [SerializeField] private float _viewDistance = 8f;
    [SerializeField] private float _viewAngle = 90f;

    [Header("레이어 설정")]
    [SerializeField] private LayerMask _playerMask;   // Player
    [SerializeField] private LayerMask _obstacleMask; // Wall, Interactable 등

    private Transform _playerTransform;

    // 외부(EnemyController 등)에서 데이터에 따라 시야 값을 동적으로 바꿔줄 수 있는 함수
    public void SetVisionStats(float distance, float angle)
    {
        _viewDistance = distance;
        _viewAngle = angle;
    }

    public bool IsPlayerInSight { get; private set; }
    // 외부(EnemyController 등)에서 데이터에 따라 시야 값을 동적으로 바꿔줄 수 있는 함수

    public void Start()
    {
        FindPlayerTarget();
    }


    private void Update()
    {
        if(_playerTransform == null)
        {
            FindPlayerTarget();
        }

        CheckForPlayerVisibilty();
    }


    private void FindPlayerTarget()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _playerTransform = playerObj.transform;
        }
    }

    private void CheckForPlayerVisibilty()
    {
        if (_playerTransform == null)
        {
            IsPlayerInSight = false;
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, _playerTransform.position);

        // 1. 시야 거리 체크
        if (distanceToPlayer < _viewDistance)
        {
            Vector3 directionToPlayer = (_playerTransform.position - transform.position);
            directionToPlayer.y = 0;
            directionToPlayer = directionToPlayer.normalized;

            // 2. 시야 각도 체크
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

            if (angleToPlayer < _viewAngle * 0.5f)
            {
                // 적 몸통보다 약간 앞(0.5f)에서 레이저 발사 (자기 자신 감지 방지)
                Vector3 rayOrigin = transform.position + Vector3.up * 1f + transform.forward * 0.5f;
                Vector3 targetPos = _playerTransform.position + Vector3.up * 1f;
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
        if (_playerTransform != null)
        {
            Gizmos.color = IsPlayerInSight ? Color.yellow : Color.blue;
            Gizmos.DrawLine(transform.position + Vector3.up * 1f, _playerTransform.position + Vector3.up * 1f);
        }
    }
}