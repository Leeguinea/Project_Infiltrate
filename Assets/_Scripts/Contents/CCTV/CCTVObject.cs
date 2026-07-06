using UnityEngine;

//좌우 회전로직
//시야각 안에 플레이어가 있으면 발각 && 회전 정지

public class CCTVObject : MonoBehaviour
{
    [Header("감시 설정")]
    [SerializeField] private float _viewAngle = 90f; // 시야각
    [SerializeField] private float _viewDistance = 7.0f; //최대 감시거리(Raycast_레이저 쏘는 거리)

    [Header("레이어 설정")]
    [SerializeField] private LayerMask _targetAndObstacleMask; //.검사할 레이어(플레이어 + Obstacle)

    private Transform _playerTransform; //감지된 플레이어의 위치를 기억할 상자
    private bool _isPlayerInSight = false; //플레이어가 시야 안에 있는지의 여부

    void Update()
    {
        CheckPlayerVisibility();    
    }

    //플레이어가 영역 안에 들어오면
    private void OnTriggerEnter(Collider other)
    {
        //태그 == 플레이어
        if(other.CompareTag("Player"))
        {
            _playerTransform = other.transform;
            Debug.Log("CCTV 침입자가 진입했습니다!");
        }
    }

    //플레이어가 영역(센서)에서 나가면
    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            _playerTransform = null; //플레이어 위치 삭제
            _isPlayerInSight = false;
            Debug.Log("침입자 없음");
        }
    }

    //거리에 들어온 플레이어가 정면 '시야각' 안에도 있는지 계산하는 함수
    //Raycast(레이저)
    private void CheckPlayerVisibility()
    {
        if (_playerTransform == null) return;

        // CCTV에서 플레이어의 방향
        Vector3 directionToPlayer = (_playerTransform.position - transform.position).normalized;

        // CCTV 정면 방향 ~ 플레이어 방향 사이의 '사이 각도' 
        float angleBetween = Vector3.Angle(transform.forward, directionToPlayer);

        //시야각/2 <= '사이 각도'안에 들어왔다면? (=> cctv 시야에 노출)
        //Raycast 적용
        if (angleBetween <= _viewAngle * 0.5f)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, directionToPlayer, out hit, _viewDistance, _targetAndObstacleMask))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    if (!_isPlayerInSight)
                    {
                        _isPlayerInSight = true;
                        Debug.Log("플레이어가 발각!!");
                    }
                }

                //레이저에 가장 먼저 도달한게 Obstacle 레이어이라면?
                else
                {
                    if (_isPlayerInSight)
                    {
                        _isPlayerInSight = false;
                        Debug.Log("플레이어가 시야각 안에 있지만, 장애물 뒤에 엄폐되어 발각안됨");
                    }
                }
            }
        }
        //시야각 안에 들어오지 않은 상태
        else
        {
            if (_isPlayerInSight)
            {
                _isPlayerInSight = false;
                Debug.Log("플레이어가 CCTV의 사각지대로 이동했습니다.");
            }
        }
    }

    private void OnDrawGizmos()
    {
        if(_playerTransform != null)
        {
            Gizmos.color = _isPlayerInSight ? Color.red : Color.green;
            Gizmos.DrawLine(transform.position, _playerTransform.position);
        }
    }


}
