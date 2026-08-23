using UnityEngine;

public class EnemyStealthAction : MonoBehaviour
{
    private EnemyController _controller;
    private Collider _myCollider;

    private void Awake()
    {
        _controller = GetComponent<EnemyController>();
        _myCollider = GetComponent<Collider>();
    }

    //기습
    public void TakeAssassination()
    {
        Debug.Log($"[{name}]: 적이 뒤에서 기습당해 제압되었다!");

        //TODO: 애니메이션으로 교체하고 아래 코드 삭제
        transform.rotation = Quaternion.Euler(90f, transform.eulerAngles.y, transform.eulerAngles.z);
        transform.position = new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z);

        //경고 UI 끄기 (프로퍼티로 접근)
        if (_controller.SurpriseUI != null)
        {
            _controller.SurpriseUI.SetActive(false);
        }

        //적 ai 컨트롤러 전체 off해서 행동정지
        if(_controller != null)
        {
            //이 스크립트 자체를 꺼버림
            _controller.enabled = false;
        }

        this.enabled = false;
    }



    //플레이어가 시체를 붙잡을 떄
    public void CarryBody(Transform playerTransform)
    {
        // 플레이어와 물리적으로 부딪혀 밀쳐내는 것을 막기 위해 콜라이더 Off
        if (_myCollider != null)
        {
            _myCollider.enabled = false;
        }

        //플레이어의 자식으로 들어가게 함.
        //적이 누워있는 위치
        //transform.SetParent(playerTransform);
        ////transform.localPosition = new Vector3(0f, 0f, 1.0f);
        transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        //리지드바디 잠깐 꺼주기 (끌려다니는 동안 물리 충돌로 버벅거리지 않게 하려고)
        if (GetComponent<Rigidbody>() != null)
        {
            GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    //플레이어가 시체를 놓을 때
    public void DropBody()
    {
        transform.SetParent(null);

        if (GetComponent<Rigidbody>() != null)
        {
            GetComponent<Rigidbody>().isKinematic = false;
        }

        // 내려놓았으니 다시 충돌할 수 있도록 콜라이더 On
        if (_myCollider != null)
        {
            _myCollider.enabled = true;
        }

        transform.position = new Vector3(transform.position.x, 0.1f, transform.position.z);
    }



}
