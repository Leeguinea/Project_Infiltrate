using UnityEngine;
using Cinemachine; // 시네머신 네임스페이스 추가!

public class OfficeTeleporter : MonoBehaviour
{
    [Header("이동할 목적지 Transform")]
    public Transform targetDestination;

    private bool _isPlayerNearby = false;
    private GameObject _playerObj;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerNearby = true;
            _playerObj = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerNearby = false;
            _playerObj = null;
        }
    }

    private void Update()
    {
        if (_isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            TeleportPlayer();
        }
    }

    private void TeleportPlayer()
    {
        if (_playerObj == null || targetDestination == null) return;

        // 1. 플레이어 이동 전후의 거리 변화량(Delta) 계산
        Vector3 deltaPosition = targetDestination.position - _playerObj.transform.position;

        // 2. CharacterController 비활성화 후 순간이동
        CharacterController cc = _playerObj.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        _playerObj.transform.position = targetDestination.position;
        _playerObj.transform.rotation = targetDestination.rotation;

        if (cc != null) cc.enabled = true;

        // 3. 시네머신 카메라에게 순간이동(Warp) 상태 알림
        CinemachineFreeLook freeLook = FindAnyObjectByType<CinemachineFreeLook>();
        if (freeLook != null)
        {
            // 카메라 위치도 잔상 없이 즉시 플레이어 위치로 이동시킵니다.
            freeLook.OnTargetObjectWarped(_playerObj.transform, deltaPosition);
        }

        Debug.Log("순간이동 완료 및 카메라 위치 동기화: " + targetDestination.name);
    }
}