using UnityEngine;

//서류 문서 오브젝트 

public class DocumentObject : Interactable
{
    void Awake()
    {
        interactionPrompt = "HOLD [E] TO TAKE DOCUMENTS";
        RequiredInteractionTime = 1.5f;
    }

    public override void OnInteractComplete()
    {
        base.OnInteractComplete();

        Debug.Log("Take Document!");

        Destroy(gameObject);
    }
}
