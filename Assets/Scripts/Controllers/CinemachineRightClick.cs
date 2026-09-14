using UnityEngine;
using Cinemachine;

public class CinemachineRightClick : MonoBehaviour
{
    private CinemachineFreeLook _freeLook;

    void Awake()
    {
        _freeLook = GetComponent<CinemachineFreeLook>();
    }

    void Update()
    {
        if (_freeLook == null) return;

        // 마우스 우클릭(1)을 누르고 있을 때만 좌우(X축) 회전 허용
        if (Input.GetMouseButton(1))
        {
            _freeLook.m_XAxis.m_InputAxisName = "Mouse X";
            _freeLook.m_YAxis.m_InputAxisName = "Mouse Y";
        }
        else
        {
            // 우클릭을 떼면 회전 중단
            _freeLook.m_XAxis.m_InputAxisName = "";
            _freeLook.m_YAxis.m_InputAxisName = "";
            
            _freeLook.m_XAxis.m_InputAxisValue = 0f;
            _freeLook.m_YAxis.m_InputAxisValue = 0f;
        }
    }
}