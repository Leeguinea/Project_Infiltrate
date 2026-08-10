using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private Transform _mainCamerTransform;

    void Start()
    {
        if(Camera.main != null)
        {
            _mainCamerTransform = Camera.main.transform;
        }
    }

    void LateUpdate()
    {
        if(_mainCamerTransform != null)
        {
            transform.LookAt(transform.position + _mainCamerTransform.forward);
        }
    }
}
