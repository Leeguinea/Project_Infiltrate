using UnityEngine;
using UnityEngine.AI;

public class NPCController : MonoBehaviour
{
    public enum NPCState { Ambient, Panic, Flee }

    [Header("이동 및 패닉 설정")]
    [SerializeField] private float _fleeSpeed = 5.0f;
    [SerializeField] private float _panicDuration = 1.5f; // 패닉 유지 시간 (초)
    [SerializeField] private float _fleeDistance = 15.0f;  // 도망칠 랜덤 거리

    [Header("범죄 감지 설정")]
    [SerializeField] private float _bodyDetectRadius = 8.0f;
    [SerializeField] private string _unconsciousTag = "Unconscious";

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
        CheckCrime();

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
                if (_animator) _animator.SetBool("IsFleeing", false);
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
}