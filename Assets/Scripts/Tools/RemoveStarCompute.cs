using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveStarCompute : MonoBehaviour
{
    public ComputeShader computerShader;
    public RenderTexture renderTexture;
    // Start is called before the first frame update
    void Start()
    {
        //renderTexture = new RenderTexture(galaxyTexture.width, galaxyTexture.height, 24);
        //renderTexture.enableRandomWrite = true;
        //Graphics.Blit(galaxyTexture, renderTexture);
        ////renderTexture.Create();

        //computerShader.SetTexture(0, "Result", renderTexture);

        //computerShader.Dispatch(0, renderTexture.width / 8, renderTexture.height / 8, 1);
        LoadComputeShaders();
    }

    private void LoadComputeShaders()
    {
        if (computerShader == null)
        {
            computerShader = Resources.Load<ComputeShader>("Shaders/RemoveStars");
            if (computerShader == null)
                Debug.LogError("Failed to load <RemoveStars> Compute Shader");
        }
    }

    public void CreateRenderTexture(Texture2D texture)
    {
        renderTexture = new RenderTexture(texture.width, texture.height, 24);
        renderTexture.enableRandomWrite = true;
        Graphics.Blit(texture, renderTexture);

        computerShader.SetTexture(0, "Result", renderTexture);
    }

    public void DispatchShader(Texture2D texture)
    {
        //this might not be correct
        computerShader.Dispatch(0, renderTexture.width / 8, renderTexture.height / 8, 1);
        //Graphics.CopyTexture(renderTexture, texture);//copy modified texture back into og texture
        Debug.Log("Is Readable: " + texture.isReadable);
        toTexture2D(renderTexture,texture);
    }

    void toTexture2D(RenderTexture rTex,Texture2D texture)
    {
        // ReadPixels looks at the active RenderTexture.
        RenderTexture.active = rTex;
        texture.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        texture.Apply();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
