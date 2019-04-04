using UnityEngine;
using System.Collections;

public class BillboardZeyu : MonoBehaviour
{
    private Camera Cam;

    void Start()
    {
        var Cam = GameObject.Find("MixedRealityCamera");
    }

    //Orient the camera after all movement is completed this frame to avoid jittering
    void LateUpdate()
    {
        transform.LookAt(transform.position + Cam.transform.rotation * Vector3.forward,
            Cam.transform.rotation * Vector3.up);
    }
}