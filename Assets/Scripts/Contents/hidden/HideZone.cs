using UnityEngine;


//적을 숨길 장소(수풀, 쓰레기통, 상자 등)에 붙임.

public class HideZone : MonoBehaviour
{
    [Header("은닉 설정")]
    [SerializeField] private bool _hideCompletely = true;

    // 기절한 적이 은닉 구역(Trigger)에 들어왔을 때
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Unconscious"))
        {
            HideTarget(other.gameObject);
        }
    }

    // 은닉 구역에서 꺼낼 때 
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Unconscious"))
        {
            RevealTarget(other.gameObject);
        }
    }


    private void HideTarget(GameObject target)
    {
        Debug.Log($"[은닉]기절한 적 ({target.name})이 숨겨졌다.");

        // 적/CCTV 시야 레이저 및 플레이어 충돌 차단
        Collider targetCollider = target.GetComponent<Collider>();
        if (targetCollider != null)
        {
            targetCollider.enabled = false;
        }
            

        // 물리 연산(Ragdoll/Rigidbody) 고정
        Rigidbody targetRb = target.GetComponent<Rigidbody>();
        if (targetRb != null) targetRb.isKinematic = true;

        // 화면에서 시각적으로 완전히 제거
        if (_hideCompletely)
        {
            target.SetActive(false);
        }
    }

    private void RevealTarget(GameObject target)
    {
        Debug.Log($"[은닉 해제] 기절한 적 ({target.name})이 노출되었습니다.");

        target.SetActive(true);

        Collider targetCollider = target.GetComponent<Collider>();
        if (targetCollider != null) targetCollider.enabled = true;

        Rigidbody targetRb = target.GetComponent<Rigidbody>();
        if (targetRb != null) targetRb.isKinematic = false;
    }
}