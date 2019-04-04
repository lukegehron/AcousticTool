using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.XR.WSA.Input;

public class WaveEmitterCrowd : MonoBehaviour
{

    //Line Render Colors
    float colorCounter = 1.0f;
    float colorFalloff = 0.001f;
    float colorWallHitFalloff = 0.05f;
    public float[] colorArrayAlpha = new float[162];
    

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
    int hasAlreadyHappened = 0;


    // Empty Arrays to fill
    public Vector3[][] myarrays = new Vector3[162][]; //the 3D Points. there are 2562 lines (numOfVectors) made up of 20 points (lengthOfLineRenderer) to make a line
    public GameObject[] EmptyRayHolders; //Each line renderer requires its own GameObject. These are those gameobjects.
    private Vector3[] TravelVectors; //Vectors set in the start below
    private Vector3[] ChangedTravelVectors; //Vectors set in the start below


    private bool allowCrowd = false;
    

    

    void Start()
    {
        //Contoller events for thumbstick
        InteractionManager.InteractionSourcePressed += OnControllerPressed;
        InteractionManager.InteractionSourceUpdated += OnSourceUpdated;

        for (int m = 0; m < numOfVectors; m++)
        {
            colorArrayAlpha[m] = 1.0f;
        }

        TravelVectors = new Vector3[numOfVectors]; //2562
        ChangedTravelVectors = new Vector3[numOfVectors]; //2562

        TravelVectors[0] = new Vector3(1f, 0f, 0f);
        TravelVectors[1] = new Vector3(0.447f, 0f, 0.894f);
        TravelVectors[2] = new Vector3(0.447f, 0.851f, 0.276f);
        TravelVectors[3] = new Vector3(0.447f, 0.526f, -0.724f);
        TravelVectors[4] = new Vector3(0.447f, -0.526f, -0.724f);
        TravelVectors[5] = new Vector3(0.447f, -0.851f, 0.276f);
        TravelVectors[6] = new Vector3(-0.447f, 0.526f, 0.724f);
        TravelVectors[7] = new Vector3(-0.447f, 0.851f, -0.276f);
        TravelVectors[8] = new Vector3(-0.447f, 0f, -0.894f);
        TravelVectors[9] = new Vector3(-0.447f, -0.851f, -0.276f);
        TravelVectors[10] = new Vector3(-0.447f, -0.526f, 0.724f);
        TravelVectors[11] = new Vector3(-1f, 0f, 0f);
        TravelVectors[12] = new Vector3(0.851f, 0f, 0.526f);
        TravelVectors[13] = new Vector3(0.526f, 0.5f, 0.688f);
        TravelVectors[14] = new Vector3(0.851f, 0.5f, 0.162f);
        TravelVectors[15] = new Vector3(0.526f, 0.809f, -0.263f);
        TravelVectors[16] = new Vector3(0.851f, 0.309f, -0.425f);
        TravelVectors[17] = new Vector3(0.526f, 0f, -0.851f);
        TravelVectors[18] = new Vector3(0.851f, -0.309f, -0.425f);
        TravelVectors[19] = new Vector3(0.526f, -0.809f, -0.263f);
        TravelVectors[20] = new Vector3(0.851f, -0.5f, 0.162f);
        TravelVectors[21] = new Vector3(0.526f, -0.5f, 0.688f);
        TravelVectors[22] = new Vector3(0f, -0.809f, 0.588f);
        TravelVectors[23] = new Vector3(0f, -0.309f, 0.951f);
        TravelVectors[24] = new Vector3(0f, 0.309f, 0.951f);
        TravelVectors[25] = new Vector3(0f, 0.809f, 0.588f);
        TravelVectors[26] = new Vector3(-0.526f, 0f, 0.851f);
        TravelVectors[27] = new Vector3(-0.526f, 0.809f, 0.263f);
        TravelVectors[28] = new Vector3(0f, 1f, 0f);
        TravelVectors[29] = new Vector3(0f, 0.809f, -0.588f);
        TravelVectors[30] = new Vector3(-0.526f, 0.5f, -0.688f);
        TravelVectors[31] = new Vector3(0f, 0.309f, -0.951f);
        TravelVectors[32] = new Vector3(0f, -0.309f, -0.951f);
        TravelVectors[33] = new Vector3(-0.526f, -0.5f, -0.688f);
        TravelVectors[34] = new Vector3(0f, -0.809f, -0.588f);
        TravelVectors[35] = new Vector3(0f, -1f, 0f);
        TravelVectors[36] = new Vector3(-0.526f, -0.809f, 0.263f);
        TravelVectors[37] = new Vector3(-0.851f, -0.309f, 0.425f);
        TravelVectors[38] = new Vector3(-0.851f, 0.309f, 0.425f);
        TravelVectors[39] = new Vector3(-0.851f, 0.5f, -0.162f);
        TravelVectors[40] = new Vector3(-0.851f, 0f, -0.526f);
        TravelVectors[41] = new Vector3(-0.851f, -0.5f, -0.162f);
        TravelVectors[42] = new Vector3(0.657f, 0.717f, 0.233f);
        TravelVectors[43] = new Vector3(0.727f, 0.518f, 0.45f);
        TravelVectors[44] = new Vector3(0.502f, 0.717f, 0.484f);
        TravelVectors[45] = new Vector3(0.968f, 0f, 0.251f);
        TravelVectors[46] = new Vector3(0.891f, 0.267f, 0.368f);
        TravelVectors[47] = new Vector3(0.968f, 0.238f, 0.077f);
        TravelVectors[48] = new Vector3(0.502f, 0.238f, 0.831f);
        TravelVectors[49] = new Vector3(0.727f, 0.267f, 0.632f);
        TravelVectors[50] = new Vector3(0.657f, 0f, 0.754f);
        TravelVectors[51] = new Vector3(0.657f, 0.443f, -0.61f);
        TravelVectors[52] = new Vector3(0.727f, 0.588f, -0.354f);
        TravelVectors[53] = new Vector3(0.502f, 0.681f, -0.532f);
        TravelVectors[54] = new Vector3(0.891f, 0.433f, -0.141f);
        TravelVectors[55] = new Vector3(0.968f, 0.147f, -0.203f);
        TravelVectors[56] = new Vector3(0.502f, 0.864f, 0.03f);
        TravelVectors[57] = new Vector3(0.727f, 0.684f, -0.059f);
        TravelVectors[58] = new Vector3(0.657f, -0.443f, -0.61f);
        TravelVectors[59] = new Vector3(0.727f, -0.155f, -0.668f);
        TravelVectors[60] = new Vector3(0.502f, -0.296f, -0.813f);
        TravelVectors[61] = new Vector3(0.891f, 0f, -0.455f);
        TravelVectors[62] = new Vector3(0.968f, -0.147f, -0.203f);
        TravelVectors[63] = new Vector3(0.502f, 0.296f, -0.813f);
        TravelVectors[64] = new Vector3(0.727f, 0.155f, -0.668f);
        TravelVectors[65] = new Vector3(0.657f, -0.717f, 0.233f);
        TravelVectors[66] = new Vector3(0.727f, -0.684f, -0.059f);
        TravelVectors[67] = new Vector3(0.502f, -0.864f, 0.03f);
        TravelVectors[68] = new Vector3(0.891f, -0.433f, -0.141f);
        TravelVectors[69] = new Vector3(0.968f, -0.238f, 0.077f);
        TravelVectors[70] = new Vector3(0.502f, -0.681f, -0.532f);
        TravelVectors[71] = new Vector3(0.727f, -0.588f, -0.354f);
        TravelVectors[72] = new Vector3(0.727f, -0.267f, 0.632f);
        TravelVectors[73] = new Vector3(0.502f, -0.238f, 0.831f);
        TravelVectors[74] = new Vector3(0.891f, -0.267f, 0.368f);
        TravelVectors[75] = new Vector3(0.502f, -0.717f, 0.484f);
        TravelVectors[76] = new Vector3(0.727f, -0.518f, 0.45f);
        TravelVectors[77] = new Vector3(-0.252f, -0.443f, 0.861f);
        TravelVectors[78] = new Vector3(0.009f, -0.588f, 0.809f);
        TravelVectors[79] = new Vector3(-0.252f, -0.681f, 0.687f);
        TravelVectors[80] = new Vector3(0.272f, -0.433f, 0.859f);
        TravelVectors[81] = new Vector3(0.252f, -0.147f, 0.957f);
        TravelVectors[82] = new Vector3(0.252f, -0.864f, 0.436f);
        TravelVectors[83] = new Vector3(0.272f, -0.684f, 0.677f);
        TravelVectors[84] = new Vector3(0.272f, 0.684f, 0.677f);
        TravelVectors[85] = new Vector3(0.252f, 0.864f, 0.436f);
        TravelVectors[86] = new Vector3(0.252f, 0.147f, 0.957f);
        TravelVectors[87] = new Vector3(0.272f, 0.433f, 0.859f);
        TravelVectors[88] = new Vector3(-0.252f, 0.681f, 0.687f);
        TravelVectors[89] = new Vector3(0.009f, 0.588f, 0.809f);
        TravelVectors[90] = new Vector3(-0.252f, 0.443f, 0.861f);
        TravelVectors[91] = new Vector3(-0.272f, 0.155f, 0.95f);
        TravelVectors[92] = new Vector3(-0.502f, 0.296f, 0.813f);
        TravelVectors[93] = new Vector3(-0.009f, 0f, 1f);
        TravelVectors[94] = new Vector3(-0.502f, -0.296f, 0.813f);
        TravelVectors[95] = new Vector3(-0.272f, -0.155f, 0.95f);
        TravelVectors[96] = new Vector3(-0.252f, 0.955f, -0.155f);
        TravelVectors[97] = new Vector3(-0.272f, 0.951f, 0.146f);
        TravelVectors[98] = new Vector3(-0.502f, 0.864f, -0.03f);
        TravelVectors[99] = new Vector3(-0.009f, 0.951f, 0.309f);
        TravelVectors[100] = new Vector3(0.252f, 0.955f, 0.155f);
        TravelVectors[101] = new Vector3(-0.502f, 0.681f, 0.532f);
        TravelVectors[102] = new Vector3(-0.272f, 0.855f, 0.441f);
        TravelVectors[103] = new Vector3(0.272f, 0.855f, -0.441f);
        TravelVectors[104] = new Vector3(0.252f, 0.681f, -0.687f);
        TravelVectors[105] = new Vector3(0.272f, 0.951f, -0.146f);
        TravelVectors[106] = new Vector3(-0.252f, 0.864f, -0.436f);
        TravelVectors[107] = new Vector3(0.009f, 0.951f, -0.309f);
        TravelVectors[108] = new Vector3(-0.252f, 0.147f, -0.957f);
        TravelVectors[109] = new Vector3(-0.272f, 0.433f, -0.859f);
        TravelVectors[110] = new Vector3(-0.502f, 0.238f, -0.831f);
        TravelVectors[111] = new Vector3(-0.009f, 0.588f, -0.809f);
        TravelVectors[112] = new Vector3(0.252f, 0.443f, -0.861f);
        TravelVectors[113] = new Vector3(-0.502f, 0.717f, -0.484f);
        TravelVectors[114] = new Vector3(-0.272f, 0.684f, -0.677f);
        TravelVectors[115] = new Vector3(0.273f, -0.155f, -0.95f);
        TravelVectors[116] = new Vector3(0.251f, -0.443f, -0.861f);
        TravelVectors[117] = new Vector3(0.273f, 0.155f, -0.95f);
        TravelVectors[118] = new Vector3(-0.252f, -0.147f, -0.957f);
        TravelVectors[119] = new Vector3(0.009f, 0f, -1f);
        TravelVectors[120] = new Vector3(-0.251f, -0.864f, -0.436f);
        TravelVectors[121] = new Vector3(-0.272f, -0.684f, -0.677f);
        TravelVectors[122] = new Vector3(-0.502f, -0.717f, -0.484f);
        TravelVectors[123] = new Vector3(-0.009f, -0.588f, -0.809f);
        TravelVectors[124] = new Vector3(0.252f, -0.681f, -0.687f);
        TravelVectors[125] = new Vector3(-0.502f, -0.238f, -0.831f);
        TravelVectors[126] = new Vector3(-0.272f, -0.433f, -0.859f);
        TravelVectors[127] = new Vector3(0.272f, -0.951f, -0.146f);
        TravelVectors[128] = new Vector3(0.251f, -0.955f, 0.155f);
        TravelVectors[129] = new Vector3(0.272f, -0.855f, -0.441f);
        TravelVectors[130] = new Vector3(-0.252f, -0.955f, -0.155f);
        TravelVectors[131] = new Vector3(0.009f, -0.951f, -0.309f);
        TravelVectors[132] = new Vector3(-0.272f, -0.855f, 0.441f);
        TravelVectors[133] = new Vector3(-0.502f, -0.681f, 0.532f);
        TravelVectors[134] = new Vector3(-0.009f, -0.951f, 0.309f);
        TravelVectors[135] = new Vector3(-0.502f, -0.864f, -0.03f);
        TravelVectors[136] = new Vector3(-0.272f, -0.951f, 0.146f);
        TravelVectors[137] = new Vector3(-0.968f, 0.147f, 0.203f);
        TravelVectors[138] = new Vector3(-0.891f, 0f, 0.455f);
        TravelVectors[139] = new Vector3(-0.968f, -0.147f, 0.203f);
        TravelVectors[140] = new Vector3(-0.727f, 0.155f, 0.668f);
        TravelVectors[141] = new Vector3(-0.657f, 0.443f, 0.61f);
        TravelVectors[142] = new Vector3(-0.657f, -0.443f, 0.61f);
        TravelVectors[143] = new Vector3(-0.727f, -0.155f, 0.668f);
        TravelVectors[144] = new Vector3(-0.727f, 0.684f, 0.059f);
        TravelVectors[145] = new Vector3(-0.657f, 0.717f, -0.233f);
        TravelVectors[146] = new Vector3(-0.727f, 0.588f, 0.354f);
        TravelVectors[147] = new Vector3(-0.968f, 0.238f, -0.077f);
        TravelVectors[148] = new Vector3(-0.891f, 0.433f, 0.141f);
        TravelVectors[149] = new Vector3(-0.727f, 0.267f, -0.632f);
        TravelVectors[150] = new Vector3(-0.657f, 0f, -0.754f);
        TravelVectors[151] = new Vector3(-0.727f, 0.518f, -0.45f);
        TravelVectors[152] = new Vector3(-0.968f, 0f, -0.251f);
        TravelVectors[153] = new Vector3(-0.891f, 0.267f, -0.368f);
        TravelVectors[154] = new Vector3(-0.727f, -0.518f, -0.45f);
        TravelVectors[155] = new Vector3(-0.657f, -0.717f, -0.233f);
        TravelVectors[156] = new Vector3(-0.727f, -0.267f, -0.632f);
        TravelVectors[157] = new Vector3(-0.968f, -0.238f, -0.077f);
        TravelVectors[158] = new Vector3(-0.891f, -0.267f, -0.368f);
        TravelVectors[159] = new Vector3(-0.727f, -0.588f, 0.354f);
        TravelVectors[160] = new Vector3(-0.727f, -0.684f, 0.059f);
        TravelVectors[161] = new Vector3(-0.891f, -0.433f, 0.141f);

        ChangedTravelVectors = TravelVectors;



    }

    public void OnSourceUpdated(InteractionSourceUpdatedEventArgs eventData)
    {
        if (eventData.state.thumbstickPosition.x > 0.5)
        {
            forwards = 1;
            hasBeenPressed = 1;
        }else if (eventData.state.thumbstickPosition.x < -0.5)
        {
            forwards = 0;
            hasBeenPressed = 1;
        }
    }

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

        if (allowCrowd){
            int myMod = Random.Range(80, 200);
            if (counter % myMod == 0)
            {
                if (hasAlreadyHappened == 1)
                {
                    for (int m = 0; m < numOfVectors; m++)
                    {
                        Destroy(EmptyRayHolders[m]);
                    }
                }


                ChangedTravelVectors = TravelVectors;
                MakeNoise();

                for (int m = 0; m < numOfVectors; m++)
                {
                    colorArrayAlpha[m] = 1;
                }
                counter = 0;
            }
        }
        
        //if (Input.GetKeyDown("c"))
        



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
                if (Physics.Raycast(OldPosition, ChangedTravelVectors[m], out hit, stepFactor, layerMask))
                {
                    ChangedTravelVectors[m] = Vector3.Reflect(ChangedTravelVectors[m], hit.normal);

                    Vector3 Deconstruct = hit.normal;
                    Deconstruct.x = Deconstruct.x * .05f;
                    Deconstruct.y = Deconstruct.y * .05f;
                    Deconstruct.z = Deconstruct.z * .05f;

                    float NRC = GetNRC.getNRCFromCollider(hit.collider);

                    //If forwards, reduce vloume of hit ray according to NRC of surface
                    if(forwards == 1)
                        colorArrayAlpha[m] = colorArrayAlpha[m] * (1 - NRC);
                    else
                        colorArrayAlpha[m] = colorArrayAlpha[m] / (1 - NRC);

                  
                }


                // Change the NewPosition Vector's x and y components
                if (forwards == 1)
                {

                    NewPosition.x = OldPosition.x + (ChangedTravelVectors[m].x * stepFactor);
                    NewPosition.y = OldPosition.y + (ChangedTravelVectors[m].y * stepFactor);
                    NewPosition.z = OldPosition.z + (ChangedTravelVectors[m].z * stepFactor);
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
                        NewPosition.x = OldPosition.x - (ChangedTravelVectors[m].x * stepFactor);
                        NewPosition.y = OldPosition.y - (ChangedTravelVectors[m].y * stepFactor);
                        NewPosition.z = OldPosition.z - (ChangedTravelVectors[m].z * stepFactor);
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

            
            counter++;
            //Debug.Log(Time.deltaTime);

        }
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
        hasAlreadyHappened = 1;


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
            //if (forwards == 1)
            //{
            //    forwards = 0;
            //}
            //else
            //{
            //    forwards = 1;
            //}
        }
    }


    public Vector3[] GetVectorsListAtIndex(int index)
    {
        Vector3[] vects = new Vector3[numOfVectors];
        for (int i = 0; i < numOfVectors; i++)
        {
            vects[i] = myarrays[i][1] - gameObject.transform.position;
        }
        return vects;
    }

    public void clearPoints()
    {
        allowCrowd = false;                
            for (int m = 0; m < numOfVectors; m++)
            {
                Destroy(EmptyRayHolders[m]);
            }
            
            hasBeenPressed = 0;                
    }
    public void reEnable()
    {
        allowCrowd = true;

        for (int m = 0; m < numOfVectors; m++)
        {
            colorArrayAlpha[m] = 1.0f;
        }

        TravelVectors = new Vector3[numOfVectors]; //2562
        ChangedTravelVectors = new Vector3[numOfVectors]; //2562

        TravelVectors[0] = new Vector3(1f, 0f, 0f);
        TravelVectors[1] = new Vector3(0.447f, 0f, 0.894f);
        TravelVectors[2] = new Vector3(0.447f, 0.851f, 0.276f);
        TravelVectors[3] = new Vector3(0.447f, 0.526f, -0.724f);
        TravelVectors[4] = new Vector3(0.447f, -0.526f, -0.724f);
        TravelVectors[5] = new Vector3(0.447f, -0.851f, 0.276f);
        TravelVectors[6] = new Vector3(-0.447f, 0.526f, 0.724f);
        TravelVectors[7] = new Vector3(-0.447f, 0.851f, -0.276f);
        TravelVectors[8] = new Vector3(-0.447f, 0f, -0.894f);
        TravelVectors[9] = new Vector3(-0.447f, -0.851f, -0.276f);
        TravelVectors[10] = new Vector3(-0.447f, -0.526f, 0.724f);
        TravelVectors[11] = new Vector3(-1f, 0f, 0f);
        TravelVectors[12] = new Vector3(0.851f, 0f, 0.526f);
        TravelVectors[13] = new Vector3(0.526f, 0.5f, 0.688f);
        TravelVectors[14] = new Vector3(0.851f, 0.5f, 0.162f);
        TravelVectors[15] = new Vector3(0.526f, 0.809f, -0.263f);
        TravelVectors[16] = new Vector3(0.851f, 0.309f, -0.425f);
        TravelVectors[17] = new Vector3(0.526f, 0f, -0.851f);
        TravelVectors[18] = new Vector3(0.851f, -0.309f, -0.425f);
        TravelVectors[19] = new Vector3(0.526f, -0.809f, -0.263f);
        TravelVectors[20] = new Vector3(0.851f, -0.5f, 0.162f);
        TravelVectors[21] = new Vector3(0.526f, -0.5f, 0.688f);
        TravelVectors[22] = new Vector3(0f, -0.809f, 0.588f);
        TravelVectors[23] = new Vector3(0f, -0.309f, 0.951f);
        TravelVectors[24] = new Vector3(0f, 0.309f, 0.951f);
        TravelVectors[25] = new Vector3(0f, 0.809f, 0.588f);
        TravelVectors[26] = new Vector3(-0.526f, 0f, 0.851f);
        TravelVectors[27] = new Vector3(-0.526f, 0.809f, 0.263f);
        TravelVectors[28] = new Vector3(0f, 1f, 0f);
        TravelVectors[29] = new Vector3(0f, 0.809f, -0.588f);
        TravelVectors[30] = new Vector3(-0.526f, 0.5f, -0.688f);
        TravelVectors[31] = new Vector3(0f, 0.309f, -0.951f);
        TravelVectors[32] = new Vector3(0f, -0.309f, -0.951f);
        TravelVectors[33] = new Vector3(-0.526f, -0.5f, -0.688f);
        TravelVectors[34] = new Vector3(0f, -0.809f, -0.588f);
        TravelVectors[35] = new Vector3(0f, -1f, 0f);
        TravelVectors[36] = new Vector3(-0.526f, -0.809f, 0.263f);
        TravelVectors[37] = new Vector3(-0.851f, -0.309f, 0.425f);
        TravelVectors[38] = new Vector3(-0.851f, 0.309f, 0.425f);
        TravelVectors[39] = new Vector3(-0.851f, 0.5f, -0.162f);
        TravelVectors[40] = new Vector3(-0.851f, 0f, -0.526f);
        TravelVectors[41] = new Vector3(-0.851f, -0.5f, -0.162f);
        TravelVectors[42] = new Vector3(0.657f, 0.717f, 0.233f);
        TravelVectors[43] = new Vector3(0.727f, 0.518f, 0.45f);
        TravelVectors[44] = new Vector3(0.502f, 0.717f, 0.484f);
        TravelVectors[45] = new Vector3(0.968f, 0f, 0.251f);
        TravelVectors[46] = new Vector3(0.891f, 0.267f, 0.368f);
        TravelVectors[47] = new Vector3(0.968f, 0.238f, 0.077f);
        TravelVectors[48] = new Vector3(0.502f, 0.238f, 0.831f);
        TravelVectors[49] = new Vector3(0.727f, 0.267f, 0.632f);
        TravelVectors[50] = new Vector3(0.657f, 0f, 0.754f);
        TravelVectors[51] = new Vector3(0.657f, 0.443f, -0.61f);
        TravelVectors[52] = new Vector3(0.727f, 0.588f, -0.354f);
        TravelVectors[53] = new Vector3(0.502f, 0.681f, -0.532f);
        TravelVectors[54] = new Vector3(0.891f, 0.433f, -0.141f);
        TravelVectors[55] = new Vector3(0.968f, 0.147f, -0.203f);
        TravelVectors[56] = new Vector3(0.502f, 0.864f, 0.03f);
        TravelVectors[57] = new Vector3(0.727f, 0.684f, -0.059f);
        TravelVectors[58] = new Vector3(0.657f, -0.443f, -0.61f);
        TravelVectors[59] = new Vector3(0.727f, -0.155f, -0.668f);
        TravelVectors[60] = new Vector3(0.502f, -0.296f, -0.813f);
        TravelVectors[61] = new Vector3(0.891f, 0f, -0.455f);
        TravelVectors[62] = new Vector3(0.968f, -0.147f, -0.203f);
        TravelVectors[63] = new Vector3(0.502f, 0.296f, -0.813f);
        TravelVectors[64] = new Vector3(0.727f, 0.155f, -0.668f);
        TravelVectors[65] = new Vector3(0.657f, -0.717f, 0.233f);
        TravelVectors[66] = new Vector3(0.727f, -0.684f, -0.059f);
        TravelVectors[67] = new Vector3(0.502f, -0.864f, 0.03f);
        TravelVectors[68] = new Vector3(0.891f, -0.433f, -0.141f);
        TravelVectors[69] = new Vector3(0.968f, -0.238f, 0.077f);
        TravelVectors[70] = new Vector3(0.502f, -0.681f, -0.532f);
        TravelVectors[71] = new Vector3(0.727f, -0.588f, -0.354f);
        TravelVectors[72] = new Vector3(0.727f, -0.267f, 0.632f);
        TravelVectors[73] = new Vector3(0.502f, -0.238f, 0.831f);
        TravelVectors[74] = new Vector3(0.891f, -0.267f, 0.368f);
        TravelVectors[75] = new Vector3(0.502f, -0.717f, 0.484f);
        TravelVectors[76] = new Vector3(0.727f, -0.518f, 0.45f);
        TravelVectors[77] = new Vector3(-0.252f, -0.443f, 0.861f);
        TravelVectors[78] = new Vector3(0.009f, -0.588f, 0.809f);
        TravelVectors[79] = new Vector3(-0.252f, -0.681f, 0.687f);
        TravelVectors[80] = new Vector3(0.272f, -0.433f, 0.859f);
        TravelVectors[81] = new Vector3(0.252f, -0.147f, 0.957f);
        TravelVectors[82] = new Vector3(0.252f, -0.864f, 0.436f);
        TravelVectors[83] = new Vector3(0.272f, -0.684f, 0.677f);
        TravelVectors[84] = new Vector3(0.272f, 0.684f, 0.677f);
        TravelVectors[85] = new Vector3(0.252f, 0.864f, 0.436f);
        TravelVectors[86] = new Vector3(0.252f, 0.147f, 0.957f);
        TravelVectors[87] = new Vector3(0.272f, 0.433f, 0.859f);
        TravelVectors[88] = new Vector3(-0.252f, 0.681f, 0.687f);
        TravelVectors[89] = new Vector3(0.009f, 0.588f, 0.809f);
        TravelVectors[90] = new Vector3(-0.252f, 0.443f, 0.861f);
        TravelVectors[91] = new Vector3(-0.272f, 0.155f, 0.95f);
        TravelVectors[92] = new Vector3(-0.502f, 0.296f, 0.813f);
        TravelVectors[93] = new Vector3(-0.009f, 0f, 1f);
        TravelVectors[94] = new Vector3(-0.502f, -0.296f, 0.813f);
        TravelVectors[95] = new Vector3(-0.272f, -0.155f, 0.95f);
        TravelVectors[96] = new Vector3(-0.252f, 0.955f, -0.155f);
        TravelVectors[97] = new Vector3(-0.272f, 0.951f, 0.146f);
        TravelVectors[98] = new Vector3(-0.502f, 0.864f, -0.03f);
        TravelVectors[99] = new Vector3(-0.009f, 0.951f, 0.309f);
        TravelVectors[100] = new Vector3(0.252f, 0.955f, 0.155f);
        TravelVectors[101] = new Vector3(-0.502f, 0.681f, 0.532f);
        TravelVectors[102] = new Vector3(-0.272f, 0.855f, 0.441f);
        TravelVectors[103] = new Vector3(0.272f, 0.855f, -0.441f);
        TravelVectors[104] = new Vector3(0.252f, 0.681f, -0.687f);
        TravelVectors[105] = new Vector3(0.272f, 0.951f, -0.146f);
        TravelVectors[106] = new Vector3(-0.252f, 0.864f, -0.436f);
        TravelVectors[107] = new Vector3(0.009f, 0.951f, -0.309f);
        TravelVectors[108] = new Vector3(-0.252f, 0.147f, -0.957f);
        TravelVectors[109] = new Vector3(-0.272f, 0.433f, -0.859f);
        TravelVectors[110] = new Vector3(-0.502f, 0.238f, -0.831f);
        TravelVectors[111] = new Vector3(-0.009f, 0.588f, -0.809f);
        TravelVectors[112] = new Vector3(0.252f, 0.443f, -0.861f);
        TravelVectors[113] = new Vector3(-0.502f, 0.717f, -0.484f);
        TravelVectors[114] = new Vector3(-0.272f, 0.684f, -0.677f);
        TravelVectors[115] = new Vector3(0.273f, -0.155f, -0.95f);
        TravelVectors[116] = new Vector3(0.251f, -0.443f, -0.861f);
        TravelVectors[117] = new Vector3(0.273f, 0.155f, -0.95f);
        TravelVectors[118] = new Vector3(-0.252f, -0.147f, -0.957f);
        TravelVectors[119] = new Vector3(0.009f, 0f, -1f);
        TravelVectors[120] = new Vector3(-0.251f, -0.864f, -0.436f);
        TravelVectors[121] = new Vector3(-0.272f, -0.684f, -0.677f);
        TravelVectors[122] = new Vector3(-0.502f, -0.717f, -0.484f);
        TravelVectors[123] = new Vector3(-0.009f, -0.588f, -0.809f);
        TravelVectors[124] = new Vector3(0.252f, -0.681f, -0.687f);
        TravelVectors[125] = new Vector3(-0.502f, -0.238f, -0.831f);
        TravelVectors[126] = new Vector3(-0.272f, -0.433f, -0.859f);
        TravelVectors[127] = new Vector3(0.272f, -0.951f, -0.146f);
        TravelVectors[128] = new Vector3(0.251f, -0.955f, 0.155f);
        TravelVectors[129] = new Vector3(0.272f, -0.855f, -0.441f);
        TravelVectors[130] = new Vector3(-0.252f, -0.955f, -0.155f);
        TravelVectors[131] = new Vector3(0.009f, -0.951f, -0.309f);
        TravelVectors[132] = new Vector3(-0.272f, -0.855f, 0.441f);
        TravelVectors[133] = new Vector3(-0.502f, -0.681f, 0.532f);
        TravelVectors[134] = new Vector3(-0.009f, -0.951f, 0.309f);
        TravelVectors[135] = new Vector3(-0.502f, -0.864f, -0.03f);
        TravelVectors[136] = new Vector3(-0.272f, -0.951f, 0.146f);
        TravelVectors[137] = new Vector3(-0.968f, 0.147f, 0.203f);
        TravelVectors[138] = new Vector3(-0.891f, 0f, 0.455f);
        TravelVectors[139] = new Vector3(-0.968f, -0.147f, 0.203f);
        TravelVectors[140] = new Vector3(-0.727f, 0.155f, 0.668f);
        TravelVectors[141] = new Vector3(-0.657f, 0.443f, 0.61f);
        TravelVectors[142] = new Vector3(-0.657f, -0.443f, 0.61f);
        TravelVectors[143] = new Vector3(-0.727f, -0.155f, 0.668f);
        TravelVectors[144] = new Vector3(-0.727f, 0.684f, 0.059f);
        TravelVectors[145] = new Vector3(-0.657f, 0.717f, -0.233f);
        TravelVectors[146] = new Vector3(-0.727f, 0.588f, 0.354f);
        TravelVectors[147] = new Vector3(-0.968f, 0.238f, -0.077f);
        TravelVectors[148] = new Vector3(-0.891f, 0.433f, 0.141f);
        TravelVectors[149] = new Vector3(-0.727f, 0.267f, -0.632f);
        TravelVectors[150] = new Vector3(-0.657f, 0f, -0.754f);
        TravelVectors[151] = new Vector3(-0.727f, 0.518f, -0.45f);
        TravelVectors[152] = new Vector3(-0.968f, 0f, -0.251f);
        TravelVectors[153] = new Vector3(-0.891f, 0.267f, -0.368f);
        TravelVectors[154] = new Vector3(-0.727f, -0.518f, -0.45f);
        TravelVectors[155] = new Vector3(-0.657f, -0.717f, -0.233f);
        TravelVectors[156] = new Vector3(-0.727f, -0.267f, -0.632f);
        TravelVectors[157] = new Vector3(-0.968f, -0.238f, -0.077f);
        TravelVectors[158] = new Vector3(-0.891f, -0.267f, -0.368f);
        TravelVectors[159] = new Vector3(-0.727f, -0.588f, 0.354f);
        TravelVectors[160] = new Vector3(-0.727f, -0.684f, 0.059f);
        TravelVectors[161] = new Vector3(-0.891f, -0.433f, 0.141f);

        ChangedTravelVectors = TravelVectors;

    }
}