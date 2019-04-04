using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.XR.WSA.Input;


public class WaveEmitterBeam : MonoBehaviour
{

    //Line Render Colors
    float colorCounter = 1.0f;
    float colorFalloff = 0.005f;
    float colorWallHitFalloff = 0.05f;
    public float[] colorArrayAlpha = new float[1200];


    //GameControllers
    public int pause = 0; //PAUSE THE GAME
    public int forwards = 1; //1 = forwards, 0 = backwards
   

    //Referenced GameObjects+Prefabs    
    public GameObject linePrefab; // reference the linestyle prefab
    public GameObject spherePrefab; //the balls that hit the walls
    public GameObject spriteBillboardPrefab; //the ripples that hit the walls
    public GameObject spriteBillboardPrefab1; //the ripples that hit the walls
    public GameObject spriteBillboardPrefab2; //the ripples that hit the walls


    //General Variables
    public int counter = 0;  // global counting variable 
    public int lengthOfLineRenderer = 5; // The number of positions on the line renderer
    public int hasBeenPressed = 0; //check if mouse has been pressed, this could just be a boolean....
    public int numOfVectors; // the number of mesh verticies... this number would change based on emitter throw angle
    public float stepFactor = 0.5f; //distance per step... this decreases the size + speed, and increases the resolution


    // Empty Arrays to fill
    public Vector3[][] myarrays = new Vector3[1200][]; //the 3D Points. there are 2562 lines (numOfVectors) made up of 20 points (lengthOfLineRenderer) to make a line
    public GameObject[] EmptyRayHolders; //Each line renderer requires its own GameObject. These are those gameobjects.
    private Vector3[] TravelVectors; //Vectors set in the start below



    void Start()
    {
        //Contoller events for thumbstick
        InteractionManager.InteractionSourcePressed += OnControllerPressed;
        InteractionManager.InteractionSourceUpdated += OnSourceUpdated;

        for (int m = 0; m < numOfVectors; m++)
        {
            colorArrayAlpha[m] = 1.0f;
        }

        TravelVectors = TriangleList.GetVectorList(TriangleList.EmitType.Beam);
        numOfVectors = TriangleList.GetVectorCount(TriangleList.EmitType.Beam);


        Quaternion rot = gameObject.transform.rotation;
        for(int i = 0;i<numOfVectors;i++)
        {
            TravelVectors[i] = rot * TravelVectors[i];
        }
    }

    //Forward and backwards with thumb stick
    public void OnSourceUpdated(InteractionSourceUpdatedEventArgs eventData)
    {
        if (eventData.state.thumbstickPosition.x > 0.5)
        {
            forwards = 1;
            hasBeenPressed = 1;
        }
        else if (eventData.state.thumbstickPosition.x < -0.5)
        {
            forwards = 0;
            hasBeenPressed = 1;
        }
    }

    //Pause with thumbstick
    public void OnControllerPressed(InteractionSourcePressedEventArgs eventData)
    {
        if (eventData.pressType == InteractionSourcePressType.Thumbstick)
        {
            PauseSound();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            PauseSound();
        }

        if (Input.GetKeyDown("r"))
        {
            if (forwards == 1)
            {
                forwards = 0;
            }
            else
            {
                forwards = 1;
            }
        }



        if (hasBeenPressed == 1)
        {

            for (int m = 0; m < numOfVectors; m++)
            {


                LineRenderer lineRenderer = EmptyRayHolders[m].GetComponent<LineRenderer>();

                Vector3 OldPosition = myarrays[m][lengthOfLineRenderer - 1];
                Vector3 NewPosition = new Vector3(0, 0, 0);



                for (int i = 0; i < lengthOfLineRenderer - 1; i++)
                {
                    int j = i + 1;
                    myarrays[m][i] = myarrays[m][j];

                }

                //DONT HIT UI
                int layerMask = 1 << 5;
                layerMask = ~layerMask;

                RaycastHit hit;
                if (Physics.Raycast(OldPosition, TravelVectors[m], out hit, stepFactor, layerMask))
                {
                    TravelVectors[m] = Vector3.Reflect(TravelVectors[m], hit.normal);

                    Vector3 Deconstruct = hit.normal;
                    Deconstruct.x = Deconstruct.x * .05f;
                    Deconstruct.y = Deconstruct.y * .05f;
                    Deconstruct.z = Deconstruct.z * .05f;

                    float NRC = GetNRC.getNRCFromCollider(hit.collider);



                    colorArrayAlpha[m] = colorArrayAlpha[m] * (1 - NRC);

                    if (colorArrayAlpha[m] > .01)
                    {
                        if (NRC < .2)
                        {
                            GameObject ripple = Instantiate(spriteBillboardPrefab1, hit.point + Deconstruct, Quaternion.identity);
                            Destroy(ripple, 1);
                        }
                        else if (NRC > .2 && NRC < .6)
                        {
                            GameObject ripple = Instantiate(spriteBillboardPrefab, hit.point + Deconstruct, Quaternion.identity);
                            Destroy(ripple, 1);
                        }
                        else
                        {
                            GameObject ripple = Instantiate(spriteBillboardPrefab2, hit.point + Deconstruct, Quaternion.identity);
                            Destroy(ripple, 1);
                        }
                    }
                }


                // Change the NewPosition Vector's x and y components
                if (forwards == 1)
                {

                    NewPosition.x = OldPosition.x + (TravelVectors[m].x * stepFactor);
                    NewPosition.y = OldPosition.y + (TravelVectors[m].y * stepFactor);
                    NewPosition.z = OldPosition.z + (TravelVectors[m].z * stepFactor);
                }
                else
                {
                    if (OldPosition == gameObject.transform.position)
                    {
                        NewPosition.x = OldPosition.x;
                        NewPosition.y = OldPosition.y;
                        NewPosition.z = OldPosition.z;
                    }
                    else
                    {
                        NewPosition.x = OldPosition.x - (TravelVectors[m].x * stepFactor);
                        NewPosition.y = OldPosition.y - (TravelVectors[m].y * stepFactor);
                        NewPosition.z = OldPosition.z - (TravelVectors[m].z * stepFactor);
                    }
                }

                Color c1 = new Color(0.5f * colorArrayAlpha[m], colorArrayAlpha[m], 1, colorArrayAlpha[m] * 2);
                Color c2 = new Color(1, 1, 1, 0);

                myarrays[m][lengthOfLineRenderer - 1] = NewPosition;
                lineRenderer.SetPositions(myarrays[m]);
                lineRenderer.SetColors(c2, c1);

                //Set material color and opacity

                colorArrayAlpha[m] = colorArrayAlpha[m] - colorFalloff;

            }

            colorCounter = colorCounter - colorFalloff;

            if (pause == 1) // to pause the game
            {
                hasBeenPressed = 0;
            }

            if (colorCounter < .01)
            {
                for (int m = 0; m < numOfVectors; m++)
                {
                    Destroy(EmptyRayHolders[m]);
                }
                Destroy(gameObject);
                hasBeenPressed = 0;

            }
            counter++;
            //Debug.Log(Time.deltaTime);

        }
    }

    public void KillThisSound()
    {
        for (int m = 0; m < numOfVectors; m++)
        {
            Destroy(EmptyRayHolders[m]);
        }
        Destroy(gameObject);
        hasBeenPressed = 0;
    }

    public void MakeNoise()
    {
        //hasBeenPressed = 1;
        for (int x = 0; x < numOfVectors; x++)
        {
            myarrays[x] = new Vector3[5];
            for (int y = 0; y < 5; y++)
            {
                //myarrays[x][y] = new Vector3(0.5f, 0.5f, 0.5f); // change this variable to change where the sound is emitted
                myarrays[x][y] = gameObject.transform.position; // change this variable to change where the sound is emitted
                
            }
        }

        Debug.Log("MousePressed");
        hasBeenPressed = 1;


        EmptyRayHolders = new GameObject[numOfVectors];
        for (int i = 0; i < numOfVectors; i++)
        {
            GameObject currentGameObject = Instantiate(linePrefab, new Vector3(0, 0, 0), Quaternion.identity);
            EmptyRayHolders[i] = currentGameObject;
        }
    }

    public void PauseSound()
    {
        if (hasBeenPressed == 0)
        {
            hasBeenPressed = 1;
        }
        else
        {
            hasBeenPressed = 0;
        }
    }


        public Vector3[] GetVectorsListAtIndex(int index)
    {
        Vector3[] vects = new Vector3[numOfVectors];
        for (int i = 0; i < numOfVectors; i++)
        {
            Quaternion rot = Quaternion.Inverse(gameObject.transform.rotation);
            vects[i] = rot * (myarrays[i][1] - gameObject.transform.position);
            //vects[i] = myarrays[i][1] - gameObject.transform.position;
        }
        return vects;
    }
}