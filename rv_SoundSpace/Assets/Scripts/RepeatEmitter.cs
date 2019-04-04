using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepeatEmitter : MonoBehaviour
{

    GameObject emitterPrefab;
    TriangleList.EmitType emitType = TriangleList.EmitType.LowRes;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void startEmitting(GameObject emitter, TriangleList.EmitType eType)
    {
        emitType = eType;
        emitterPrefab = emitter;
        GameObject newEmitter = Instantiate(emitterPrefab, gameObject.transform.position, gameObject.transform.rotation);
        if(emitType == TriangleList.EmitType.LowRes)
        {
            newEmitter.GetComponent<WaveEmitter>().MakeNoise();
            //StartCoroutine(emit(3));
        } else
        {
            newEmitter.GetComponent<WaveEmitterBeam>().MakeNoise();
        }
            
       
    }

    IEnumerator emit(int seconds)
    {
       
        yield return new WaitForSeconds(seconds);
        GameObject newEmitter = Instantiate(emitterPrefab, gameObject.transform.position, Quaternion.identity);
        newEmitter.GetComponent<WaveEmitter>().MakeNoise();

    }
}
