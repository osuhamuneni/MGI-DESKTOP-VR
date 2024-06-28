using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FitsReader;
using nom.tam.fits;
using System;
using UnityEngine.Networking;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using basirua;
using ICSharpCode.SharpZipLib.BZip2;
using System.Linq;

public class FITSObject : MonoBehaviour,ITextureHandler
{
    public ECameraFocus GalaxyCameraFocus;
    public bool isLoadOnStart = true;
    private GameObject cameraControl;
    public FITSImage[] ImageData;
    [SerializeField]private List<FITSImage> m_imageDataBckList;
    [Header("Texture settings")]
    public TextureWrapMode WrapMode;
    public GameObject objectToInstatiate;
    public float scale;
    public Texture2D fitsTexture;
    RenderTexture renderTexture;
    public ComputeShader Colorize;
    public ComputeShader Offset;
    public ComputeShader Combine;
    public ComputeShader remap;
    //public ComputeShader DarkenShader;
    private bool m_EnableTexSave = true;
    private bool isImageDataBackup = false;

    [Header("Data Loader")]
    public bool loadFromPath = false;
    public Texture2D loadTexture;
    public string fitsPath;
    private Camera Camera => Camera.main;

    public GameObject CameraControl { get => cameraControl; set => cameraControl = value; }
    public List<FITSImage> ImageDataBckList { get => m_imageDataBckList; set => m_imageDataBckList = value; }
    public bool EnableTexSave { get => m_EnableTexSave; set => m_EnableTexSave = value; }
    private GalaxyInfoReader _galaxyInfoReader;
    private UIFaceCamera _uiFaceCamera;
    private GalaxyInfos _galaxyInfos;

    private string[] _backupPath;
    public string GameObjectName { get { return gameObject.name.Replace("(Clone)", ""); } }

    private void Awake()
    {
        if (ConfigManager.Instance == null)
            return;

        scale = ConfigManager.Instance.configData.scale;
        for (int i = 0; i < ImageData.Length; i++)
        {
            ImageData[i].Settings.ColorizeMode = ConfigManager.Instance.configData.colorizeMode;
            ImageData[i].Settings.Denoise = ConfigManager.Instance.configData.denoiseColorisation;
        }

        if (ImageData[0].Settings.SourceMode == FITSSourceMode.Disk)
        {
            fitsPath = Application.streamingAssetsPath;
            _backupPath = new string[ImageData.Length];
            for (int i = 0; i < ImageData.Length; i++)
            {
                _backupPath[i] = $"{Application.streamingAssetsPath}/{ImageData[i].Path}";
                ImageData[i].BackupPath = _backupPath[i];
            }
        }
        else
        {
            fitsPath = Application.persistentDataPath + "/" + gameObject.name + "/" + "fits";
            if (!Directory.Exists(fitsPath))
            {
                Directory.CreateDirectory(fitsPath);
            }
        }

        EnableTexSave = true;
    }

    private void LoadComputeShaders()
    {
        if (Colorize == null)
        {
            Colorize = Resources.Load<ComputeShader>("Shaders/Colorize");
            if (Colorize == null)
                Debug.LogError("Failed to load <Colorize> Compute Shader");
        }

        if (Offset == null)
        {
            Offset = Resources.Load<ComputeShader>("Shaders/ImageProccessing");
            if (Offset == null)
                Debug.LogError("Failed to load <ImageProccessing> Compute Shader");
        }

        if (Combine == null)
        {
            Combine = Resources.Load<ComputeShader>("Shaders/MaximumAndMinimum");
            if (Combine == null)
                Debug.LogError("Failed to load <MaximumAndMinimum> Compute Shader");
        }

        if (remap == null)
        {
            remap = Resources.Load<ComputeShader>("Shaders/Remap");
            if (remap == null)
                Debug.LogError("Failed to load <Remap> Compute Shader");
        }
    }

    public void Start()
    {
        _galaxyInfoReader = FindObjectOfType<GalaxyInfoReader>();
        LoadComputeShaders();

        isImageDataBackup = false;

        if (isLoadOnStart)
        {
            Texture2D tex = LoadFromPath(512, 512);
            if (tex != null)
            {
                fitsTexture = tex;

                for (int i = 0; i < ImageData.Length; i++)
                {
                    if (ImageData[i].Settings.SourceMode == FITSSourceMode.Web && ImageData[i].Path.ToLower().Contains("http"))
                    {
                        //ImageData[i].Path = $"{fitsPath}/{GameObjectName}_fits_{i}.fits";
                        ImageData[i].Path = $"/{GameObjectName}/fits/{GameObjectName}_fits_{i}.fits";
                        //ImageData[i].Settings.SourceMode = FITSSourceMode.Disk;
                    }
                }

                FITSReader.loadObjectFromFits(ImageData, gameObject, scale);
                ProcessTexturesFromPath();
            }
            else
            {
                LoadFITS(false);
            }
        }
        
        Debug.Log($"{gameObject.name} start-up");

        if (fitsTexture != null)
        {
            BackupImageData();
        }

        StartCoroutine(WaitForSecondsRoutine(1f));
    }

    private IEnumerator WaitForSecondsRoutine(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        // Code here will execute after the specified seconds
        if (gameObject.GetComponent<GalaxiesFaceCamera>() == null)
        {
            gameObject.AddComponent<GalaxiesFaceCamera>().FaceCamera();
        }
    }

    public void BackupImageData()
    {
        if (!isImageDataBackup)
        {
            ImageDataBckList = new List<FITSImage>();
            ImageDataBckList = ImageData.ToList();
            foreach (var image in ImageDataBckList)
            {
                image.Settings.SourceMode = FITSSourceMode.Disk;
            }

            isImageDataBackup = true;
        }
    }

    public void LoadFitsImages()
    {
        //loading from json
        LoadFITS(false);
    }

    public void LoadFitsImages(bool load)
    {
        LoadFITS(load);
    }  

    private void LoadFITS(bool webPass = false)
    {
        if (ImageData.Length == 0)
        {
            Debug.LogError("ImageData length on object: " + gameObject.name + " is 0, FITSObject must have at least 1 fits file to load");
            return; // if there are no images for us to load then exit the load function so that game doesn't scrash.
        }

        if (_galaxyInfos == null)
            _galaxyInfos = _galaxyInfoReader.GetGalaxyInfo(gameObject.name);

        if (GalaxyManager.manager.LoadFromCSV && !webPass)
        {
            for (int i = 0; i < ImageData.Length; i++)
            {
                ImageData[i].Settings.SourceMode = FITSSourceMode.Web;
                switch (ImageData[i].colorBand)
                {
                    case ColorBand.Red:
                        ImageData[i].Path = _galaxyInfos.RBand;
                        break;
                    case ColorBand.Green:
                        ImageData[i].Path = _galaxyInfos.GBand;
                        break;
                    case ColorBand.Blue:
                        ImageData[i].Path = _galaxyInfos.IBand;
                        break;
                    case ColorBand.UV:
                        ImageData[i].Path = _galaxyInfos.UBand;
                        break;
                    case ColorBand.XRay:
                        ImageData[i].Path = _galaxyInfos.ZBand;
                        break;
                }
            }
        }

        if (!webPass)
        {
            for (int i = 0; i < ImageData.Length; i++)
            {
                if (ImageData[i].Settings.SourceMode == FITSSourceMode.Web)
                {
                    StartCoroutine(PreloadFITS(ImageData));
                    return;
                }
            }

        }
        UpdateFITS(ImageData);
    }

    private void GetUIObject(GameObject go)
    {
        GameObject uiObject = Camera.gameObject.transform.GetChild(0).gameObject;
        _uiFaceCamera = uiObject.GetComponent<UIFaceCamera>();
        uiObject.GetComponent<GalaxyInfoDisplay>().InitGalaxyInfo(_galaxyInfos);
    }

    public void ShowHideUIInfos(bool show)
    {
        GetUIObject(gameObject);
        _uiFaceCamera.gameObject.SetActive(show);
    }

    /// <summary>
    /// Async UpdateFITS
    /// </summary>
    /// <param name="imageData"></param>
    async public void UpdateFITS(FITSImage[] imageData, int length = -1, bool recut = true)
    {
        renderTexture = new RenderTexture(1536, 1024, 0);
        length = length == -1 ? imageData.Length : length;

        for (int i = 0; i < length; i++)
        {
            string path = imageData[i].Settings.SourceMode == FITSSourceMode.Disk ? imageData[i].BackupPath : imageData[i].Path;
            Fits FITSObject = FITSReader.GetFitsFromPath(path); // this  loads the image from the path

            Debug.Log($"this is the path of {gameObject.name}, {path}");

            ImageHDU HDU = (ImageHDU)FITSObject.readHDU();
            ImageData HDUIData = (ImageData)HDU.Data;

            Array[] dataArray = (Array[])HDUIData.DataArray;
            Array[] result = await LoopOverData(dataArray);
            float[] intList = (float[])result[0];
            float min = (float)result[1].GetValue(0);
            float max = (float)result[1].GetValue(1);
            int XDimension = dataArray[0].Length;
            int YDimension = dataArray.Length;

            float[] doublePixels = intList;

            if (i == 0)
            {
                renderTexture = new RenderTexture(XDimension, YDimension, 0);
                renderTexture.enableRandomWrite = true;
            }

            ComputeBuffer pixelsRemapBuffer = new ComputeBuffer(doublePixels.Length, sizeof(float));

            float[] remapResult = (float[])doublePixels.Clone();

            pixelsRemapBuffer.SetData(remapResult);
            remap.SetBuffer(0, "pixels", pixelsRemapBuffer);
            remap.SetFloat("max", max);
            remap.SetFloat("min", min);
            remap.Dispatch(0, 65535, 1, 1);

            pixelsRemapBuffer.GetData(remapResult);
            pixelsRemapBuffer.Release();
            RenderTexture minTex = new RenderTexture(1536, 1024, 0);
            minTex.enableRandomWrite = true;

            RenderTexture temp = renderTexture;
            temp.enableRandomWrite = true;
            ComputeBuffer pixelsColorizeBuffer = new ComputeBuffer(remapResult.Length, sizeof(float));
            ComputeBuffer blackBuffer = new ComputeBuffer(1, sizeof(float));
            pixelsColorizeBuffer.SetData(remapResult);
            float[] blacks = new float[1] { 0 };
            blackBuffer.SetData(blacks);
            Colorize.SetTexture(0, "Result", temp);
            Colorize.SetBuffer(0, "pixels", pixelsColorizeBuffer);
            Colorize.SetBuffer(0, "blacks", blackBuffer);
            Colorize.SetFloat("width", XDimension);
            Colorize.SetFloat("boost", ImageData[i].Settings.LowBoost);
            Colorize.SetFloat("threshold", ImageData[i].Settings.ThresholdSettings.Threshold);
            Colorize.SetFloat("XOffset", ImageData[i].Settings.Offset.x);
            Colorize.SetFloat("YOffset", ImageData[i].Settings.Offset.y);
            Colorize.SetVector("RGBMultipliers", ImageData[i].Settings.RGBMultiplier);
            Colorize.Dispatch(0, 512, 512, 1);
            pixelsColorizeBuffer.GetData(remapResult);
            blackBuffer.GetData(blacks);
            pixelsColorizeBuffer.Release();
            blackBuffer.Release();
        }

        FITSReader.loadObjectFromFits(imageData, gameObject, scale);

        fitsTexture = toTexture2D(renderTexture);
        if (fitsTexture != null)
        {
            fitsTexture.wrapMode = WrapMode;
            fitsTexture.Apply();
            SaveToPath(fitsTexture);

            CutNPlaceOnCenter(recut);
            BackupImageData();
        }
    }

    private void ProcessTexturesFromPath()
    {     
        fitsTexture = loadTexture;
        fitsTexture.wrapMode = WrapMode;
        fitsTexture.Apply();

        CutNPlaceOnCenter();

        for(int i = 0; i <= ImageData.Length - 1; i++)
        {
            ImageData[i].Path = $"{fitsPath}/{GameObjectName}_fits_{i}.fits";
        }        
    }

    private Vector2 brightestPoint;
    private float pr;

    private void CutNPlaceOnCenter(bool recut = true)
    {
        BrightestFinder bf = new BrightestFinder();
        if(recut)
        {
            brightestPoint = bf.FindBrightestAreaCenter(fitsTexture, 0.6f);

            // -- CUT AND PLACE GALAXY ON CENTER !!!!!!!! --
            pr = bf.RadialProfileOfLight(fitsTexture, brightestPoint, 80f, 7);
            pr *= 2.5f;
        }

        fitsTexture = bf.ProcessTexture(fitsTexture, 1024, 1024, brightestPoint);
        fitsTexture.filterMode = FilterMode.Trilinear;
        fitsTexture.Apply();

        Shader shader = Shader.Find("Unlit/GalaxyCutShader");
        if (shader != null)
        {
            Material mat = new Material(shader);
            mat.SetTexture("_MainTex", fitsTexture);

            if (!ConfigManager.Instance.configData.cropGalaxies)
                mat.SetFloat("_Scale", 750);
            else
                mat.SetFloat("_Scale", pr);

            Renderer rend = GetComponent<Renderer>();
            if(rend != null)
                rend.material = mat;
        }
        else
        {
            Debug.LogError("<Unlit/GalaxyCutShader> has not been found or loaded correctly");
        }
        SetGalaxyScaling(pr);
    }

    private void SetGalaxyScaling(float radialProfile)
    {
        return;
        float scaleRatio = radialProfile;
        float scaling = 2 * scaleRatio;
        float convertedScale = Mathf.Clamp(scaling, 0.5f, 2f) * 2;//prevent galaxies from being to small or big
        transform.localScale = Vector3.one * convertedScale;

        GetComponent<GalaxiesFaceCamera>().FaceCamera();
    }

    private Texture2D toTexture2D(RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGB24, false);
        // ReadPixels looks at the active RenderTexture.
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
        return tex;
    }

    static async Task<Array[]> LoopOverData(Array[] dataArray)
    {
        // we're still on unity thread here
        return await Task.Run(
            () =>
            {
                float[] intList = new float[dataArray.Length * dataArray[0].Length];
                float max = 0;
                float min = 0;
                int index = 0;
                for (int u = 0; u < dataArray.Length; u++)
                {
                    foreach (var item in dataArray[u])
                    {
                        float castItem = Convert.ToSingle(item);

                        if (castItem < min) min = castItem;
                        if (castItem > max) max = castItem;

                        intList[index] = castItem;
                        index++;
                    }
                }
                return new Array[] { intList, new float[] { min, max } };
            }
        );
    }

    public IEnumerator PreloadFITS(FITSImage[] images)
    {
        for (int i = 0; i < images.Length; i++)
        {
            if (images[i].Settings.SourceMode == FITSSourceMode.Web)
            {
                Debug.Log("Downloading FITS");
                Debug.Log(images[i].Path);
                UnityWebRequest www = UnityWebRequest.Get(images[i].Path);
                var dh = new DownloadHandlerFile(Application.dataPath + "/" + www.GetHashCode());
                dh.removeFileOnAbort = true;
                www.downloadHandler = dh;

                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError(www.error);
                }
                else
                {
                    Debug.Log("Download finished");
                    byte[] data = File.ReadAllBytes(Application.dataPath + "/" + www.GetHashCode());

                    Debug.Log(data);
                    Debug.Log("Above is the url fit downloaded");

                    if (images[i].Path.EndsWith(".gz"))
                    {
                        MemoryStream compressed = new MemoryStream(data);
                        using var decompressor = new GZipStream(compressed, CompressionMode.Decompress);

                        FileStream file = File.Create(fitsPath + "/" + gameObject.name + "_fits_" + i + ".fits");
                        decompressor.CopyTo(file);
                        file.Flush();
                        file.Close();

                        images[i].Path = "Assets/" + www.downloadHandler.GetHashCode() + ".fits";
                        Debug.LogWarning("File imported in assets");
                    }
                    else if (images[i].Path.EndsWith(".bz2"))
                    {
                        string bzPath = this.fitsPath + "/" + gameObject.name + "_fits_" + i + ".bz2";
                        string fitsPath = this.fitsPath + "/" + gameObject.name + "_fits_" + i + ".fits";

                        Debug.Log("Iam bz written");
                        FileStream file = File.Create(bzPath);
                        file.Flush();
                        file.Close();

                        File.WriteAllBytes(bzPath, data);  // write all byte that is data into the .bz file

                        FileStream fitsfile = File.Create(fitsPath);

                        BZip2.Decompress(File.OpenRead(bzPath), fitsfile, true);

                        images[i].Path = fitsPath;
                    }
                    else
                    {
                        images[i].Path = this.fitsPath + "/" + gameObject.name + "_fits_" + i + ".fits";
                    }
                }
            }
        }
        LoadFITS(true);
    }

    public void SaveToPath(Texture2D texture)
    {
        loadTexture = texture;
        byte[] getdata = texture.EncodeToPNG();
        var dirPath = Application.persistentDataPath + "/" + gameObject.name;
        if (!Directory.Exists(dirPath))
            Directory.CreateDirectory(dirPath);

        SaveToPng(getdata, dirPath);
    }

    public void SaveToPng(byte[] bytes, string path)
    {
        if (EnableTexSave)
        {
            string fullPath = path + "/" + gameObject.name + ".png";
            File.WriteAllBytes(fullPath, bytes);
            Debug.Log("Texture saved as png to, " + fullPath);
        }
    }

    public Texture2D LoadFromPath(int texH, int texW)
    {
        try
        {
            string filePath = Application.persistentDataPath + "/" + gameObject.name + "/" + gameObject.name + ".png";
            byte[] bytes = File.ReadAllBytes(filePath);
            Texture2D texture2D = new Texture2D(texW, texH);
            texture2D.wrapMode = WrapMode;
            texture2D.LoadImage(bytes);
            texture2D.name = gameObject.name.ToString();
            texture2D.Apply();
            loadTexture = texture2D;
            loadTexture.wrapMode = WrapMode;
            loadTexture.Apply();
            return texture2D;
        }
        catch
        {
            Debug.Log("Textures not found reloading fits");
            return null;
        }        
    }
}
