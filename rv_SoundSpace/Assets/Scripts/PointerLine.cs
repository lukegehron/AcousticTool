using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.XR.WSA.Input;
using UnityEngine.EventSystems;

public class PointerLine : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var interactionSourceStates = InteractionManager.GetCurrentReading();
        foreach (var interactionSourceState in interactionSourceStates)
        {
            if (interactionSourceState.source.handedness == InteractionSourceHandedness.Right)
            {

                GameObject HMD = GameObject.Find("MixedRealityCameraParent");
                Vector3 offset = new Vector3(0, 0, 0);
                if (HMD != null)
                {
                    offset = HMD.transform.position;
                }

                Vector3 pos;
                Vector3 forward;
                Vector3 right;
                Vector3 up;
                if (interactionSourceState.sourcePose.TryGetPosition(out pos) && interactionSourceState.sourcePose.TryGetForward(out forward) && interactionSourceState.sourcePose.TryGetRight(out right) && interactionSourceState.sourcePose.TryGetUp(out up))
                {
                    forward = Quaternion.AngleAxis(35, right) * forward;
                    pos = pos + (0.035f * up) + offset;


                    int layerMask = 1 << 5;
                    //layerMask = ~layerMask;                


                    RaycastHit hit;
                    Ray ray = new Ray(pos, forward);
                    if (Physics.Raycast(ray, out hit, 2f, layerMask))
                    {
                        LineRenderer line = GetComponent<LineRenderer>();

                        if (line != null)
                        {
                            line.enabled = true;
                            line.SetPosition(0, pos);
                            line.SetPosition(1, hit.point);
                        }
                    }else
                    {
                        LineRenderer line = GetComponent<LineRenderer>();
                        if (line != null)
                        {
                            line.enabled = false;
                        }

                    }

                }
            }
        }
    }
}
