using System.Collections;
using System.Collections.Generic;
using FitsReader;
using UnityEngine;
using UnityEngine.UI;

public class ImageManager : MonoBehaviour
{
    public FITSObject target;
    public Slider brightness;
   /*public Slider treshhold;*/
    public Slider redValue;
    public Slider greenValue;
    public Slider blueValue;

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
            
       Material mat =  target.transform.GetComponent<MeshRenderer>().materials[0];
       mat.SetFloat("brightness", brightness.value);
       mat.SetFloat("redMultiplier", redValue.value);
       mat.SetFloat("greenMultiplier", greenValue.value);
       mat.SetFloat("blueMultiplier", blueValue.value);
    }

    public void updateFitsData()
    {
        Debug.Log("Changing Image !");
        
       Material mat =  target.transform.GetComponent<MeshRenderer>().materials[0];
       mat.SetFloat("brightness", brightness.value);
         mat.SetFloat("redMultiplier", redValue.value);
       mat.SetFloat("greenMultiplier", greenValue.value);
       mat.SetFloat("blueMultiplier", blueValue.value);
        /*Texture2D Texture = FITSReader.Texture2DFromFITS(target.ImageData);

        Texture.wrapMode = target.WrapMode;

        Renderer R = target.transform.GetComponent<Renderer>();
        R.material = new Material(R.material);
        R.sharedMaterial.SetTexture("_MainTex", Texture);
        R.sharedMaterial.SetTextureScale("_MainTex", target.TextureScale);*/
    }
    // Update is called once per frame
}
