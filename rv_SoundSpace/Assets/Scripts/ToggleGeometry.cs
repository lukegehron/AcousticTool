using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleGeometry : MonoBehaviour
{
    public GameObject gameobj1;
    public GameObject gameobj2;
    public GameObject gameobj3;
    public int goOn = 1;

    // Start is called before the first frame update
    void Start()
    {
        gameobj1.SetActive(false);
        gameobj2.SetActive(false);
        gameobj3.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetKeyDown("t"))
        {
            if (goOn == 0)
            {
                gameobj1.SetActive(false);
                gameobj2.SetActive(false);
                gameobj3.SetActive(false);
                goOn = 1;
            }
            else if (goOn == 1)
            {
                gameobj1.SetActive(true);
                gameobj2.SetActive(false);
                gameobj3.SetActive(false);
                goOn = 2;
            }
            else if (goOn == 2)
            {
                gameobj1.SetActive(false);
                gameobj2.SetActive(true);
                gameobj3.SetActive(false);
                goOn = 3;
            }
            else
            {
                gameobj1.SetActive(false);
                gameobj2.SetActive(false);
                gameobj3.SetActive(true);
                goOn = 0;
            }
            
            
        }
    }
}
