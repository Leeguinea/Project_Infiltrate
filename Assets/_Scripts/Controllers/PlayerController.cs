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


    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        Crouch();

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

}
