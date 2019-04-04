using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Autodesk.Forge.ARKit;
using SimpleJSON;

public static class GetNRC
{
    public static float getNRCFromCollider(Collider collider)
    {

        float result = 0.5f;
        ForgeProperties fp = collider.gameObject.GetComponentInParent<ForgeProperties>();
                
        if (fp != null)
        {
            JSONNode temp = fp.Properties["props"];
            //Debug.Log("testing");
            foreach (var v in temp.Values)
            {
                if (v["name"] == "NRC")
                {
                    string val = v["value"];
                    float tryResult;
                    if (float.TryParse(val, out tryResult))
                        result = tryResult;
                    
                }
            }
        } else
        {
            MaterialNRC m = collider.gameObject.GetComponent<MaterialNRC>();
            if (m != null)    
            {
                result = m.NRC;
            }
        }

        return result;
    }
}
