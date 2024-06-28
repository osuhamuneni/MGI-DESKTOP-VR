using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using System;

public struct Data
{
    public string Name;
    public string ImageURL;
    public Actor[] Actors;
}

[Serializable]
public struct Actor
{
    public Pos pos;
}

[Serializable]
public struct Pos
{
    public float x;
    public float y;
    public float z;
}
public struct CartPos
{
    public float u;
    public float v;
    public float w;
}

public class GalaxyJson : MonoBehaviour
{
    [SerializeField] public float RA ; //{ get; set; }
    [SerializeField] public float DE; //{ get; set; }
    [SerializeField] public double PI_DBL=22; //{ get; set; }
    public Vector2 TextureScale = Vector2.one;
    public Vector3 originalPosition;
    public Vector3 oPosition;
    public Quaternion originalRotation;
    public Vector3 originalScale;
    CartPos cartpos;
    //public double cartesianPositioning;
     Text uiNameText ;
    [SerializeField] RawImage uiRawImage;
    public Texture2D resultTexture;


    
    string jsonURL = "https://drive.google.com/uc?export=download&id=1zKB4H8tXCPeAb4lVFASYyb8FrSuNJTsx";
    private void cartesianPositioningCalc()
    {
        double RA_rad = 283.45; //*(Math.PI_DBL / 180);
        double DE_rad = 43.883333; //* (Math.PI_DBL / 180);

        cartpos.u = (float)(Math.Cos(DE_rad)) * (float)(Math.Cos(RA_rad));
        cartpos.v = (float)(Math.Cos(DE_rad)) * (float)(Math.Sin(RA_rad));
        cartpos.w = (float)(Math.Sin(DE_rad));
        Debug.Log("initial cart position" + cartpos.u);
        Debug.Log("initial cart position" + cartpos.w);
    }
    void Start()
    {
        StartCoroutine(GetData(jsonURL));
    }

    IEnumerator GetData(string url)
    {
        Vector3 originalPosition;
        UnityWebRequest request = UnityWebRequest.Get(url);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            // error ...

        }
        else
        {
           // success...
            Data data = JsonUtility.FromJson<Data>(request.downloadHandler.text);


             //print data in UI
             //uiNameText.text = data.Name ;
            Debug.Log(data);
            //Debug.Log(data.Actors.pos);

            originalPosition = transform.position;
            //var json = JsonConvert.SerializeObject(originalPosition);
            //Debug.Log("Position as JSON: " + json);
            //var oPosition = JsonConvert.DeserializeObject<List<Vector3>>(json); // To Deserialise

            // Load image:
            StartCoroutine(GetImage(data));
        }

        // Clean up any resources it is using.
        request.Dispose();
    }

    IEnumerator GetImage(Data data)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(data.ImageURL);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            // error ...
            Debug.Log("from galaxyjson script www connected " );
        }
        else
        {
            //success...
            resultTexture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            Debug.Log("from galaxyjson scrip " + resultTexture);
       
       
       
       /*     Renderer R = GetComponent<Renderer>();
            R.material = new Material(R.material);
            R.sharedMaterial.SetTexture("_MainTex", Texture);
            R.sharedMaterial.SetTextureScale("_MainTex", TextureScale);*/
            
            //originalPosition = transform.position;
            //originalRotation = transform.rotation;
            //originalScale = transform.localScale;
            // this position is hard codeded but needs to be from the JSON FILE
            /*Debug.Log("working so far");
            //firstposition=data.Actors[0].pos.x
            Debug.Log("firstposition" + data.Actors[0].pos.x);
            Debug.Log("Secondposition" + data.Actors[0].pos.y);
*/
            // Vector3 position = new Vector3(data.Actors[0].pos.x, data.Actors[0].pos.y, data.Actors[0].pos.z);
            // transform.position = position;
            Vector3 position = new Vector3(cartpos.u, cartpos.v, cartpos.w);
                transform.position = position;
        }

        // Clean up any resources it is using.
        request.Dispose();
    }

}
