using UnityEngine;

public class Interactable : MonoBehaviour
{
    [Header("상호작용 기본 설정")]
    public string interactionPrompt = "HOLD [E] TO INTERACT";
    public float RequiredInteractionTime = 2.0f; // 미션을 완료하는 데 걸리는 시간(초)

    //오버라이드해서 쓸 예정
    public virtual void OnInteractComplete()
    {
        Debug.Log($"{name} 상호작용 완료!");
    }
}
