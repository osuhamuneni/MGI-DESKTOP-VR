using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GalaxyCSVLoader : MonoBehaviour
{
    public Mesh mesh;
    public Material material;
    private GalaxyInfoReader _galaxyInfoReader;
    [SerializeField] private Toggle _cvsToggle;

    private List<FITSObject> fitsObjectsCSVList = new List<FITSObject>();

    private bool _loadFromCSV = false;
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

    private void Start()
    {
        _cvsToggle.onValueChanged.AddListener(OnToggleValueChanged);
        if (LoadFromCSV)
        {
            CreateGalaxy();
        }
        _galaxyInfoReader = FindObjectOfType<GalaxyInfoReader>();
    }

    private void OnToggleValueChanged(bool isOn)
    {
        LoadFromCSV = isOn;
        if (fitsObjectsCSVList.Count == 0 && isOn)
            CreateGalaxy();
    }

    private void CreateGalaxy(string[] galaxyToCreate = null)
    {
        foreach (var galaxyInfos in _galaxyInfoReader.GetGalaxyInfosList)
        {
            string galaxyName = galaxyInfos.GalaxyName.Replace("Galaxy Name: ", "");
            if(galaxyToCreate != null)
            {
                //TODO only create the named galaxies
            }

            GameObject galaxyGo = new GameObject(galaxyName);
            galaxyGo.AddComponent<Transform>();
            galaxyGo.AddComponent<MeshFilter>().sharedMesh = mesh;
            galaxyGo.AddComponent<MeshRenderer>().material = material;
            galaxyGo.AddComponent<SphereCollider>();
            galaxyGo.AddComponent<FITSObject>();
            //galaxyGo.AddComponent<GalaxiesFaceCamera>();

            var fitsObject = galaxyGo.GetComponent<FITSObject>();
            fitsObject.scale = ConfigManager.Instance.configData.scale;
            fitsObject.ImageData = new FitsReader.FITSImage[3];

            for (int i = 0; i < 3; i++)
            {
                fitsObject.ImageData[i] = new FitsReader.FITSImage
                {
                    Path = GetColorBandPath(galaxyInfos, i),
                    isEnabled = true,
                    colorBand = GetColorBand(i),
                    Settings = new FitsReader.FITSReadSettings
                    {
                        SourceMode = FitsReader.FITSSourceMode.Web,
                        RGBMultiplier = GetRGBMultiplier(i),
                        LowBoost = 10,
                        BoostMode = FitsReader.BoostMode.Difference
                    }
                };
            }

            //GalaxyInfos gInfos = _galaxyInfoReader.GetGalaxyInfo(galaxyGo.name);
            //GalaxyInfoDisplay galaxyInfoDisplay = AddUIObject(galaxyGo);
            //galaxyInfoDisplay.InitGalaxyInfo(gInfos);
            //Galaxy galaxy = galaxyGo.GetComponent<Galaxy>();
            //galaxy.Init(this, galaxyInfoDisplay, gInfos);

            fitsObjectsCSVList.Add(fitsObject);
        }
    }

    private string GetColorBandPath(GalaxyInfos galaxyInfos, int index)
    {
        switch (index)
        {
            case 0:
                return galaxyInfos.RBand;
            case 1:
                return galaxyInfos.GBand;
            case 2:
                return galaxyInfos.IBand;
            default:
                return string.Empty;
        }
    }

    private static ColorBand GetColorBand(int index)
    {
        switch (index)
        {
            case 0: return ColorBand.Red;
            case 1: return ColorBand.Green;
            case 2: return ColorBand.Blue;
            default: return ColorBand.UV;
        }
    }

    private static Vector3 GetRGBMultiplier(int index)
    {
        switch (index)
        {
            case 0: return new Vector3(1, 0, 0);
            case 1: return new Vector3(0, 1, 0);
            case 2: return new Vector3(0, 0, 1);
            default: return Vector3.one;
        }
    }

    //private GalaxyInfoDisplay AddUIObject(GameObject go)
    //{
    //    GameObject prefab = Instantiate(uiPrefab, go.transform.position, Quaternion.identity, go.transform);
    //    return prefab.GetComponent<GalaxyInfoDisplay>();
    //}
}
