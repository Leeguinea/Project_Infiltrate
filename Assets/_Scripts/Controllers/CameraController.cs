using UnityEngine;

/*
 * [역할]
* 1. 카메라가 플레이어의 이동에 맞춰 부드럽게 추격 (SmoothDamp 적용)
 * 2. 시작 시 플레이어 위치로 즉시 이동하여 초기 딜레이 방지
 */

public class CameraController : MonoBehaviour
{
    [SerializeField]
    GameObject _player;

    [Header("거리 및 속도")]
    [SerializeField]
    Vector3 _delta = new Vector3(0.0f, 10.0f, -10.0f); //쿼터뷰 값.

    [SerializeField]
    [Range(0.01f, 0.5f)]
    private float _smoothTime = 0.15f; // 도달 시간 (작을수록 빠릿함)

    [SerializeField]
    private float _maxSpeed = 100f; // 카메라 최대 속도 제한

    private Vector3 _currentVelocity = Vector3.zero; // SmoothDamp 내부 계산용 속도 변수

    void Start()
    {
        if (_player == null)
        {
            _player = GameObject.FindWithTag("Player");
        }

        if (_player == null)
        {
            Debug.LogError("CameraContrller에서 Player 태그를 가진 오브젝트를 찾을 수 없습니다.");
            return;
        }

        if (_player != null)
        {
            transform.position = _player.transform.position + _delta;
        }
    }

    //카메라 이동 시 흔들림 방지 == 기본 
    void LateUpdate()
    {
        if (_player == null)
            return;

        Vector3 targetPos = _player.transform.position + _delta;

        // 'Lerp' 적용 시 끊기는 느낌이 들어서(target과 가까워지면 느려짐) 'SmoothDamp' 적용함.
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref _currentVelocity, _smoothTime, _maxSpeed);
    }
}
