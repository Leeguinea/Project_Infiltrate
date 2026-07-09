using UnityEngine;

/*public enum 
{
    None,       //숨지 않음 (평상시)

}
*/

public class PlayerController : MonoBehaviour
{
    private CharacterController characterController;

    [Header("이동설정")]
    public float moveSpeed = 5f;

    [Header("앉기/서기 설정")]
    [SerializeField] private float _normalHeight = 2f;
    [SerializeField] private float _crouchHeight = 1f;

    [Header("벽 엄폐(cover) 설정")]
    [SerializeField] private LayerMask _wallMask; //인스펙터에서 'Wall' 레이어 변수
    [SerializeField] private float _maxCoverDistance = 1.5f; //벽을 감지할 최대거리
    
    private bool _isCoverd = false; //현재 벽에 붙은 상태인지
    private Vector3 _wallNormal;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        Crouch();

        if(Input.GetKeyDown(KeyCode.Space))
        {
            ToggleCover();
        }

        //만약 벽에 엄폐중이라면 좌우(a,d)만 가능
        if (_isCoverd)
        {
            MoveAlongWall();
            return;
        }


        //키보드 입력 받기
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        
        //이동 방향 계산
        Vector3 moveDirection = new Vector3(moveX, 0f, moveZ).normalized;

        //입력이 있을 때만 이동 및 회전 처리
        if(moveDirection.magnitude > 0.1f)
        {
            transform.forward = moveDirection;

            characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
        }
        
    }

    // C 버튼을 누르면 앉기 + 서기 상태로 변경됨 (물리적 높이를 바꿈)
    private void Crouch()
    {
        if(Input.GetKeyDown(KeyCode.C))
        {
            //서 있는 상태(컨트롤러 높이가 원래 높이와 비슷하다면) -> 앉은 상태 
            if(characterController.height  > _crouchHeight + 0.1f)
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

    //스페이스바를 누르면 벽에 엄폐
    private void ToggleCover()
    {
        //만약 이미 벽에 엄폐중이라면? ->염폐 해제
        if(_isCoverd)
        {
            _isCoverd = false;
            Debug.Log("벽 어폐 해제!");
            return;
        }

        //평상 시 상태라면 -> 플레이어 앞 방향으로 레이저를 쏘아 벽이 있는지 검사
        RaycastHit hit;

        //가슴 높이에서 ray
        Vector3 rayOrigin = transform.position + Vector3.up * 1f;

        if(Physics.Raycast(rayOrigin, transform.forward, out hit, _maxCoverDistance, _wallMask))
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

    //벽에 은폐중일 때 좌우로만 움직이게 하는 함수
    private void MoveAlongWall()
    {
        //좌우(수평)만 가능
        float hInput = Input.GetAxisRaw("Horizontal");

        //입력이 엇으면 계산없이 멈춤
        if (Mathf.Abs(hInput) < 0.1f) return;

        //하늘 방향과 벽이 정면 방향을 외적하여 "벽의 오른쪽 방향 벡터"추출
        Vector3 wallRight = Vector3.Cross(_wallNormal, Vector3.up).normalized;

        //wallRight: 오른쪽을 누르면 +1, 왼쪽을 누르면 -1
        Vector3 moveDirection = wallRight * hInput;

        float coverMoveSpeed = moveSpeed * 0.5f;
        characterController.Move(moveDirection * coverMoveSpeed * Time.deltaTime);

    }


}
