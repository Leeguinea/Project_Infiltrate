using UnityEngine;
using System.Collections;
using Cinemachine;

public class TeleportDoor : MonoBehaviour
{
    [Header("이동할 목적지")]
    public Transform destinationPoint;

    [Header("시네마신 프리룩 카메라")]
    [SerializeField] private CinemachineFreeLook freeLookCam;

    private Transform playerTransform;
    private bool _isPlayerInRange = false;
    private static bool isTeleporting = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerTransform = other.transform;
            _isPlayerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInRange = false;
        }
    }

    private void Update()
    {
        if (isTeleporting || !_isPlayerInRange || playerTransform == null || destinationPoint == null) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(TeleportRoutine());
        }
    }

    private IEnumerator TeleportRoutine()
    {
        isTeleporting = true;

        if (destinationPoint == transform)
        {
            Debug.LogError($"[에러] '{gameObject.name}'의 목적지가 자기 자신으로 지정되어 있습니다!");
            isTeleporting = false;
            yield break;
        }

        // Rigidbody 물리 방해 차단
        Rigidbody rb = playerTransform.GetComponent<Rigidbody>();
        bool previousKinematic = false;
        if (rb != null)
        {
            previousKinematic = rb.isKinematic;
            rb.isKinematic = true;
        }

        // CharacterController 끄기
        CharacterController cc = playerTransform.GetComponent<CharacterController>();
        if (cc == null)
        {
            cc = playerTransform.GetComponentInChildren<CharacterController>();
        }

        if (cc != null)
        {
            cc.enabled = false;
        }

        // 이동 전 플레이어의 기존 위치 저장
        Vector3 oldPosition = playerTransform.position;

        // 위치를 목적지로 강제 변경
        playerTransform.position = destinationPoint.position;
        Physics.SyncTransforms();

        // FreeLook 카메라 워프 처리 (인스펙터에 연결된 카메라 사용)
        if (freeLookCam != null)
        {
            freeLookCam.OnTargetObjectWarped(playerTransform, destinationPoint.position - oldPosition);
        }
        else
        {
            Debug.LogWarning($"[주의] '{gameObject.name}' 문에 FreeLook 카메라가 연결되어 있지 않습니다!");
        }

        // 1프레임 대기
        yield return null;

        // CharacterController 다시 켜기
        if (cc != null)
        {
            cc.enabled = true;
        }

        // Rigidbody 원상복구
        if (rb != null)
        {
            rb.isKinematic = previousKinematic;
        }

        // 연속 텔레포트 방지 쿨타임
        yield return new WaitForSeconds(0.5f);
        isTeleporting = false;
    }
}