using UnityEngine;
using System.Collections;

public class Billboard2 : MonoBehaviour
{
    private Camera m_Camera;

    void OnEnable()
    {

        //var m_CameraGO = GameObject.FindWithTag("MainCamera");

        //m_Camera = m_CameraGO.GetComponent<Camera>();
        m_Camera = Camera.main;
    }

    //Orient the camera after all movement is completed this frame to avoid jittering
    void LateUpdate()
    {
        transform.LookAt(transform.position + m_Camera.transform.rotation * Vector3.forward,
            m_Camera.transform.rotation * Vector3.up);
    }
}