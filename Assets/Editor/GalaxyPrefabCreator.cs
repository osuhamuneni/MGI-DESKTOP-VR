using UnityEngine;
using UnityEditor;
using System.IO;

public class GalaxyPrefabCreator : EditorWindow
{
    private const string SourceFolderPath = "Assets/StreamingAssets/virgo galaxy fits files";//use for Virgo Galaxies
    //private const string SourceFolderPath = "Assets/StreamingAssets/Spiral galaxies fits files";//use for Spiral Galaxies
    private const string ModelPath = "Assets/FITS/Scene/Sharp Elipse.fbx";
    private const string PrefabSavePath = "Assets/Resources/Prefabs/Galaxies";

    [MenuItem("Tools/Generate Galaxy Prefabs")]
    private static void CreateGalaxyPrefabs()
    {
        string[] folders = Directory.GetDirectories(SourceFolderPath);

        foreach (string folderPath in folders)
        {
            string folderName = Path.GetFileName(folderPath);

            string prefabPath = $"{PrefabSavePath}/{folderName}.prefab";
            if (PrefabExists(prefabPath))
            {
                Debug.Log($"Prefab already exists for folder '{folderName}'. Skipping prefab creation.");
                continue;
            }

            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(ModelPath);
            Material material = LoadMaterialFromModel(ModelPath);

            GameObject prefabGO = new GameObject(folderName);
            prefabGO.AddComponent<Transform>();
            prefabGO.AddComponent<MeshFilter>().sharedMesh = mesh;
            prefabGO.AddComponent<MeshRenderer>().material = material;
            prefabGO.AddComponent<MeshCollider>().sharedMesh = mesh;
            prefabGO.GetComponent<MeshCollider>().convex = true;
            prefabGO.AddComponent<FITSObject>();
            //prefabGO.AddComponent<GalaxiesFaceCamera>();

            var fitsObject = prefabGO.GetComponent<FITSObject>();

            fitsObject.objectToInstatiate = prefabGO;
            fitsObject.scale = 20;

            fitsObject.ImageData = new FitsReader.FITSImage[3];

            string[] fitsFiles = Directory.GetFiles(folderPath, "*.fits");

            for (int i = 0; i < 3; i++)
            {
                string fitsFile = GetFitsFilePath(fitsFiles, i);
                if (!string.IsNullOrEmpty(fitsFile))
                {
                    string assetPath = fitsFile.Replace("Assets/StreamingAssets/", "");
                    fitsObject.ImageData[i].Path = assetPath;
                    fitsObject.ImageData[i].isEnabled = true;
                    fitsObject.ImageData[i].colorBand = GetColorBand(i);
                    fitsObject.ImageData[i].Settings = new FitsReader.FITSReadSettings();
                    fitsObject.ImageData[i].Settings.RGBMultiplier = GetRGBMultiplier(i);
                    fitsObject.ImageData[i].Settings.LowBoost = 10;
                    fitsObject.ImageData[i].Settings.BoostMode = FitsReader.BoostMode.Difference;
                }
            }

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(prefabGO, prefabPath);
            DestroyImmediate(prefabGO);
        }

        AssetDatabase.Refresh();
    }

    public static Material LoadMaterialFromModel(string modelPath)
    {
        UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(modelPath);

        // Find the material among the loaded assets
        Material material = null;
        foreach (var asset in assets)
        {
            if (asset is Material)
            {
                material = asset as Material;
                break;
            }
        }

        return material;
    }

    private static string GetFitsFilePath(string[] files, int index)
    {
        string prefix = GetPrefix(index);
        foreach (string file in files)
        {
            if (file.Contains(prefix))
                return file;
        }
        return string.Empty;
    }

    private static string GetPrefix(int index)
    {
        switch (index)
        {
            case 0: return "frame-r-";
            case 1: return "frame-g-";
            case 2: return "frame-i-";
            default: return string.Empty;
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

    private static bool PrefabExists(string prefabPath)
    {
        return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null;
    }
}
