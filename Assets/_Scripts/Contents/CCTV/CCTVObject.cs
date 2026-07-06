using UnityEngine;

//좌우 회전로직
//시야각 안에 플레이어가 있으면 발각 && 회전 정지

public class CCTVObject : MonoBehaviour
{
    [Header("감시 설정")]
    [SerializeField] private float _viewAngle = 90f; // 시야각

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
    private void CheckPlayerVisibility()
    {
        if (_playerTransform == null) return;

        // CCTV에서 플레이어의 방향
        Vector3 directionToPlayer = (_playerTransform.position - transform.position).normalized;

        // CCTV 정면 방향 ~ 플레이어 방향 사이의 '사이 각도' 
        float angleBetween = Vector3.Angle(transform.forward, directionToPlayer);

        //시야각/2 <= '사이 각도' 
        if(angleBetween <= _viewAngle * 0.5f)
        {
            //시야각 안으로 처음 들어왔을 때만 로그처리
            if(!_isPlayerInSight)
            {
                _isPlayerInSight = true;
                Debug.Log("[경고] 플레이어가 CCTV의 부채꼴 시야에 노출되었습니다.");
            }
        }
        else
        {
            if(_isPlayerInSight)
            {
                _isPlayerInSight = false;
                Debug.Log("감지 해제");
            }
        }

    }


}
