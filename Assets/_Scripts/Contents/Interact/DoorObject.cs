using UnityEngine;

public class DoorObject : Interactable
{
    private bool _isOpened = false;

    void Awake()
    {
        interactionPrompt = "HOLD [E] TO OPEN THE DOOR";
        RequiredInteractionTime = 3.0f;
    }

    public override void OnInteractComplete()
    {
        if (_isOpened) return;

        base.OnInteractComplete();

        Debug.Log("OPEN DOOR");
        _isOpened = true;

        //임시 위치 조정
        transform.position += new Vector3(0, 3.0f, 0);

        //열린 문은 상호작용 더이상 불가
        if (GetComponent<Collider>() != null)
        {
            GetComponent<Collider>().enabled = false;
        }


    }
}
