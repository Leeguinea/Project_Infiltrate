using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NPCController : MonoBehaviour
{
    public enum NPCState { Ambient, Panic, Flee }   //평온, 패닉, 도망

    [Header("NPC 상태 정보")]
    public bool isTarget = false;
    public ClueSet assignedClue;

    [Header("외형 오브젝트 (유니티 Inspector에서 연결)")]
    public GameObject redHatObject;
    public GameObject pinkGlassesObject;
    public GameObject blueBagObject;
    public GameObject yellowShirtObject;

    [Header("시야 세부 설정")]
    [SerializeField] private float _viewAngle = 120.0f;          // NPC 시야각 (전방 120도)
    [SerializeField] private LayerMask _obstacleMask;            // 시야를 가리는 벽/장애물 레이어

    [Header("이동 및 패닉 설정")]
    [SerializeField] private float _fleeSpeed = 3.0f;
    [SerializeField] private float _panicDuration = 2.0f; // 패닉 유지 시간 (초)
    [SerializeField] private float _fleeDistance = 30.0f;  // 도망칠 랜덤 거리
    [SerializeField] private float _panicPropagateRadius = 6.0f; // 주변 NPC에게 패닉을 전파할 반경
    public NPCState CurrentState => _currentState;

    [Header("범죄 감지 설정")]
    [SerializeField] private float _bodyDetectRadius = 8.0f;
    [SerializeField] private string _unconsciousTag = "Unconscious";

    [Header("일상 배회 설정")]
    [SerializeField] private float _wanderSpeed = 2.0f;       // 걷는 속도 (도망 속도보다 느리게)
    [SerializeField] private float _wanderRadius = 15.0f;     // 배회 반경
    [SerializeField] private float _minWaitTime = 2.0f;       // 목적지 도착 후 최소 대기시간
    [SerializeField] private float _maxWaitTime = 5.0f;       // 목적지 도착 후 최대 대기시간

    private float _waitTimer = 0f;
    private float _currentWaitTime = 0f;
    private bool _isWaiting = false;

    private NavMeshAgent _agent;
    private Animator _animator;
    private VisionSensor _sensor;
    private PlayerController _player;

    private NPCState _currentState = NPCState.Ambient;
    private float _panicTimer = 0f;

    private bool _isPendingPanic = false; // 이미 패닉 대기 중인지 체크하는 플래그

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _sensor = GetComponent<VisionSensor>();
    }

    private void Start()
    {
        _player = FindAnyObjectByType<PlayerController>();

        SetState(NPCState.Ambient);
    }

    private void OnDisable()
    {
        _isPendingPanic = false;
    }

    private void Update()
    {
        // 범죄 감지 (Ambient 상태일 때 범죄를 보면 Panic으로 상태 변환)
        CheckCrime();

        switch (_currentState)
        {
            case NPCState.Ambient:
                // 목적지에 도착했는지 확인
                if (!_agent.pathPending && _agent.remainingDistance <= 0.5f)
                {
                    if (!_isWaiting)
                    {
                        // 도착하자마자 잠시 멍때리기(대기) 시작
                        _isWaiting = true;
                        _waitTimer = 0f;
                        _currentWaitTime = Random.Range(_minWaitTime, _maxWaitTime);
                        if (_animator) _animator.SetBool("IsWalking", false); // 걷기 애니메이션 끄기
                    }
                    else
                    {
                        // 대기 시간 타이머 계산
                        _waitTimer += Time.deltaTime;
                        if (_waitTimer >= _currentWaitTime)
                        {
                            // 대기 끝! 새로운 랜덤 위치로 이동 시작
                            _isWaiting = false;
                            SetRandomWanderDestination();
                        }
                    }
                }
                break;

            case NPCState.Panic:
                _panicTimer += Time.deltaTime;
                if (_panicTimer >= _panicDuration)
                {
                    SetState(NPCState.Flee);
                }
                break;

            case NPCState.Flee:
                // 목적지에 거의 다 왔으면 새로운 랜덤 위치 잡고 다시 뛰기
                if (!_agent.pathPending && _agent.remainingDistance <= 1.0f)
                {
                    SetRandomFleeDestination();
                }
                break;
        }
    }

    public void SetState(NPCState newState)
    {
        _currentState = newState;
        _panicTimer = 0f;

        switch (newState)
        {
            case NPCState.Ambient:
                if (_agent)
                {
                    _agent.isStopped = false;
                    _agent.speed = _wanderSpeed; // 걷기 속도로 설정
                }
                if (_animator)
                {
                    _animator.SetBool("IsWalking", false);
                    _animator.SetBool("IsFleeing", false);
                }

                SetRandomWanderDestination(); // 처음 시작할 때 바로 랜덤 목적지 설정

                break;

            case NPCState.Panic:
                if (_agent) 
                    _agent.isStopped = true; // 패닉 동안 멈춤

                if (_animator)
                {
                    _animator.SetBool("IsWalking", false);
                    _animator.SetBool("IsFleeing", false);
                    _animator.SetTrigger("OnPanic");       //AnyState ->Panic
                }

                PropagatePanic(); // 패닉 상태에 진입하자마자 주변 NPC 및 경비에게 연쇄 전파
                break;

            case NPCState.Flee:
                if (_agent)
                {
                    _agent.isStopped = false;
                    _agent.speed = _fleeSpeed;
                }
                if (_animator)
                {
                    _animator.SetBool("IsWalking", false);
                    _animator.SetBool("IsFleeing", true);
                }
                

                // 도망 시작할 때 딱 한 번만 목적지 찍기
                SetRandomFleeDestination();
                break;
        }
    }

    // 주변 NavMesh 상의 랜덤 위치를 목적지로 지정하는 함수
    private void SetRandomWanderDestination()
    {
        if (_agent == null || !_agent.isActiveAndEnabled) return;

        Vector3 randomDirection = Random.insideUnitSphere * _wanderRadius;
        randomDirection += transform.position;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, _wanderRadius, NavMesh.AllAreas))
        {
            _agent.SetDestination(hit.position);
            if (_animator) _animator.SetBool("IsWalking", true); // 걷기 애니메이션 켜기
        }
    }


    // 단순 랜덤 방향 목적지 설정
    private void SetRandomFleeDestination()
    {
        if (_agent == null || !_agent.isActiveAndEnabled) return;

        // NPC 중심으로 반경 내 랜덤 위치 추출
        Vector3 randomDirection = Random.insideUnitSphere * _fleeDistance;
        randomDirection += transform.position;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, _fleeDistance, NavMesh.AllAreas))
        {
            _agent.SetDestination(hit.position);
        }
    }

    // 기본 범죄 감지
    private void CheckCrime()
    {
        if (_currentState != NPCState.Ambient) return;

        // 1. 시체 업은 플레이어 감지
        if (_sensor != null && _sensor.IsPlayerInSight)
        {
            // 디버그용 로그 추가
            Debug.Log($"[NPC] 플레이어 시야 감지됨! / Player null 여부: {(_player == null)} / IsCarryingBody: {(_player != null && _player.IsCarryingBody)}");
            if (_player != null && _player.IsCarryingBody)
            {
                SetState(NPCState.Panic);
                return;
            }
        }

        // 2. 주변 시체 감지
        Collider[] hits = Physics.OverlapSphere(transform.position, _bodyDetectRadius);
        foreach (var col in hits)
        {
            if (col.CompareTag(_unconsciousTag))
            {
                // NPC 위치에서 시체 위치로 향하는 방향 계산
                Vector3 dirToBody = (col.transform.position - transform.position).normalized;

                //  NPC의 전방 바라보는 방향과 시체 방향 사이의 각도 계산 (시야각 내에 있는지)
                if (Vector3.Angle(transform.forward, dirToBody) < _viewAngle * 0.5f)
                {
                    float distToBody = Vector3.Distance(transform.position, col.transform.position);

                    //
                    // Raycast를 쏴서 NPC와 시체 사이에 벽(Obstacle)이 없는지 확인
                    // (NPC 눈높이인 Vector3.up * 1.5f 지점에서 레이 발사)
                    if (!Physics.Raycast(transform.position + Vector3.up * 1.5f, dirToBody, distToBody, _obstacleMask))
                    {
                        SetState(NPCState.Panic);
                        break;
                    }
                }
            }
        }
    }


    // 단서 시스템 연동: 단서 세트를 전달받아 NPC에게 적용
    /// TargetGenerator가 호출해 주는 함수

    public void ApplyClueSet(ClueSet clueSet, bool targetState)
    {
        assignedClue = clueSet;
        isTarget = targetState;

        ApplyAppearance(clueSet.appearance);
    }

    private void ApplyAppearance(AppearanceType type)
    {
        if (redHatObject) redHatObject.SetActive(type == AppearanceType.RedHat);
        if (pinkGlassesObject) pinkGlassesObject.SetActive(type == AppearanceType.PinkGlasses);
        if (blueBagObject) blueBagObject.SetActive(type == AppearanceType.BlueBag);
        if (yellowShirtObject) yellowShirtObject.SetActive(type == AppearanceType.YellowShirt);
    }

    // 연쇄 패닉
    //주변 NPC에게 비명을 지르고 패닉을 전파
    private void PropagatePanic()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, _panicPropagateRadius);
        foreach (var col in hits)
        {
            // 자기 자신 제외한 다른 일반 NPC 연쇄 패닉
            if (col.gameObject != gameObject && col.TryGetComponent<NPCController>(out var otherNPC))
            {
                if (otherNPC.CurrentState == NPCState.Ambient)
                {
                    // NPC와 나 사이에 벽이 있는지 검사 (벽이 없어야 비명이 들림)
                    Vector3 dirToOther = (otherNPC.transform.position - transform.position).normalized;
                    float distToOther = Vector3.Distance(transform.position, otherNPC.transform.position);

                    if (!Physics.Raycast(transform.position + Vector3.up * 1.5f, dirToOther, distToOther, _obstacleMask))
                    {
                        float soundDelay = distToOther * 0.03f; //거리가 멀수록 조금 더 늦게 들림. 
                        float reactionDelay = Random.Range(0.1f, 0.25f); //NPC 개개인의 반응 속도 차이 (0.1 ~ 0.25c초 사이의 무작위 값)
                        float totalDelay = soundDelay + reactionDelay;

                        //코루틴으로 시간 차
                        otherNPC.TriggerPanicWithDelay(totalDelay);
                    }
                }
            }

            // 주변에 경비(Enemy/Guard)가 있다면 경계 태세 유발
            if (col.CompareTag("Enemy"))
            {
                //TODO: 경비 ai와 연동
                Debug.Log($"[NPC] 주변 경비({col.name})에게 범죄 상황 인지시킴!");
            }
        }
    }
    
    //패닉 예약 함수
    //범죄 현장 발견한 npc가 panic인 상태를 목격한 다른 npc
    public void TriggerPanicWithDelay(float delay)
    {
        //이미 패닉 상태이거나, 패닉 대기 중이라면 중복 실행 방지
        if (CurrentState == NPCState.Ambient || _isPendingPanic) return;
        
        StartCoroutine(PanicRoutine(delay));
    }

    private IEnumerator PanicRoutine(float delay)
    {
        _isPendingPanic = true;

        yield return new WaitForSeconds(delay);
        //TODO: 이 시간 동안 '어??'하고 뭐지하는 느낌의 애니메이션을 넣기. 해당 방향으로 쳐다보기
        
        _isPendingPanic = false;
        SetState(NPCState.Panic);// 여기서 Panic이 되면서 이 NPC도 PropagatePanic()을 부르게 됨!
    }
}
