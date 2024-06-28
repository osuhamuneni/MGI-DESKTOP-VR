using System.Collections;
using System.Collections.Generic;
using basirua;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PetrosianRadius))]
public class PetrosianRadiusEditor : Editor
{
    public override void OnInspectorGUI() {
        
        PetrosianRadius pr = (PetrosianRadius)target;
        base.OnInspectorGUI();
        
        if (GUILayout.Button("Calculate Radius"))
        {
            var bf = new BrightestFinder();
            Renderer R = pr.gameObject.GetComponent<Renderer>();

            Texture2D tex = (Texture2D)R.sharedMaterial.mainTexture;
            
            float petrosianRadius = bf.RadialProfileOfLight(tex, pr.galaxyPos, pr.stepMult, pr.steps);
            tex = bf.DrawDebugCircle(tex, pr.galaxyPos, petrosianRadius);
            tex.Apply();
            
            R.sharedMaterial.SetTexture("_MainTex", tex);
            
            Debug.Log("Drew on tex");
        }
        
        if (GUILayout.Button("Petrosian Step debug"))
        {
            var bf = new BrightestFinder();
            Renderer R = pr.gameObject.GetComponent<Renderer>();

            Texture2D tex = (Texture2D)R.sharedMaterial.mainTexture;
            
            tex = bf.RadialProfileOfLightDebug(tex, pr.galaxyPos, pr.stepMult, pr.steps, pr.name);
            tex.Apply();
            
            R.sharedMaterial.SetTexture("_MainTex", tex);
            
            Debug.Log("Drew on tex");
        }
        
        if (GUILayout.Button("Draw circle"))
        {
            var bf = new BrightestFinder();
            Renderer R = pr.gameObject.GetComponent<Renderer>();

            Texture2D tex = (Texture2D)R.sharedMaterial.mainTexture;

            tex = bf.DrawDebugCircle(tex, pr.galaxyPos, pr.debugCircleSize);
            tex.Apply();
            
            R.sharedMaterial.SetTexture("_MainTex", tex);
            
            Debug.Log("Drew circle");
        }
        
        if (GUILayout.Button("Reset texture"))
        {
            FITSObject FO3 = pr.gameObject.GetComponent<FITSObject>();

            FO3.Start();
            
            Debug.Log("Resetted tex");
        }
    }
}
