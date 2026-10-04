using UnityEngine;

public class PadRotator : MonoBehaviour
{
    public float rotationSpeed = 30f;

    private void Update()
    {
        // 바닥에 누운 채로 Y축(Vector3.up)을 기준으로 평평하게 회전!
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }
}