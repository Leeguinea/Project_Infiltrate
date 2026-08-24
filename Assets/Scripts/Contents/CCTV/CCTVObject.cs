using UnityEngine;
using System.Collections;

//좌우 회전로직
//시야각 안에 플레이어가 있으면 발각 && 회전 정지

public class CCTVObject : MonoBehaviour
{
    [Header("감시 설정")]
    [SerializeField] private float _viewAngle = 90f; // 시야각
    [SerializeField] private float _viewDistance = 7.0f; //최대 감시거리(Raycast_레이저 쏘는 거리)
    [SerializeField] private GameObject _cctvMonitorUI; // Raw Image가 있는 부모 UI (CCTV 모니터)
    [SerializeField] private float _uiDisableDelay = 1.0f; //모니터 꺼질 때까지의 지연시간(초)

    [Header("발각 지연 설정")]
    [SerializeField] private float _detectionDelay = 3.0f;  //n초 동안 노출되어야 적 호출
    private float _detectionTimer = 0f;
    private bool _hasCalledEnemy = false; //적을 호출했는지 여부

    [Header("레이어 설정")]
    [SerializeField] private LayerMask _targetAndObstacleMask; //.검사할 레이어(Player , Obstacle, Unconscious가)

    private Transform _targetTransform; //감지된 플레이어의 위치를 기억할 상자
    private bool _isTargetInSight = false; //플레이어가 시야 안에 있는지의 여부

    private Coroutine _turnOffDelayTimer; //예약 명령을 담는 타이머
    private EnemyController _assignedEnemy = null;

    void Update()
    {
        CheckTargetVisibility();    // 타겟(플레이어/시체) 시야 감지

        if (_isTargetInSight)
        {
            if (!_hasCalledEnemy)
            {
                _detectionTimer += Time.deltaTime;

                // 설정한 시간을 넘기는 순간 호출
                if (_detectionTimer >= _detectionDelay)
                {
                    _hasCalledEnemy = true; // 중복 호출 방지용
                    Debug.Log(" cctv 발각, 경비원을 호출합니다!");
                    CallClosestEnemy(); // 가장 가까운 경비원 호출
                }
            }
        }
        else
        {
            // 플레이어가 숨거나 사각지대로 벗어나면 리셋.
            if (!_hasCalledEnemy)
            {
                _detectionTimer = 0f;
            }
        }

        HandleMonitorUI();
    }

    //트리거 영역에 플레이어 또는 기절한 적이 진입했을 때
    private void OnTriggerEnter(Collider other)
    {
        //태그 == 플레이어
        if(other.CompareTag("Player") || other.CompareTag("Unconscious") || other.transform.root.CompareTag("Unconscious"))
        {
            _targetTransform = other.transform;
            Debug.Log($"CCTV 감지 영역에 감지 대상 [{other.name} / Tag: {other.tag}] 진입");

        }
    }

    //감지 영역에서 나갔을 때
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Unconscious") || other.transform.root.CompareTag("Unconscious"))
        {
            if (_targetTransform != null && (other.transform == _targetTransform || other.transform.root == _targetTransform.root))
            {
                _targetTransform = null;
                _isTargetInSight = false;
                _assignedEnemy = null;

                _detectionTimer = 0f;
                _hasCalledEnemy = false;
                Debug.Log("CCTV 영역에서 감지 대상이 벗어남");
            }
        }
    }

    // 시야각 및 Raycast 감지 함수
    private void CheckTargetVisibility()
    {
        if (_targetTransform == null) return;

        // CCTV에서 플레이어의 방향
        Vector3 directionToPlayer = (_targetTransform.position - transform.position).normalized;

        // CCTV 정면 방향 ~ 플레이어 방향 사이의 '사이 각도' 
        float angleBetween = Vector3.Angle(transform.forward, directionToPlayer);

        //시야각/2 <= '사이 각도'안에 들어왔다면? (=> cctv 시야에 노출)
        //Raycast 적용
        if (angleBetween <= _viewAngle * 0.5f)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, directionToPlayer, out hit, _viewDistance, _targetAndObstacleMask))
            {
                if (hit.collider.CompareTag("Player") || hit.collider.CompareTag("Unconscious") || hit.collider.transform.root.CompareTag("Unconscious"))
                {
                    if (!_isTargetInSight)
                    {
                        _isTargetInSight = true;
                        Debug.Log("플레이어가 발각!!");
                    }
                }

                //레이저에 가장 먼저 도달한게 Obstacle 레이어이라면?
                else
                {
                    if (_isTargetInSight)
                    {
                        _isTargetInSight = false;
                        _assignedEnemy = null;
                        Debug.Log("안들킴. 플레이어가 시야각 안에 있지만, 장애물 뒤에 엄폐되어 발각안됨");
                    }
                }
            }
        }
        //시야각 안에 들어오지 않은 상태
        else
        {
            if (_isTargetInSight)
            {
                _isTargetInSight = false;
                Debug.Log("플레이어가 CCTV의 사각지대로 이동했습니다.");
            }
        }
    }

    private void CallClosestEnemy()
    {
        //TODO: 더 효율적인 게 있다면 수정
        EnemyController[] allEnemies = FindObjectsByType<EnemyController>();

        //TODO: 일단 최소 거리는 무한대로 (나중에 일정거리로 제한 할 수도 있음)
        if (allEnemies.Length == 0) return;
        
        EnemyController closestEnemy = null;
        float shortestDistance = Mathf.Infinity;

        // CCTV 자신의 위치(transform.position) 기준 가장 가까운 적을 연산
        foreach (EnemyController enemy in allEnemies)
        {
            // 기절한 적("Unconscious")은 경보 출동 대상에서 제외
            if (enemy.CompareTag("Unconscious") || enemy.transform.root.CompareTag("Unconscious"))
            {
                continue;
            }

            // 스크립트가 꺼져 있거나 오브젝트가 비활성화된 적 제외
            if (!enemy.enabled || !enemy.gameObject.activeInHierarchy)
            {
                continue;
            }

            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);

            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                closestEnemy = enemy;

            }
        }

        // 찾아낸 단 한 마리의 적에게만 추격 명령을 전달.
        if (closestEnemy != null)
        {
            _assignedEnemy = closestEnemy;
            Debug.Log($"[CCTV 포착] 가장 가까운 경비원 [{_assignedEnemy.name}]을 즉시 출동시킵니다.");
            _assignedEnemy.CCTVCommandChase(); // EnemyController 내부의 무전 수신 함수 호출
        }
        // 출동할 수 있는 적이 없을 때 예외 로그 출력
        else
        {
            Debug.LogWarning("[CCTV 경보] 근처에 출동 가능한 살아있는 경비원이 없습니다!");
        }

    }


    private void OnDrawGizmos()
    {
        if(_targetTransform != null)
        {
            Gizmos.color = _isTargetInSight ? Color.red : Color.green;
            Gizmos.DrawLine(transform.position, _targetTransform.position);
        }
    }

    //CCTV 모니터 UI를 On/Off하는 함수
    //코루틴을 이용해서 시야밖을 벗어나도 천천히 꺼지게 만듦
    private void HandleMonitorUI()
    {
        if (_cctvMonitorUI == null) return;

        //플레이어가 시야각 안이라면?
        if(_isTargetInSight)
        {
            //코루틴(타이머) 취소
            if(_turnOffDelayTimer != null)
            {
                StopCoroutine(_turnOffDelayTimer);
                _turnOffDelayTimer = null;
            }

            _cctvMonitorUI.SetActive(true);
        }
        //플레이어가 시야각을 벗어나면 -> 코루틴으로 UI를 n초 뒤에 off
        else
        {
            if(_cctvMonitorUI.activeSelf && _turnOffDelayTimer == null)
            {
                _turnOffDelayTimer = StartCoroutine(DisableUIDelayed());
            }
        }
    }

    //코루틴 함수 (지정된 시간만큼 기다렸다가 ui를 off)
    private IEnumerator DisableUIDelayed()
    {
        yield return new WaitForSeconds(_uiDisableDelay);

        _cctvMonitorUI.SetActive(false);
        _turnOffDelayTimer = null; // 타이머 상자 비우기
    }
}

