using UnityEngine;
//깃데스크
public class PlayerController : MonoBehaviour
{
    [Header("이동설정")]
    public float moveSpeed = 5f;

    private CharacterController characterController;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {

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
}
