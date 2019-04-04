using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshWave : MonoBehaviour
{
    public bool enableMesh = true;
    private bool meshisVisible = true;
    private WaveEmitter waveEmitter;
   
    // Start is called before the first frame update
    void Start()
    {       
        waveEmitter = GetComponent<WaveEmitter>();
        GameObject main = GameObject.Find("MainSoundControl");
        if (main != null)
        {
            meshisVisible = main.GetComponent<MainSoundControl>().MeshesAreVisible;
            GetComponent<MeshRenderer>().enabled = meshisVisible;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(enableMesh)
            GenerateMesh();
    }

    public void GenerateMesh()
    {
        var mesh = new Mesh();
        if (!transform.GetComponent<MeshFilter>()) //If you havent got any meshrenderer or filter
        {           
            transform.gameObject.AddComponent<MeshFilter>();
        }

        transform.GetComponent<MeshFilter>().mesh = mesh;


        //create empty color list
        Color[] colors = new Color[TriangleList.GetVectorCount()];
        for (int i=0;i< TriangleList.GetVectorCount(); i++)
        {
            float alpha = waveEmitter.colorArrayAlpha[i];
            float maxAlpha = 1f;
            colors[i] = new Color(0f, 0f, 1f, alpha * maxAlpha);
        }
        //

        
        mesh.vertices = waveEmitter.GetVectorsListAtIndex(0);
        mesh.triangles = TriangleList.GetTriangleList();
        mesh.colors = colors;
        
        mesh.RecalculateNormals();

        Color c = transform.GetComponent<MeshRenderer>().material.color;
        float newAlpha = waveEmitter.colorArrayAlpha[0] * 0.1f;
        float rimAlpha = waveEmitter.colorArrayAlpha[0] * 0.2f;
        Color c_base = new Color(newAlpha, newAlpha, newAlpha);
        float a = waveEmitter.colorArrayAlpha[0];
        
        c.a = waveEmitter.colorArrayAlpha[0];
        c.r = 0.09131362f * rimAlpha;
        c.g = 0.2332605f * rimAlpha;
        c.b = 0.3396226f * rimAlpha;
       
        

        transform.GetComponent<MeshRenderer>().material.SetColor("_Color", c_base);
        transform.GetComponent<MeshRenderer>().material.SetColor("_RimColor", c);
       
    }
       
    
}
