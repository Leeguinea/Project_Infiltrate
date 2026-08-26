using UnityEngine;
using System.Collections;

/*public enum 
{
    None,       //숨지 않음 (평상시)

}
*/

public class PlayerController : MonoBehaviour
{
    private CharacterController characterController;
    private Animator _animator; 

    [Header("이동설정")]
    public float moveSpeed = 5f;
    private bool _isAssassinating = false; // 암살 수행 중인지 여부

    [Header("앉기/서기 설정")]
    [SerializeField] private float _normalHeight = 2f;
    [SerializeField] private float _crouchHeight = 1f;

    [Header("벽 엄폐(cover) 설정")]
    [SerializeField] private LayerMask _wallMask; //인스펙터에서 'Wall' 레이어 변수
    [SerializeField] private float _maxCoverDistance = 1.5f; //벽을 감지할 최대거리

    private bool _isCoverd = false; //현재 벽에 붙은 상태인지
    private Vector3 _wallNormal;

    [Header("암살 설정")]
    [SerializeField] private float _assassinateDistance = 1.5f; //암살 가능 거리
    [SerializeField] private LayerMask _enemyLayer;

    private EnemyController _lastDetectedEnemy = null; // 직전 프레임에 감지했던 적 기억용 변수

    [Header("시체 운반 설정")]
    [SerializeField] private GameObject _bodyActionPrompt;
    [SerializeField] private float _bodyCheckDistance = 2.0f; //시체를 감지할 수 있는 거리
    [SerializeField] private bool _isCarryingBody = false; //현재 시체를 끌고 있는가?
    public bool IsCarryingBody => _isCarryingBody;
    private EnemyController _carryingEnemy = null; //내가 끌고 있는 그 시체 

    //private EnemyController _lastDetectedBody = null;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        _animator = GetComponentInChildren<Animator>();
    }

    
    //애니메이션, 이동 등
    void Update()
    {
        // 암살 애니메이션 수행 중일 때는 모든 이동/조작 차단
        if (_isAssassinating) return;

        Crouch();
        CheckForAssassination();
        HandleBodyCarry();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ToggleCover();
        }

        // 만약 벽에 엄폐중이라면 좌우(a,d)만 가능
        if (_isCoverd)
        {
            MoveAlongWall();
            return; // 엄폐 중일 때 아래쪽 평상시 이동이 같이 실행되지 않도록 차단
        }

        // 인벤토리가 켜져 있으면 플레이어 이동 로직을 아예 패스함!
        if (Inventory.isInventoryOpen)
        {
            if (_animator != null)
            {
                _animator.SetFloat("Speed", 0f);
                _animator.SetFloat("DragSpeed", 0f);
            }
            return;
        }

        // 키보드 입력 받기
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        // 이동 방향 계산
        Vector3 moveDirection = new Vector3(moveX, 0f, moveZ).normalized;
        bool isMoving = moveDirection.magnitude > 0.1f;

        // 달리기(왼쪽쉬프트)
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float runSpeed = moveSpeed * 1.8f;
        float currentSpeed = isRunning ? runSpeed : moveSpeed;

        if (_isCarryingBody)
        {
            currentSpeed = moveSpeed * 0.3f;
        }

        // 물리 이동 및 회전 처리
        if (isMoving)
        {
            Vector3 targetLookDirection = _isCarryingBody ? -moveDirection : moveDirection;
            Quaternion targetRotation = Quaternion.LookRotation(targetLookDirection);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            characterController.Move(moveDirection * currentSpeed * Time.deltaTime);
        }

        // 애니메이터 파라미터 제어 (일반 이동 / 시체 끌기 분리)
        if (_animator != null)
        {
            if (_isCarryingBody)
            {
                // 시체를 끌고 있는 상태: DragSpeed로 Drag_Idle(0) <-> Drag_Walk(1) 제어
                _animator.SetFloat("DragSpeed", isMoving ? 1.0f : 0f);
                _animator.SetFloat("Speed", 0f);
            }
            else
            {
                // 일반 이동 상태: Speed로 Idle(0) <-> Walk(1) <-> Run(2) 제어
                float animSpeedValue = isMoving ? (isRunning ? 2.0f : 1.0f) : 0f;
                _animator.SetFloat("Speed", animSpeedValue);
                _animator.SetFloat("DragSpeed", 0f);
            }
        }

        // 시체를 끌고 있는 중이라면 매 프레임 위치/회전 보정
        UpdateCarryingBodyPosition();
    }

    // 부드러운 위치/회전 추적 메서드
    private void UpdateCarryingBodyPosition()
    {
        if (!_isCarryingBody || _carryingEnemy == null) return;

        // 플레이어 정면 0.8f 위치 계산 (가슴 앞쪽 위치)
        Vector3 targetPos = transform.position + transform.forward * 0.8f;
        targetPos.y = transform.position.y - 0.2f; // 높이 조절

        // 1. 위치를 부드럽게 추적 (15f는 따라오는 속도)
        _carryingEnemy.transform.position = Vector3.Lerp(_carryingEnemy.transform.position, targetPos, Time.deltaTime * 15f);

        // 2. 회전을 부드럽게 추적 (10f는 회전 속도)
        Quaternion targetRotation = Quaternion.LookRotation(transform.forward) * Quaternion.Euler(90f, 0f, 0f);
        _carryingEnemy.transform.rotation = Quaternion.Slerp(_carryingEnemy.transform.rotation, targetRotation, Time.deltaTime * 15f);
    }



    // C 버튼을 누르면 앉기 + 서기 상태로 변경됨 (물리적 높이를 바꿈)
    private void Crouch()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            //서 있는 상태(컨트롤러 높이가 원래 높이와 비슷하다면) -> 앉은 상태 
            if (characterController.height > _crouchHeight + 0.1f)
            {
                characterController.height = _crouchHeight;
                characterController.center = new Vector3(0f, _crouchHeight * 0.5f, 0f);
            }
            //앉아있는 상태면 -> 서 있는 상태
            else
            {
                characterController.height = _normalHeight;
                characterController.center = new Vector3(0f, _normalHeight * 0.5f, 0f);
            }
        }
    }

    //1. 벽에 은폐중일 때 좌우로만 움직임
    //2. 벽 끝에 도달했다면 더이상 못감.
    private void MoveAlongWall()
    {
        float hInput = Input.GetAxisRaw("Horizontal");
        if (Mathf.Abs(hInput) < 0.1f) return;

        // 외적 공식으로 벽의 우측 방향 벡터 추출
        Vector3 wallRight = Vector3.Cross(_wallNormal, Vector3.up).normalized;
        Vector3 moveDirection = wallRight * hInput;

        RaycastHit edgeHit;

        // 내가 잠시 후 이동할 '조금 앞선 위치' 계산 (반지름 0.4f 만큼 앞)
        Vector3 futurePosition = transform.position + (moveDirection * 0.4f) + (Vector3.up * 1f);

        // 미래의 위치에서 벽 쪽을 향해 레이저를 쐈을 때 벽이 없다면? -> 이동하지 않고 리턴(멈춤)
        if (!Physics.Raycast(futurePosition, -_wallNormal, out edgeHit, 1.0f, _wallMask))
        {
            return;
        }

        // 벽이 아직 남아있는 안전한 경우에만 이동 처리
        float coverMoveSpeed = moveSpeed * 0.5f;
        characterController.Move(moveDirection * coverMoveSpeed * Time.deltaTime);

        // 엄폐 중 이동 시에도 애니메이션 전달
        if (_animator != null)
        {
            _animator.SetFloat("Speed", Mathf.Abs(hInput));
        }
    }

    //스페이스바를 누르면 벽에 엄폐
    private void ToggleCover()
    {
        //만약 이미 벽에 엄폐중이라면? ->염폐 해제
        if (_isCoverd)
        {
            _isCoverd = false;

            //엄폐 해제 시 벽에서 살짝 떨어짐
            Vector3 detachPosition = transform.position + _wallNormal * 0.3f;
            detachPosition.y = transform.position.y;
            transform.position = detachPosition;

            Debug.Log("벽 엄폐 해제!");
            return;
        }

        //평상 시 상태라면 -> 플레이어 앞 방향으로 레이저를 쏘아 벽이 있는지 검사
        RaycastHit hit;

        //가슴 높이에서 ray
        Vector3 rayOrigin = transform.position + Vector3.up * 1f;

        if (Physics.Raycast(rayOrigin, transform.forward, out hit, _maxCoverDistance, _wallMask))
        {
            _isCoverd = true;
            _wallNormal = hit.normal;
            Debug.Log($"벽 발견! 부딪힌 물체: {hit.collider.name}");

            //플레이어의 위치를 벽 바로 앞으로 붙기
            Vector3 targetPosition = hit.point + hit.normal * 0.4f;
            targetPosition.y = transform.position.y;
            transform.position = targetPosition;
            transform.forward = -hit.normal;

        }

    }

    //암살 가능한 타겟 찾기 및 UI 띄우기 
    private void CheckForAssassination()
    {
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, _assassinateDistance, _enemyLayer);

        EnemyController currentTargetEnemy = null;
        float closetDistance = float.MaxValue; // 가장 가까운 적(플레이어와의 거리가 가장 짧은 적)

        foreach (var enemyCollider in hitEnemies)
        {
            EnemyController enemy = enemyCollider.GetComponent<EnemyController>();
            if (enemy != null && enemy.enabled)
            {
                if (IsBehindEnemy(enemy))
                {
                    //발견된 적 중 플레이어와 가장 가까운 적 선별
                    float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position); //발견한 적과의 거리
                    if(distanceToEnemy < closetDistance)
                    {
                        closetDistance = distanceToEnemy; 
                        currentTargetEnemy = enemy; // 암살 가능한 대상을 찾음
                    }
                }
            }
        }

        // 감지 대상이 바뀌었을 때의 처리
        if (currentTargetEnemy != _lastDetectedEnemy)
        {
            // 1. 이전에 감지하던 적이 있다면 그 적의 UI를 off
            if (_lastDetectedEnemy != null)
            {
                _lastDetectedEnemy.ToggleActionPrompt(false);
            }

            // 2. 새로 감지된 적이 있다면 그 적의 UI를 on
            if (currentTargetEnemy != null)
            {
                currentTargetEnemy.ToggleActionPrompt(true);
            }

            _lastDetectedEnemy = currentTargetEnemy; // 현재 적 기억
        }

        // 3. E키 입력 처리
        if (currentTargetEnemy != null && Input.GetKeyDown(KeyCode.E))
        {
            //e키 입력시, 플레이어가 적을 향하도록 방향 조절
            Vector3 lookTarget = currentTargetEnemy.transform.position;
            lookTarget.y = transform.position.y;
            Vector3 lookDir = (lookTarget - transform.position).normalized;

            if(lookDir != Vector3.zero)
            {
                transform.forward = lookDir;
            }

            // 암살/제압 애니메이션 재생
            if (_animator != null)
            {
                _animator.SetTrigger("Attack");
                //Debug.Log("공격 트리거 발동됨!");
            }
            else
            {
                Debug.LogError("애니메이터를 찾지 못했습니다!");
            }

            currentTargetEnemy.ToggleActionPrompt(false); // UI 끄기
            currentTargetEnemy.TakeAssassination();       // 적 제압
            _lastDetectedEnemy = null;

            StartCoroutine(LockMovementForAssassination(2.0f));
        }
    }

    // 암살 애니메이션 시간 동안 플레이어 동작을 멈추는 코루틴
    private IEnumerator LockMovementForAssassination(float duration)
    {
        _isAssassinating = true;

        // 이동 애니메이션도 즉시 Idle(0)로 멈춤
        if (_animator != null)
        {
            _animator.SetFloat("Speed", 0f);
        }

        // 지정한 시간(애니메이션 길이)만큼 대기
        yield return new WaitForSeconds(duration);

        _isAssassinating = false;
    }


    //적의 뒤통수에 있는지 판별
    private bool IsBehindEnemy(EnemyController enemy)
    {
        Vector3 enemyForward = enemy.transform.forward;
        Vector3 dirToPlayer = (transform.position - enemy.transform.position).normalized;

        float angle = Vector3.Angle(enemyForward, dirToPlayer);

        //120도 보다 크다면 적의 뒤에 플레이어가 있다고 판단.
        return angle > 120f;
    }

    //죽은 시체 
    private void HandleBodyCarry()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");


        // 시체 잡기/놓기 키 (G 키)
        if (_isCarryingBody)
        {
            if ( Input.GetKeyDown(KeyCode.G))
            {
                if (_carryingEnemy != null)
                {
                    _carryingEnemy.DropBody();  
                }

                // 시체를 놓았으므로 bool 값을 false로 전환
                _carryingEnemy = null;
                _isCarryingBody = false;

                // 애니메이터에 시체를 놓았음을 전달
                if (_animator != null)
                {
                    _animator.SetBool("isCarrying", false);
                }
                Debug.Log("시체를 바닥에 놓았습니다.");
            }
            return; // 시체를 끌고 있을 때는 아래의 '새로운 시체 찾기'를 실행하지 않고 탈출
        }

        // [상태 2] 탐색
        EnemyController currentTarget = null;
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, _bodyCheckDistance, _enemyLayer);

        foreach (var hitCollider in hitColliders)
        {
            EnemyController enemy = hitCollider.GetComponent<EnemyController>();
            if (enemy != null && !enemy.enabled)
            {
                currentTarget = enemy;
                break;
            }
        }


        // 3. 입력
        if (currentTarget != null && Input.GetKeyDown(KeyCode.G))
        {
            _carryingEnemy = currentTarget;
            _isCarryingBody = true;

            // 플레이어가 시체(적)를 정면으로 바라보도록 회전 처리
            Vector3 lookTarget = currentTarget.transform.position;
            lookTarget.y = transform.position.y;
            Vector3 lookDir = (lookTarget - transform.position).normalized;

            if (lookDir != Vector3.zero)
            {
                transform.forward = lookDir;
            }

            if (_animator != null)
            {
                _animator.SetBool("isCarrying", true);
            }

            _carryingEnemy.CarryBody(transform);
            Debug.Log("시체를 옮기는 중입니다.");
        }
    }
}












