using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Cinemachine;
using FitsReader;
using System.Linq;
using TMPro;
using nom.tam.fits;
using System.Collections;

public enum ECameraFocus
{
    Rotate,
    Fixed,
}

public class GalaxyManager : MonoBehaviour
{
    public static GalaxyManager manager;//It would be more save to use "instance"
    public GameObject galaxyPrefab;
    [Header("Galaxy Data")]
    public InputField XField;
    public InputField YField;
    public Dropdown ColorInput;
    [Header("Cameras")]  
    public GameObject galaxyCam;
    public CinemachineBrain mainCamera;
    public CinemachineVirtualCamera galaxyRotateCam;
    public CinemachineVirtualCamera galaxyFixedCam;
    CinemachineVirtualCamera currentActiveCamera;   
    public List<FITSObject> galaxies;

    [Header("Debug")]
    public Color gizColor = Color.green;

    const string CAM_CONTROL = "camControl";
    private GameObject _galaxyCamControl;
    private GameObject selectedGalaxy;
    float mouseX;
    float mouseY;

    [Header("Selection Mode")]
    public bool useRayCastForSelection = true;

    [Header("Camera Zooming")]
    public float zoomSpeed;
    private float m_maxFov;

    [Header("Ui configuration")]
    public UIMultplierGroup brightness;
    public UIMultplierGroup red;
    public UIMultplierGroup green;
    public UIMultplierGroup blue;
    public UIMultplierGroup uv;
    public UIMultplierGroup rad;
    public UIMultplierGroup lowBoost;

    private Slider brightnessIntensity;
    private Slider redIntensity;
    private Slider greenIntensity;
    private Slider blueIntensity;

    private Texture2D galaxyTexture;
    private Color[] galaxyColors;
    bool isViewingGalaxy = false;
    public Toggle modifyLevel;

    [Header("Color Multiplier")]
    public Toggle isMultiplierUni;
    public float valChange = 1;
    public TextMeshProUGUI redColorVal;
    public TextMeshProUGUI greenColorVal;
    public TextMeshProUGUI blueColorVal;
    public TextMeshProUGUI uvColorVal;
    public TextMeshProUGUI radColorVal;
    public UnityEvent onSelectEvent;
    public UnityEvent onDeselectEvent;

    [Header("Galaxy UI")]
    public GameObject galaxyDataInfoBtn;
    public GameObject galaxyBasePrefabUGUI;
    public GameObject galaxyNameUGUI;
    public GameObject galaxyDataPanel;
    public Transform galaxyDataParent;
    public TextMeshProUGUI selectionHelper;
    private string howtoselect = "Use \"Left Mouse Button\" to enter galaxy view";
    private string howtounlockmouse = "Use \"Right Mouse Button\" to unlock the mouse or press \"F\" to exit galaxy view";
    private string howtoexit = "Use \"Right Mouse Button\" or press \"F\" again to exit galaxy view or\nUse \"Left Mouse Button\" to enter galaxy view again";

    [Header("Galaxy Parent Fix")]
    public GameObject parentPrefab;

    [Header("New Features")]
    public bool loadGalaxyFromRes = false;
    [SerializeField] GameObject m_galaxyPrefab;
    [SerializeField] List<GalaxyDataBaseAsset> m_galaxyAssets;

    [Header("Controls/PC")]
    public Mouse mousepc;

    [SerializeField] private bool _loadFromCSV = true;
    public bool LoadFromCSV
    {
        get
        {
            return _loadFromCSV;
        }
        set
        {
            _loadFromCSV = value;
        }
    }

    private bool _isGalaxySelected = false;
    public bool IsGalaxySelected
    {
        get
        {
            return _isGalaxySelected;
        }
        set
        {
            _isGalaxySelected = value;
            mainCamera.enabled = value;
        }
    }

    private void Awake()
    {
        if (ConfigManager.Instance == null)
            return;

        manager = this;

        brightnessIntensity = brightness.slider;
        redIntensity = red.slider;
        greenIntensity = green.slider;
        blueIntensity = blue.slider;

        if (loadGalaxyFromRes)
        {
            m_galaxyAssets = new List<GalaxyDataBaseAsset>();
            Object[] gxs = Resources.LoadAll("Galaxy");
            foreach (Object g in gxs)
            {
                m_galaxyAssets.Add(g as GalaxyDataBaseAsset);
            }

            foreach (GalaxyDataBaseAsset gdba in m_galaxyAssets)
            {
                GameObject gxy = Instantiate(m_galaxyPrefab);
                FITSObject fITS3 = gxy.GetComponentInChildren<FITSObject>();
                fITS3.GetComponent<GalaxyDataBase>().GalaxyData = gdba;
                fITS3.gameObject.tag = "Galaxy";
                gxy.name = gdba.Data.data[1].data;
            }
        }

        selectionHelper.text = howtoselect;
    }

    /// <summary>
    /// Increases color multi val
    /// </summary>
    /// <param name="id">use first letter [r --> red, g --> green, b --> blue]</param>
    public void IncrementColorMultiVal(string id)
    {
        if (isMultiplierUni.isOn)
        {
            if (id.Trim().ToLower() == "r")
            {
                float val = float.Parse(redColorVal.text.Trim());
                if (val < 10)
                {
                    val += valChange;
                    redColorVal.text = val.ToString();
                    foreach (FITSObject fits in galaxies)
                    {
                        foreach (FITSImage iData in fits.ImageData)
                        {
                            if (iData.Settings.RGBMultiplier.x < 10)
                            {
                                Vector3 rgbm = iData.Settings.RGBMultiplier;
                                iData.Settings.RGBMultiplier = new Vector3(val, rgbm.y,rgbm.z);
                            }
                        }
                    }
                }
            }
            else if (id.Trim().ToLower() == "g")
            {
                float val = float.Parse(greenColorVal.text.Trim());
                if (val < 10)
                {
                    val += valChange;
                    greenColorVal.text = val.ToString();
                    foreach (FITSObject fits in galaxies)
                    {
                        foreach (FITSImage iData in fits.ImageData)
                        {
                            if (iData.Settings.RGBMultiplier.y < 10)
                            {
                                Vector3 rgbm = iData.Settings.RGBMultiplier;
                                iData.Settings.RGBMultiplier = new Vector3(rgbm.x, val, rgbm.z);
                            }
                        }
                    }
                }
            }
            else if (id.Trim().ToLower() == "b")
            {
                float val = float.Parse(blueColorVal.text.Trim());
                if (val < 10)
                {
                    val += valChange;
                    blueColorVal.text = val.ToString();
                    foreach (FITSObject fits in galaxies)
                    {
                        foreach (FITSImage iData in fits.ImageData)
                        {
                            if (iData.Settings.RGBMultiplier.z < 10)
                            {
                                Vector3 rgbm = iData.Settings.RGBMultiplier;
                                iData.Settings.RGBMultiplier = new Vector3(rgbm.x, rgbm.y, val);
                            }
                        }
                    }
                }
            }
            else if (id.Trim().ToLower() == "u")
            {
                float val = float.Parse(uvColorVal.text.Trim());
                if (val < 10)
                {
                    val += valChange;
                    blueColorVal.text = val.ToString();
                    foreach (FITSObject fits in galaxies)
                    {
                        foreach (FITSImage iData in fits.ImageData)
                        {
                            if (iData.colorBand == ColorBand.UV)
                            {
                                if (iData.Settings.Multiplier < 10)
                                {
                                    iData.Settings.Multiplier = val;
                                }
                            }
                        }
                    }
                }
            }
            else if (id.Trim().ToLower() == "h")
            {
                float val = float.Parse(radColorVal.text.Trim());
                if (val < 10)
                {
                    val += valChange;
                    blueColorVal.text = val.ToString();
                    foreach (FITSObject fits in galaxies)
                    {
                        foreach (FITSImage iData in fits.ImageData)
                        {
                            if (iData.colorBand == ColorBand.XRay)
                            {
                                if (iData.Settings.Multiplier < 10)
                                {
                                    iData.Settings.Multiplier = val;
                                }
                            }
                        }
                    }
                }
            }
        }
        else
        {
            if (isViewingGalaxy)
            {
                if (id.Trim().ToLower() == "r")
                {
                    float val = float.Parse(redColorVal.text.Trim());
                    FITSObject fits = selectedGalaxy.GetComponent<FITSObject>();
                    if (val < 10)
                    {
                        val += valChange;
                        redColorVal.text = val.ToString();

                        foreach (FITSImage iData in fits.ImageData)
                        {
                            if (iData.Settings.RGBMultiplier.x < 10)
                            {
                                Vector3 rgbm = iData.Settings.RGBMultiplier;
                                iData.Settings.RGBMultiplier = new Vector3(val, rgbm.y, rgbm.z);
                            }
                        }
                    }
                }
                else if (id.Trim().ToLower() == "g")
                {
                    float val = float.Parse(greenColorVal.text.Trim());
                    FITSObject fits = selectedGalaxy.GetComponent<FITSObject>();
                    if (val < 10)
                    {
                        val += valChange;
                        greenColorVal.text = val.ToString();

                        foreach (FITSImage iData in fits.ImageData)
                        {
                            if (iData.Settings.RGBMultiplier.x < 10)
                            {
                                Vector3 rgbm = iData.Settings.RGBMultiplier;
                                iData.Settings.RGBMultiplier = new Vector3(rgbm.x, val, rgbm.z);
                            }
                        }
                    }
                }
                else if (id.Trim().ToLower() == "b")
                {
                    float val = float.Parse(blueColorVal.text.Trim());
                    FITSObject fits = selectedGalaxy.GetComponent<FITSObject>();
                    if (val < 10)
                    {
                        val += valChange;
                        blueColorVal.text = val.ToString();

                        foreach (FITSImage iData in fits.ImageData)
                        {
                            if (iData.Settings.RGBMultiplier.x < 10)
                            {
                                Vector3 rgbm = iData.Settings.RGBMultiplier;
                                iData.Settings.RGBMultiplier = new Vector3(rgbm.x, rgbm.y, val);
                            }
                        }
                    }
                }
                else if (id.Trim().ToLower() == "u")
                {
                    float val = float.Parse(uvColorVal.text.Trim());
                    FITSObject fits = selectedGalaxy.GetComponent<FITSObject>();
                    if (val < 10)
                    {
                        val += valChange;
                        blueColorVal.text = val.ToString();

                        foreach (FITSImage iData in fits.ImageData)
                        {
                            if (iData.colorBand == ColorBand.UV)
                            {
                                if (iData.Settings.Multiplier < 10)
                                {
                                    iData.Settings.Multiplier = val;
                                }
                            }
                        }
                    }
                }
                else if (id.Trim().ToLower() == "h")
                {
                    float val = float.Parse(radColorVal.text.Trim());
                    FITSObject fits = selectedGalaxy.GetComponent<FITSObject>();
                    if (val < 10)
                    {
                        val += valChange;
                        blueColorVal.text = val.ToString();

                        foreach (FITSImage iData in fits.ImageData)
                        {
                            if (iData.colorBand == ColorBand.XRay)
                            {
                                if (iData.Settings.Multiplier < 10)
                                {
                                    iData.Settings.Multiplier = val;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Decrease color multi val
    /// </summary>
    /// <param name="id">use first letter [r --> red, g --> green, b --> blue]</param>
    public void DecrementColorMultiVal(string id)
    {
        if (isMultiplierUni.isOn)
        {
            if (id.Trim().ToLower() == "r")
            {
                float val = float.Parse(redColorVal.text.Trim());
                if (val > 0)
                {
                    val -= valChange;
                    redColorVal.text = val.ToString();
                    foreach (FITSObject fits in galaxies)
                    {
                        foreach (FITSImage iData in fits.ImageData)
                        {
                            if (iData.Settings.RGBMultiplier.x > 0)
                            {
                                Vector3 rgbm = iData.Settings.RGBMultiplier;
                                iData.Settings.RGBMultiplier = new Vector3(val, rgbm.y, rgbm.z);
                            }
                        }
                    }
                }
            }
            else if (id.Trim().ToLower() == "g")
            {
                float val = float.Parse(greenColorVal.text.Trim());
                if (val > 0)
                {
                    val -= valChange;
                    greenColorVal.text = val.ToString();
                    foreach (FITSObject fits in galaxies)
                    {
                        foreach (FITSImage iData in fits.ImageData)
                        {
                            if (iData.Settings.RGBMultiplier.y > 0)
                            {
                                Vector3 rgbm = iData.Settings.RGBMultiplier;
                                iData.Settings.RGBMultiplier = new Vector3(rgbm.x, val, rgbm.z);
                            }
                        }
                    }
                }
            }
            else if (id.Trim().ToLower() == "b")
            {
                float val = float.Parse(blueColorVal.text.Trim());
                if (val > 0)
                {
                    val -= valChange;
                    blueColorVal.text = val.ToString();
                    foreach (FITSObject fits in galaxies)
                    {
                        foreach (FITSImage iData in fits.ImageData)
                        {
                            if (iData.Settings.RGBMultiplier.z > 0)
                            {
                                Vector3 rgbm = iData.Settings.RGBMultiplier;
                                iData.Settings.RGBMultiplier = new Vector3(rgbm.x, rgbm.y, val);
                            }
                        }
                    }
                }
            }
        }
        else
        {
            if (isViewingGalaxy)
            {
                if (id.Trim().ToLower() == "r")
                {
                    float val = float.Parse(redColorVal.text.Trim());
                    FITSObject fits = selectedGalaxy.GetComponent<FITSObject>();
                    if (val < 10)
                    {
                        val -= valChange;
                        redColorVal.text = val.ToString();

                        foreach (FITSImage iData in fits.ImageData)
                        {
                            if (iData.Settings.RGBMultiplier.x < 10)
                            {
                                Vector3 rgbm = iData.Settings.RGBMultiplier;
                                iData.Settings.RGBMultiplier = new Vector3(val, rgbm.y, rgbm.z);
                            }
                        }
                    }
                }
                else if (id.Trim().ToLower() == "g")
                {
                    float val = float.Parse(greenColorVal.text.Trim());
                    FITSObject fits = selectedGalaxy.GetComponent<FITSObject>();
                    if (val < 10)
                    {
                        val -= valChange;
                        greenColorVal.text = val.ToString();

                        foreach (FITSImage iData in fits.ImageData)
                        {
                            if (iData.Settings.RGBMultiplier.x < 10)
                            {
                                Vector3 rgbm = iData.Settings.RGBMultiplier;
                                iData.Settings.RGBMultiplier = new Vector3(rgbm.x, val, rgbm.z);
                            }
                        }
                    }
                }
                else if (id.Trim().ToLower() == "b")
                {
                    float val = float.Parse(blueColorVal.text.Trim());
                    FITSObject fits = selectedGalaxy.GetComponent<FITSObject>();
                    if (val < 10)
                    {
                        val -= valChange;
                        blueColorVal.text = val.ToString();

                        foreach (FITSImage iData in fits.ImageData)
                        {
                            if (iData.Settings.RGBMultiplier.x < 10)
                            {
                                Vector3 rgbm = iData.Settings.RGBMultiplier;
                                iData.Settings.RGBMultiplier = new Vector3(rgbm.x, rgbm.y, val);
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Generated data ui list
    /// </summary>
    List<GameObject> generatedData;

    /// <summary>
    /// Generated UI and show the data panel
    /// </summary>
    public void ShowDataPanelUI()
    {
        
        if (!galaxyDataPanel.activeInHierarchy)
        {
            if (generatedData != null)
            {
                for (int i = 0; i <= generatedData.Count - 1; i++)
                {
                    Destroy(generatedData[i]);
                }
            }

            generatedData = new List<GameObject>();
            galaxyDataPanel.SetActive(true);
            GalaxyDataBase gdb = selectedGalaxy.GetComponent<GalaxyDataBase>();
            galaxyNameUGUI.GetComponentInChildren<TextMeshProUGUI>().text = gdb.GalaxyData.name;
            foreach (BaseData data in gdb.GalaxyData.Data.data)
            {
                if (data.header.Contains("_UI"))
                {
                    GameObject go = Instantiate(galaxyBasePrefabUGUI, galaxyDataParent.transform);
                    generatedData.Add(go);
                    GalaxyDataUI ui = go.GetComponent<GalaxyDataUI>();
                    ui.SetData(data.header.Replace("_UI", ""), data.data);
                }
            }
        }
        else
        {
            galaxyDataPanel.SetActive(false);
            for (int i = 0; i <= generatedData.Count - 1; i++)
            {
                Destroy(generatedData[i]);
            }
        }
    }

    /// <summary>
    /// Reloads all the galaxies
    /// </summary>
    public void ReloadGalaxies()
    {
        if (isMultiplierUni)
        {
            foreach (FITSObject f in galaxies)
            {
                if (f.enabled)
                {
                    var imageData = f.ImageData;
                    f.UpdateFITS(imageData);
                }
            }
        }
        else
        {
            if (isViewingGalaxy)
            {
                var obj = selectedGalaxy.GetComponent<FITSObject>();
                obj.UpdateFITS(obj.ImageData);
            }
        }
    }    

    void Start()
    {
        FITSObject[] findGs = FindObjectsOfType<FITSObject>();
        galaxies = findGs.ToList();
        foreach (FITSObject fits in galaxies)
        {
            fits.gameObject.AddComponent<MeshCollider>().convex = true;
           //fits.gameObject.AddComponent<LookAtPos>();
            var go = new GameObject(CAM_CONTROL);
            fits.CameraControl = go;
            go.transform.parent = fits.gameObject.transform;
            go.transform.localPosition = Vector3.zero;
            if (fits.gameObject.tag != "Galaxy")
            {
                fits.gameObject.tag = "Galaxy";
            }

            fits.gameObject.AddComponent<SelectGalaxy>();
            
            GameObject pr = Instantiate(parentPrefab, fits.gameObject.transform.position, fits.transform.rotation);
            fits.transform.SetParent(pr.transform);
            pr.name = fits.gameObject.name;
            pr.GetComponent<LookAtPos>().target = galaxyRotateCam.gameObject;
            fits.gameObject.transform.localEulerAngles = new Vector3(-90, 0, 0);
            fits.gameObject.transform.localPosition = Vector3.zero;
        }        

        m_maxFov = galaxyRotateCam.m_Lens.FieldOfView;
    }

    private Vector2Int offsetG;
    void Update()
    {
        if (Input.GetMouseButton(0) && useRayCastForSelection)
        {
            //GetGalaxy();
            GetGalaxyOnMouseClickRayCast();
        }

        if (isViewingGalaxy)
        {
            currentActiveCamera.m_Lens.FieldOfView -= Input.mouseScrollDelta.y * Time.smoothDeltaTime * zoomSpeed;
            currentActiveCamera.m_Lens.FieldOfView = Mathf.Clamp(currentActiveCamera.m_Lens.FieldOfView, 30, 100); 
        }
       
        UpdateFixedCamera(_galaxyCamControl);

        if (isViewingGalaxy)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                offsetG.y += 1;
                Debug.Log("UP");
                UpdateGalaxyOffset();
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                offsetG.y -= 1;
                UpdateGalaxyOffset();
            }

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                offsetG.x += 1;
                UpdateGalaxyOffset();
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                offsetG.x -= 1;
                UpdateGalaxyOffset();
            }

            XField.text = offsetG.x.ToString();
            YField.text = offsetG.y.ToString();
        }        

        if (modifyLevel.isOn)
        {
            for (int i = 0; i <= galaxies.Count - 1; i++)
            {
                try
                {
                    Texture2D tex = (Texture2D)galaxies[i].gameObject.GetComponent<Renderer>().material.mainTexture;
                    if (tex.GetPixels() != null)
                    {
                        Color[] col = tex.GetPixels();
                        for (int c = 0; c <= col.Length - 1; c++)
                        {
                            if (col[c].r > 0)
                            {
                                float ins = Mathf.Clamp(redIntensity.value / redIntensity.maxValue, 0.1f, 1);
                                col[c] = new Color(ins, col[c].g, col[c].b);
                            }

                            if (col[c].g > 0)
                            {
                                float ins = Mathf.Clamp(greenIntensity.value / greenIntensity.maxValue, 0.1f, 1);
                                col[c] = new Color(col[c].r, ins, col[c].b);
                            }

                            if (col[c].b > 0)
                            {
                                float ins = Mathf.Clamp(blueIntensity.value / blueIntensity.maxValue, 0.1f, 1);
                                col[c] = new Color(col[c].r, col[c].g, ins);
                            }
                        }
                        tex.SetPixels(col);
                        tex.Apply();
                    }
                }
                catch
                {
                    Debug.Log("Texture error!");
                }
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (isViewingGalaxy)
            {
                if (mousepc.isCursorFree)
                {
                    //exit mode
                    mousepc.LockCursor();
                    ControlCameras();
                    selectionHelper.text = howtoselect;
                    return;
                }
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (isViewingGalaxy)
            {
                mousepc.UnlockCursor();
                selectionHelper.text = howtoexit;
                return;
            }
        }        

        if(Input.GetKeyDown(KeyCode.F))
        {
            ControlCameras();
            StartCoroutine(WaitForSecondsRoutine(3f));
            Camera.main.transform.GetChild(0).gameObject.SetActive(false);
            selectionHelper.text = howtoselect;
        }
    }

    private IEnumerator WaitForSecondsRoutine(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        // Code here will execute after the specified seconds
        IsGalaxySelected = false;
    }

    public void ManipulateGalaxyColors()//manipulation on shader
    {
        if (!modifyLevel.isOn)
        {
            if (selectedGalaxy != null)
            {
                float brightness = Mathf.Clamp(brightnessIntensity.value, 1f, 10f);
                float rInt = Mathf.Clamp(redIntensity.value / redIntensity.maxValue, 0.01f, 1);
                float gInt = Mathf.Clamp(greenIntensity.value / greenIntensity.maxValue, 0.01f, 1);
                float bInt = Mathf.Clamp(blueIntensity.value / blueIntensity.maxValue, 0.01f, 1);

                Material material = selectedGalaxy.GetComponent<Renderer>().material;

                // Set the material properties
                material.SetFloat("_Brightness", brightness);
                material.SetFloat("_RIntensity", rInt);
                material.SetFloat("_GIntensity", gInt);
                material.SetFloat("_BIntensity", bInt);
            }
        }
    }

    /// <summary>
    /// Remove galaxies from current scene
    /// </summary>
    public void ClearGalaxies()
    {
        foreach(FITSObject fit in galaxies)
        {
            Destroy(fit.gameObject);
        }

        galaxies = null;
        galaxies = new List<FITSObject>();
    }

    /// <summary>
    /// Create a new galaxy
    /// </summary>
    /// <param name="dataPath"></param>
    public void CreateGalaxy(FitsRGBData dataPath)
    {
        Vector3 galaxyPosition = new Vector3(UnityEngine.Random.Range(0, 5), UnityEngine.Random.Range(0, 5), UnityEngine.Random.Range(0, 5));
        GameObject glxy = Instantiate(galaxyPrefab);
        glxy.transform.position = galaxyPosition;
        FITSObject fITS = glxy.GetComponent<FITSObject>();

        var go = new GameObject(CAM_CONTROL);
        fITS.CameraControl = go;
        go.transform.parent = fITS.gameObject.transform;
        go.transform.localPosition = Vector3.zero;
        if (fITS.gameObject.tag != "Galaxy")
        {
            fITS.gameObject.tag = "Galaxy";
        }

        fITS.gameObject.AddComponent<SelectGalaxy>();
        //red
        fITS.ImageData[0].Path = dataPath.r.path;
        fITS.ImageData[0].Settings.SourceMode = dataPath.r.isPathWeb ? FITSSourceMode.Web : FITSSourceMode.Disk;
        //green
        fITS.ImageData[1].Path = dataPath.g.path;
        fITS.ImageData[1].Settings.SourceMode = dataPath.g.isPathWeb ? FITSSourceMode.Web : FITSSourceMode.Disk;
        //blue
        fITS.ImageData[2].Path = dataPath.b.path;
        fITS.ImageData[2].Settings.SourceMode = dataPath.b.isPathWeb ? FITSSourceMode.Web : FITSSourceMode.Disk;

        //----------------------------------------//
        //add anything here to add to the galaxies//
        //----------------------------------------//

        fITS.StartCoroutine(fITS.PreloadFITS(fITS.ImageData)); // loads the data fits data to the galaxy.
        galaxies.Add(fITS);
    }

    /// <summary>
    /// gets the galaxy object
    /// </summary>
    void GetGalaxy()
    {
        RaycastHit hitInfo;
        if (Physics.Raycast(galaxyCam.transform.position, galaxyCam.transform.forward, out hitInfo, Mathf.Infinity))
        {
            if (hitInfo.collider.tag == "Galaxy")
            {
                Debug.Log("Selected : " + hitInfo.collider.name);
                Transform child = hitInfo.collider.gameObject.transform.Find(CAM_CONTROL);
                try
                {
                    selectedGalaxy = hitInfo.collider.gameObject;
                    galaxyTexture = (Texture2D)selectedGalaxy.GetComponent<Renderer>().material.mainTexture;
                    galaxyColors = galaxyTexture.GetPixels();
                }
                catch
                {
                    Debug.Log("No texture found!");
                }  
            }
        }
    }

    void GetGalaxyOnMouseClickRayCast()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray.origin, ray.direction, out hit,Mathf.Infinity))
        {
            if (hit.collider.tag == "Galaxy")
            {
                Debug.Log(hit.collider.name + " -- selected");
                isViewingGalaxy = true;
                try
                {
                    selectedGalaxy = hit.collider.gameObject;
                    galaxyTexture = (Texture2D)selectedGalaxy.GetComponent<Renderer>().material.mainTexture;
                    galaxyColors = galaxyTexture.GetPixels();
                    XField.text = "0";
                    YField.text = "0";
                    offsetG = Vector2Int.zero;
                    onSelectEvent.Invoke();
                    galaxyDataInfoBtn.SetActive(true);
                }
                catch
                {
                    Debug.Log("No texture found!");
                }
            }
        }

        Debug.DrawRay(ray.origin, ray.direction * 1000);
        Debug.Log("Casting ray!");
    }

    public static void GetGalaxyColors(GameObject selectedGalaxy)
    {
        manager.galaxyTexture = (Texture2D)selectedGalaxy.GetComponent<Renderer>().material.mainTexture;
        manager.galaxyColors = manager.galaxyTexture.GetPixels();
        manager.redIntensity.value = manager.redIntensity.maxValue;
        manager.greenIntensity.value = manager.greenIntensity.maxValue;
        manager.blueIntensity.value = manager.blueIntensity.maxValue;
    }

    public static void SetSelectedGalaxy(GameObject g)
    {
        manager.selectedGalaxy = g;
        FITSObject fitsObject = g.GetComponent<FITSObject>();
        manager.galaxyCam.SetActive(false);
        if (fitsObject != null)
        {
            if (fitsObject.GalaxyCameraFocus == ECameraFocus.Fixed)
            {
                Debug.Log("Camera will be fixed");
                manager.galaxyFixedCam.gameObject.SetActive(true);
                manager.galaxyRotateCam.gameObject.SetActive(false);
                manager.currentActiveCamera = manager.galaxyFixedCam;
                g.GetComponentInParent<LookAtPos>().isLookActive = true;
                g.GetComponentInParent<LookAtPos>().target = manager.currentActiveCamera.gameObject;
                manager.StartGalaxySelectCam(fitsObject.CameraControl.transform);
            }
            else if (fitsObject.GalaxyCameraFocus == ECameraFocus.Rotate)
            {
                Debug.Log("Camera will rotate");
                manager.galaxyFixedCam.gameObject.SetActive(false);
                manager.galaxyRotateCam.gameObject.SetActive(true);
                manager.currentActiveCamera = manager.galaxyRotateCam;
                g.GetComponentInParent<LookAtPos>().isLookActive = false;
                g.GetComponentInParent<LookAtPos>().target = manager.currentActiveCamera.gameObject;
                manager.StartGalaxySelectCam(fitsObject.CameraControl.transform);
            }
        }

        if (!manager.isMultiplierUni)
        {
            manager.redColorVal.text = "0";
            manager.greenColorVal.text = "0";
            manager.blueColorVal.text = "0";
        }

        manager.galaxyDataInfoBtn.SetActive(true);

        manager.mousepc.LockCursor();
        manager.selectionHelper.text = manager.howtounlockmouse;
    }

    /// <summary>
    /// Starts the Selected Galaxy camera
    /// </summary>
    /// <param name="rotateAround">galaxy child cam control to rotate around</param>
    /// <param name="isFixed">is camera supposed to be fixed</param>
    void StartGalaxySelectCam(Transform rotateAround)
    {
        _galaxyCamControl = rotateAround.gameObject;
        currentActiveCamera.Follow = rotateAround;
        currentActiveCamera.LookAt = rotateAround;
        isViewingGalaxy = true;

        currentActiveCamera.gameObject.SetActive(true);
    }

    /// <summary>
    /// Toggles fixed camera active state
    /// </summary>
    void ControlCameras()
    {
        if (currentActiveCamera != null)
        {
            if (currentActiveCamera.gameObject.activeInHierarchy)
            {
                selectedGalaxy.transform.parent.GetComponent<LookAtPos>().isLookActive = false;
                galaxyCam.SetActive(true);
                currentActiveCamera.gameObject.SetActive(false);
                galaxyFixedCam.gameObject.SetActive(false);
                galaxyRotateCam.gameObject.SetActive(false);
                _galaxyCamControl = null;
                isViewingGalaxy = false;
                selectedGalaxy = null;
                galaxyRotateCam.m_Lens.FieldOfView = m_maxFov;
                onDeselectEvent.Invoke();
                XField.text = "0";
                YField.text = "0";
                offsetG = Vector2Int.zero;
                galaxyDataInfoBtn.SetActive(false);
                galaxyDataPanel.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Move camera
    /// </summary>
    /// <param name="camControl"></param>
    void UpdateFixedCamera(GameObject camControl)
    {
        if (camControl != null)
        {
            if (!mousepc.isCursorFree)
            {
                if (!selectedGalaxy.GetComponentInParent<LookAtPos>().isLookActive)
                {
                    mouseX += Input.GetAxis("Mouse X");
                    mouseY += Input.GetAxis("Mouse Y");

                    camControl.transform.rotation = Quaternion.Euler(mouseY, mouseX, 0);
                }
            }  
        }
    }

    /// <summary>
    /// Update galaxy offset
    /// </summary>
    public void UpdateGalaxyOffset()
    {
        //Vector2Int offsets = new Vector2Int();

        if (XField.text == string.Empty || YField.text == string.Empty) {
            Debug.Log("One or more offset text fields are empty, please enter in a value and try again");
            return;
        }

        string inputValue = ColorInput.options[ColorInput.value].text;

        if (isViewingGalaxy)
        {
            FITSObject galaxyObject = selectedGalaxy.GetComponent<FITSObject>();
            FITSImage[] images = galaxyObject.ImageData;

            switch (inputValue)
            {
                case "Red":
                    images[0].Settings.Offset = offsetG;
                    break;

                case "Green":
                    images[1].Settings.Offset = offsetG;
                    break;

                case "Blue":
                    images[2].Settings.Offset = offsetG;
                    break;
            }

            galaxyObject.ImageData = images;
            galaxyObject.LoadFitsImages();
        }    
    }

    private void OnDrawGizmos()
    {
        if (galaxyCam != null)
        {
            Gizmos.color = gizColor;
            Gizmos.DrawRay(galaxyCam.transform.position, galaxyCam.transform.forward * 1000);
        }
    }

    public void UpdateGalaxiesColor()
    {
        if (selectedGalaxy != null)
        {
            FITSObject galaxyFits = selectedGalaxy.GetComponent<FITSObject>();
            FITSReadSettings settings = galaxyFits.ImageData[0].Settings;
            Vector3 RGBmultiplier = settings.RGBMultiplier;

            // don't apply to all RGBmultiplier just to the ones that have values so that the
            // R multiplier is changed ect.
            if (RGBmultiplier.x > 0) RGBmultiplier.x = red.slider.value; 
            else if (RGBmultiplier.y > 0) RGBmultiplier.y = green.slider.value; 
            else if (RGBmultiplier.z > 0) RGBmultiplier.z = blue.slider.value; 

            settings.Multiplier = brightness.slider.value;
            settings.RGBMultiplier = RGBmultiplier;
            settings.LowBoost = lowBoost.slider.value;

            galaxyFits.ImageData[0].Settings = settings; // set the settings on the image
            galaxyFits.UpdateFITS(galaxyFits.ImageData, 1, false);
        }
    }

    public void ToggleOnlyRed()
    {
        try
        {
            if (selectedGalaxy != null)
            {
                FITSObject fITS = selectedGalaxy.GetComponent<FITSObject>();
                fITS.EnableTexSave = false;
                fITS.ImageData = new FITSImage[1];
                fITS.ImageData[0] = fITS.ImageDataBckList[0];
                fITS.ImageData[0].colorBand = ColorBand.Red;
                Vector3 rgbM = fITS.ImageData[0].Settings.RGBMultiplier;
                fITS.ImageData[0].Settings.RGBMultiplier = new Vector3(rgbM.y, rgbM.y, rgbM.y);
                fITS.ImageData[0].isEnabled = true;
                fITS.UpdateFITS(fITS.ImageData, 1, false);
            }
        }
        catch
        {
            Debug.Log("Galaxy not found");
        }
    }

    public void ToggleOnlyGreen()
    {
        try
        {
            if (selectedGalaxy != null)
            {
                FITSObject fITS = selectedGalaxy.GetComponent<FITSObject>();
                fITS.EnableTexSave = false;
                fITS.ImageData = new FITSImage[1];
                fITS.ImageData[0] = fITS.ImageDataBckList[1];
                fITS.ImageData[0].colorBand = ColorBand.Green;
                Vector3 rgbM = fITS.ImageData[0].Settings.RGBMultiplier;
                fITS.ImageData[0].Settings.RGBMultiplier = new Vector3(rgbM.y, rgbM.y, rgbM.y);
                fITS.ImageData[0].isEnabled = true;
                fITS.UpdateFITS(fITS.ImageData, 1, false);
            }
        }
        catch
        {
            Debug.Log("Galaxy not found");
        }
    }

    public void ToggleOnlyBlue()
    {
        try
        {
            if (selectedGalaxy != null)
            {
                FITSObject fITS = selectedGalaxy.GetComponent<FITSObject>();
                fITS.EnableTexSave = false;
                fITS.ImageData = new FITSImage[1];
                fITS.ImageData[0] = fITS.ImageDataBckList[2];
                fITS.ImageData[0].colorBand = ColorBand.Blue;
                Vector3 rgbM = fITS.ImageData[0].Settings.RGBMultiplier;
                fITS.ImageData[0].Settings.RGBMultiplier = new Vector3(rgbM.z, rgbM.z, rgbM.z);
                fITS.ImageData[0].isEnabled = true;
                fITS.UpdateFITS(fITS.ImageData, 1, false);
            }
        }
        catch
        {
            Debug.Log("Galaxy not found");
        }
    }

    public void ToggleUltraviolet()
    {
        try
        {
            if (selectedGalaxy != null)
            {
                FITSObject fITS = selectedGalaxy.GetComponent<FITSObject>();
                fITS.EnableTexSave = false;
                fITS.ImageData = new FITSImage[1];
                fITS.ImageData[0] = fITS.ImageDataBckList[3];
                fITS.ImageData[0].colorBand = ColorBand.UV;
                Vector3 rgbM = fITS.ImageData[0].Settings.RGBMultiplier;
                fITS.ImageData[0].Settings.RGBMultiplier = new Vector3(1, 1, 1);
                fITS.ImageData[0].isEnabled = true;
                fITS.UpdateFITS(fITS.ImageData, 1, false);
            }
        }
        catch
        {
            Debug.Log("Galaxy or fitsimage data not found");
        }
    }

    public void ToggleRadiation()
    {
        try
        {
            if (selectedGalaxy != null)
            {
                FITSObject fITS = selectedGalaxy.GetComponent<FITSObject>();
                fITS.EnableTexSave = false;
                fITS.ImageData = new FITSImage[1];
                fITS.ImageData[0] = fITS.ImageDataBckList[4];
                fITS.ImageData[0].colorBand = ColorBand.XRay;
                Vector3 rgbM = fITS.ImageData[0].Settings.RGBMultiplier;
                fITS.ImageData[0].Settings.RGBMultiplier = new Vector3(1, 1, 1);
                fITS.ImageData[0].isEnabled = true;
                fITS.UpdateFITS(fITS.ImageData, 1, false);
            }
        }
        catch
        {
            Debug.Log("Galaxy not found");
        }
    }

    public void ToggleAllColor()
    {
        try
        {
            if (selectedGalaxy != null)
            {
                FITSObject fITS = selectedGalaxy.GetComponent<FITSObject>();
                fITS.EnableTexSave = false;
                fITS.ImageData = new FITSImage[fITS.ImageDataBckList.Count];

                for (int i = 0; i < fITS.ImageData.Length; i++)
                {
                    fITS.ImageData[i] = new FITSImage();
                    fITS.ImageData[i].Path = fITS.ImageDataBckList[i].Path;
                    fITS.ImageData[i].BackupPath = fITS.ImageDataBckList[i].BackupPath;
                    fITS.ImageData[i].colorBand = fITS.ImageDataBckList[i].colorBand;
                    fITS.ImageData[i].Settings = fITS.ImageDataBckList[i].Settings;
                    fITS.ImageData[i].preload = fITS.ImageDataBckList[i].preload;
                    fITS.ImageData[i].isEnabled = true;
                }

                try
                {
                    fITS.ImageData[0].Settings.RGBMultiplier = new Vector3(fITS.ImageData[0].Settings.RGBMultiplier.x, 0, 0);
                    fITS.ImageData[1].Settings.RGBMultiplier = new Vector3(0, fITS.ImageData[1].Settings.RGBMultiplier.y, 0);
                    fITS.ImageData[2].Settings.RGBMultiplier = new Vector3(0, 0, fITS.ImageData[2].Settings.RGBMultiplier.z);
                }
                catch
                {
                    Debug.Log("Error on settings");
                }

                fITS.UpdateFITS(fITS.ImageData, recut:false);
            }
        }
        catch
        {
            Debug.Log("Galaxy not found");
        }
    }
}

[System.Serializable]
public class FitsRGBData
{
    public DataPath r;
    public DataPath g;
    public DataPath b;

    public FitsRGBData(DataPath r, DataPath g, DataPath b)
    {
        this.r = r;
        this.g = g;
        this.b = b;
    }

    public FitsRGBData()
    {

    }
}

public struct DataPath
{
    public string path;
    public bool isPathWeb;
}

public enum ColorBand
{
    Red, Green, Blue, UV, XRay
}