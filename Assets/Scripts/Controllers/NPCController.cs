using UnityEngine;
using UnityEngine.AI;

public class NPCController : MonoBehaviour
{
    public enum NPCState { Ambient, Panic, Flee }

    [Header("NPC 상태 정보")]
    public bool isTarget = false;
    public ClueSet assignedClue;

    [Header("외형 오브젝트 (유니티 Inspector에서 연결)")]
    public GameObject redHatObject;
    public GameObject pinkGlassesObject;
    public GameObject blueBagObject;
    public GameObject yellowShirtObject;

    [Header("이동 및 패닉 설정")]
    [SerializeField] private float _fleeSpeed = 5.0f;
    [SerializeField] private float _panicDuration = 1.5f; // 패닉 유지 시간 (초)
    [SerializeField] private float _fleeDistance = 15.0f;  // 도망칠 랜덤 거리

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

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _sensor = GetComponent<VisionSensor>();
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            _player = playerObj.GetComponent<PlayerController>();

        SetState(NPCState.Ambient);
    }

    private void Update()
    {
        // 범죄 감지 (Ambient 상태일 때 범죄를 보면 Panic으로 상태 변환)
        CheckCrime();
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
        }

        

        switch (_currentState)
        {
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
                //기존: if (_animator) _animator.SetBool("IsFleeing", false);
                if (_agent)
                {
                    _agent.isStopped = false;
                    _agent.speed = _wanderSpeed; // 걷기 속도로 설정
                }
                if (_animator) _animator.SetBool("IsFleeing", false);

                // 처음 시작할 때 바로 랜덤 목적지 설정
                SetRandomWanderDestination();
                break;

            case NPCState.Panic:
                if (_agent) _agent.isStopped = true; // 패닉 동안 멈춤
                if (_animator)
                {
                    _animator.SetBool("IsFleeing", false);
                    _animator.SetTrigger("OnPanic");
                }
                break;

            case NPCState.Flee:
                if (_agent)
                {
                    _agent.isStopped = false;
                    _agent.speed = _fleeSpeed;
                }
                if (_animator) _animator.SetBool("IsFleeing", true);

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
                SetState(NPCState.Panic);
                break;
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
}