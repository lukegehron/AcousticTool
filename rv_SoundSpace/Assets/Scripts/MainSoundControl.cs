using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.XR.WSA.Input;
//using HoloToolkit.Unity.InputModule;

public class MainSoundControl : MonoBehaviour
{

    public GameObject emitterPrefab;
    public GameObject emitterBeamPrefab;
    public GameObject repeatEmitterPrefab;
    public bool MeshesAreVisible = true;
    TriangleList.EmitType emitType = TriangleList.EmitType.Beam;

    // Start is called before the first frame update
    void Start()
    {
        InteractionManager.InteractionSourcePressed += OnControllerPressed;
        InteractionManager.InteractionSourceUpdated += OnSourceUpdated;

        //TurnCrowdOff();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            CreateEmitter(new Vector3(0, 0, 0));
        }

        //var interactionSourceStates = InteractionManager.GetCurrentReading();
        //foreach (var interactionSourceState in interactionSourceStates)
        //{
        //    if (interactionSourceState.source.handedness == InteractionSourceHandedness.Right)
        //    {
        //        Vector3 pos;
        //        Vector3 forward;
        //        if (interactionSourceState.sourcePose.TryGetPosition(out pos) && interactionSourceState.sourcePose.TryGetForward(out forward))
        //        {
        //            Debug.DrawLine(pos, pos + (5f * forward), Color.white,0.1f);

        //        }
        //    }
        //}
    }

    public void OnSourceUpdated(InteractionSourceUpdatedEventArgs eventData)
    {
        if (eventData.state.source.handedness == InteractionSourceHandedness.Right && eventData.state.touchpadTouched)
        {
            Vector3 pos;
            Vector3 forward;
            Vector3 right;

            if (eventData.state.sourcePose.TryGetPosition(out pos) && eventData.state.sourcePose.TryGetForward(out forward) && eventData.state.sourcePose.TryGetRight(out right))
            {
                forward = Quaternion.AngleAxis(35, right) * forward;
                //Quaternion rotateDown = Quaternion.Euler(20f, 0, 0);
                //forward = rotateDown * forward;
                // Debug.DrawLine(pos, pos + (5f * forward), Color.white, 0.1f);
                //change gameobject based on emittype
                if (emitType == TriangleList.EmitType.LowRes)
                {
                    RaycastHit hit;
                    Ray ray = new Ray(pos, forward);
                    if (Physics.Raycast(ray, out hit))
                    {
                        LineRenderer line = GetComponent<LineRenderer>();

                        if (line != null)
                        {
                            line.enabled = true;
                            line.SetPosition(0, pos);
                            line.SetPosition(1, hit.point);
                        }
                    }

                }
            }
            
        } else
        {
            LineRenderer line = GetComponent<LineRenderer>();
            if (line != null)
            {
                line.enabled = false;
            }
        }
    }

    public void OnControllerPressed(InteractionSourcePressedEventArgs eventData)
    {
        

        GameObject HMD = GameObject.Find("MixedRealityCameraParent");
        Vector3 offset = new Vector3(0, 0, 0);
        if (HMD!=null)
        {
            offset = HMD.transform.position;
        }
        if(eventData.state.source.handedness == InteractionSourceHandedness.Right && (eventData.pressType == InteractionSourcePressType.Touchpad))
        {
            Vector3 pos;
            Vector3 forward;
            Quaternion rot;
            Vector3 right;

            if (eventData.state.sourcePose.TryGetPosition(out pos) && (eventData.state.sourcePose.TryGetForward(out forward)) && eventData.state.sourcePose.TryGetRotation(out rot) && eventData.state.sourcePose.TryGetRight(out right))
            {
                Vector3 hitPosition = pos;
                forward = Quaternion.AngleAxis(35, right) * forward;
                //change gameobject based on emittype
                if (emitType == TriangleList.EmitType.LowRes)
                {                    
                    RaycastHit hit;
                    Ray ray = new Ray(pos, forward);
                    if (Physics.Raycast(ray, out hit))
                    {
                        hitPosition = hit.point + (1.6f * hit.normal);
                    }    
                    CreateRepeatEmitter(hitPosition + offset, rot, emitterPrefab);
                } else
                {
                    CreateRepeatEmitter(pos + offset, rot, emitterBeamPrefab);
                }
            }
                
        } else if (eventData.pressType == InteractionSourcePressType.Menu)
        {
            WaveEmitterBeam[] beams = GameObject.FindObjectsOfType<WaveEmitterBeam>();
            foreach(WaveEmitterBeam v in beams)
            {
                v.KillThisSound();
            }
            WaveEmitter[] spheres = GameObject.FindObjectsOfType<WaveEmitter>();
            foreach (WaveEmitter v in spheres)
            {
                v.KillThisSound();
            }
        }


    }

    public void CreateEmitter(Vector3 position)
    {
        GameObject newEmitter = Instantiate(emitterPrefab, position, Quaternion.identity);
        newEmitter.GetComponent<WaveEmitter>().MakeNoise();

       }

    public void ChangeTypeToBeam()
    {
        emitType = TriangleList.EmitType.Beam;

    }
    public void ChangeTypeToSphere()
    {
        emitType = TriangleList.EmitType.LowRes;

    }

    public void ToggleMeshes(bool newVisiblity)
    {
        MeshesAreVisible = newVisiblity;
        MeshWave[] wavelist = GameObject.FindObjectsOfType<MeshWave>();
        foreach(MeshWave mw in wavelist)
        {
            mw.GetComponent<MeshRenderer>().enabled = MeshesAreVisible;
        }
        MeshWaveBeam[] beamlist = GameObject.FindObjectsOfType<MeshWaveBeam>();
        foreach (MeshWaveBeam m in beamlist)
        {
            m.GetComponent<MeshRenderer>().enabled = MeshesAreVisible;
        }
    }


    public void CreateRepeatEmitter(Vector3 position, Quaternion rotation, GameObject emitter)
    {
        GameObject newEmitter = Instantiate(repeatEmitterPrefab, position, rotation);
        newEmitter.GetComponent<RepeatEmitter>().startEmitting(emitter, emitType);
    }

    public void TurnCrowdOn()
    {
        WaveEmitterCrowd[] crowd = GameObject.FindObjectsOfType<WaveEmitterCrowd>();
        foreach(WaveEmitterCrowd c in crowd)
        {
            //c.gameObject.SetActive(true);
            c.reEnable();
        }
    }

    public void TurnCrowdOff()
    {
        WaveEmitterCrowd[] crowd = GameObject.FindObjectsOfType<WaveEmitterCrowd>();
        foreach (WaveEmitterCrowd c in crowd)
        {
            //c.gameObject.SetActive(false);
            c.clearPoints();
        }
    }

}
