using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

//[공통] 시민 스크립트
public class NPCController : MonoBehaviour
{


    //시민의 평상시 행동 형태
    public enum NPCType
    {
        Idle,  //기본 행동 (이동x)
        Wander,  //주변 배회하기
        Dance   //춤추기
    }

    // NPC의 상태 (FSM)
    public enum NPCState
    {
        Ambient, //평상시 (평화로움)
        Panic, //범죄 목격 후 경직/비명
        Flee //플레이어 반대 방향으로 도망
    }

    [Header("NPC 설정")]
    [SerializeField] private NPCType _npcType = NPCType.Wander;
    [SerializeField] private NPCState _currentState = NPCState.Ambient;

    [Header("이동 및 센서 연결")]
    [SerializeField] private float _fleeSpeed = 5.0f;
    [SerializeField] private float _panicDuration = 2.0f; // 놀라는 시간 (2초 후 도망)

    [Header("범죄 감지 설정")]
    [SerializeField] private float _bodyDetectRadius = 8.0f;          // 기절한 적 감지 거리
    [SerializeField] private string _unconsciousTag = "Unconscious";  // 나중에 변수명을 인스펙터에서 수정 가능
    [SerializeField] private LayerMask _detectableMask; //npc의 감지 레이저가 무시하지 않고, 걸러내야할 대상

    private NavMeshAgent _agent;
    private Animator _animator;
    private VisionSensor _sensor;
    private PlayerController _player;

    private float _panicTimer = 0f;
    private bool _isKnockedOut = false; // 암살/제압 당함 여부

    public NPCState CurrentState => _currentState;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _sensor = GetComponent<VisionSensor>();
    }


    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if(playerObj != null)
        {
            _player = playerObj.GetComponent<PlayerController>();
        }

        // 초기 상태 세팅
        SetState(NPCState.Ambient);
    }

    private void Update()
    {
        if (_isKnockedOut) return;

        //범죄(ex. 시체 운반) 감지 체크
        CheckCrimeDetection();

        // 상태별 메인 로직 실행
        switch (_currentState)
        {
            case NPCState.Ambient:      //평상시(평화)
                HandleAmbientState();
                break;

            case NPCState.Panic:        //공포
                HandlePanicState();
                break;

            case NPCState.Flee:         //도망
                HandleFleeState();
                break;
        }
    }


    //범죄 행위(ex. 시체운반) 목격 여부 확인
    private void CheckCrimeDetection()
    {
        if (_currentState != NPCState.Ambient) return;

        //디버깅 
        Debug.Log($"[NPC 진단] 센서: {_sensor != null} | 감지: {(_sensor != null && _sensor.IsPlayerInSight)} | 플레이어존재: {_player != null} | 시체들었음: {(_player != null && _player.IsCarryingBody)}");

        // 센서 시야 내에 플레이어가 있고, 플레이어가 시체를 끌고 있다면?
        if (_sensor != null && _sensor.IsPlayerInSight)
        {
            if (_player != null && _player.IsCarryingBody)
            {
                // 즉시 Panic(패닉) 상태로 전환!
                SetState(NPCState.Panic);
                return;
            }
        }

        // 바닥에 쓰러진 적(Unconscious 태그)이 시야 내에 있는 경우
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, _bodyDetectRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag(_unconsciousTag))
            {
                Vector3 directionToBody = (hitCollider.transform.position - transform.position).normalized;
                float angle = Vector3.Angle(transform.forward, directionToBody);

                // NPC 시야 각도 내(전방 90도 범위 = 중앙 기준 45도 이내)에 쓰러진 적이 있는지 체크
                if (angle < 45f)
                {
                    // 장애물(벽 등)에 가려지지 않았는지 레이캐스트 확인
                    Vector3 rayOrigin = transform.position + Vector3.up * 1f;
                    Vector3 targetPos = hitCollider.transform.position + Vector3.up * 0.2f; // 쓰러진 적 위치 (바닥 근처)

                    if (Physics.Raycast(rayOrigin, (targetPos - rayOrigin).normalized, out RaycastHit hit, _bodyDetectRadius,_detectableMask))
                    {
                        if (hit.collider.CompareTag(_unconsciousTag))
                        {
                            SetState(NPCState.Panic);
                            break;
                        }
                    }
                }
            }
        }
    }




    //상태 변경 및 애니메이션 파라미터 세팅
    public void SetState(NPCState newState)
    {
        _currentState = newState;

        switch (newState)
        {
            case NPCState.Ambient :
                if (_agent != null) 
                    _agent.isStopped = false;
                break;

            case NPCState.Panic :

                _panicTimer = 0f;

                //NavMeshAgent 멈춤 처리
                if (_agent != null && _agent.isActiveAndEnabled)
                {
                    _agent.isStopped = true; //제자리에 멈춤
                }

                //애니메이션
                if(_animator != null)
                {
                    _animator.SetTrigger("OnPanic");
                    Debug.Log($"[{gameObject.name}] : 시민이 시체 발견! ");
                }
                break;

            case NPCState.Flee :
                
                //NavMesh
                if(_agent != null && _agent.isActiveAndEnabled)
                {
                    _agent.isStopped = false;
                    _agent.speed = _fleeSpeed;
                }

                //애니메이션
                if (_animator != null)
                    _animator.SetBool("IsFleeing", true);
                break;
        }
    }


    // 상태별 메인 로직 실행
    //1. 평상시 상태 로직
    private void HandleAmbientState()
    {
        if (_npcType == NPCType.Idle)
        {
            if (_agent != null && _agent.isActiveAndEnabled)
            {
                _agent.isStopped = true;
            }
        }
        else if (_npcType == NPCType.Wander)
        {
            //TODO: 배회 로직 구현 예정
        }
    }


    //2. 패닉(놀람) 상태 로직
    private void HandlePanicState()
    {
        _panicTimer += Time.deltaTime;

        //일정 시간 지나고 도망
        if(_panicTimer >= _panicDuration)
        {
            SetState(NPCState.Flee);
        }
    }


    //3. 도망 상태 로직
    private void HandleFleeState()
    {
        if (_player == null || _agent == null) return;

        //플레이어 반대 방향 벡터 계산 후 도망
        Vector3 runDirection = transform.position - _player.transform.position;
        Vector3 fleeTarget = transform.position + runDirection.normalized * 10f;

        NavMeshHit hit;
        if(NavMesh.SamplePosition(fleeTarget, out hit, 5f, NavMesh.AllAreas))
        {
            _agent.SetDestination(hit.position);
        }
    }


}
