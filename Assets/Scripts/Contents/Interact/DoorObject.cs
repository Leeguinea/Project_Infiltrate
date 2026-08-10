using UnityEngine;

public class DoorObject : Interactable
{
    [Header("열릴 때 목표 위치 (상대값)")]
    [SerializeField] private Vector3 _targetOffset = new Vector3(0,3f,0);

    [Header("열리는 속도")]
    [SerializeField] private float _openSpeed = 2f;

    private bool _isOpened = false; //문이 열리는 중인지
    private Vector3 _startPosition; // 초기 위치
    private Vector3 _endPosition; // 최종 위치


    void Awake()
    {
        interactionPrompt = "HOLD [E] TO OPEN THE DOOR";
        RequiredInteractionTime = 3.0f;

        _startPosition = transform.position;
    }

    public override void OnInteractComplete()
    {
        if (_isOpened) return;

        //.OnInteractComplete();

        _endPosition = _startPosition + _targetOffset;
        _isOpened = true;

        Debug.Log("문이 열렸다.");

        //열린 문은 상호작용 더이상 불가
        if (GetComponent<Collider>() != null)
        {
            GetComponent<Collider>().enabled = false;
        }
    }

    void Update()
    {
        if(_isOpened)
        {
            //문을 일정 속도로 열기
            transform.position = Vector3.MoveTowards(
                transform.position,         //현재 문의 위치
                _endPosition,               //최종 문의 위치
                _openSpeed * Time.deltaTime //속도
            );


            //현재 위치 == 최종 위치
            if (transform.position == _endPosition)
            {
                _isOpened = false;
                Debug.Log("문이 완전 열렸습니다.");
            }
        }
    }


}
