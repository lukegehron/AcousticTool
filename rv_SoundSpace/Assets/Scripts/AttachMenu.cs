using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.XR.WSA.Input;

public class AttachMenu : MonoBehaviour
{
    bool followController = false;
    bool foundController = false;
    uint sourceID;
    // Start is called before the first frame update
    void Start()
    {
        ////InteractionManager.InteractionSourcePressed += OnControllerPressed;
        //var interactionSourceStates = InteractionManager.GetCurrentReading();
        //foreach (var interactionSourceState in interactionSourceStates)
        //{
        //    if (interactionSourceState.source.handedness == InteractionSourceHandedness.Left)
        //    {
        //        sourceID = interactionSourceState.source.id;
        //    }


        //}

        //GameObject controller = GameObject.Find("1118/1627/0/LeftController");
        //transform.SetParent(controller.transform);

    }
    //1118/1627/0/LeftController
    // Update is called once per frame
    void Update()
    {
        if(!foundController)
        {
            GameObject controller = GameObject.Find("1118/1627/0/LeftController");
            if(controller != null)
            {
                transform.SetParent(controller.transform,false);
                foundController = true;
            }
           
        }

        //if (followController)
        //{
        //    var interactionSourceStates = InteractionManager.GetCurrentReading();
        //    foreach (var interactionSourceState in interactionSourceStates)
        //    {
        //        if(interactionSourceState.source.id == sourceID)
        //        {
        //            Vector3 pos;
        //            if (interactionSourceState.sourcePose.TryGetPosition(out pos)) 
        //                gameObject.transform.position = pos;
        //            Quaternion rot;
        //            if (interactionSourceState.sourcePose.TryGetRotation(out rot))
        //                gameObject.transform.rotation = rot;
        //        }
        //    }
        //}
    }

    public void OnControllerPressed(InteractionSourcePressedEventArgs eventData)
    {
        if (eventData.pressType == InteractionSourcePressType.Menu)
        {
            followController = true;
            sourceID = eventData.state.source.id;
        }
            
    }
}
