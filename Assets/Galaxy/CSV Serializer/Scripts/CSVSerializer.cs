using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using System.Text;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(CSVSerializer))]
public class CSVSerializerEditor : Editor
{
    CSVSerializer serializer;

    private void OnEnable()
    {
        serializer = (CSVSerializer)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Serialize CSV"))
        {
            serializer.SerializeIt();
            EditorUtility.DisplayDialog("CSV Serialization", "CSV Serialized Sucessfully!", "Ok");
        }

        if (GUILayout.Button("Create Assets"))
        {
            if (!Directory.Exists(serializer.SoAssetPath))
            {
                Directory.CreateDirectory(serializer.SoAssetPath);
            }


            

            foreach (CSVData data in serializer.mainData)
            {
                GalaxyDataBaseAsset template = ScriptableObject.CreateInstance<GalaxyDataBaseAsset>();
                template.Data = data;
                string fp = serializer.SoAssetPath + data.dataName + ".asset";
                AssetDatabase.CreateAsset(template, fp);
            }

            EditorUtility.DisplayDialog("CSV Serialization", "Galaxy Assets Created on: "+serializer.SoAssetPath, "Ok");
            AssetDatabase.Refresh();
        }

        if (GUILayout.Button("Delete Assets"))
        {
            string[] files = Directory.GetFiles(serializer.SoAssetPath);
            foreach (string s in files)
            {
                File.Delete(s);
            }

            EditorUtility.DisplayDialog("CSV Serialization", "Galaxy Assets Deleted from: " + serializer.SoAssetPath, "Ok");
            AssetDatabase.Refresh();
        }

        if (GUILayout.Button("Asset Refresh"))
        {
            AssetDatabase.Refresh();
        }
    }

    #region Blocked
    //public void CreateScript(string fileName)
    //{
    //    string tt = serializer.TemplateClass.text;
    //    string[] st = tt.Split("\n");
    //    string[] hd = serializer.headers;
    //    for (int i = 0; i <= st.Length - 1; i++)
    //    {
    //        if (st[i].Contains("//Place"))
    //        {
    //            for (int j = 0; j <= hd.Length - 1; j++)
    //            {
    //                st[i] += "\n    public string " + hd[j] + ";";
    //            }

    //            break;
    //        }
    //    }

    //    string boi = string.Join("\n", st);


    //    byte[] arr = Encoding.UTF8.GetBytes(boi);
    //    FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Write, FileShare.None);
    //    fs.Write(arr, 0, arr.Length);
    //    fs.Close();




    //    if (!Directory.Exists(serializer.SoAssetPath))
    //    {
    //        Directory.CreateDirectory(serializer.SoAssetPath);
    //    }

    //    //serializer.GeneratedObject = Resources.Load<ScriptableObject>(fileName);
    //    AssetDatabase.Refresh();
    //}
    #endregion


}

#endif


public class CSVSerializer : MonoBehaviour
{
    /// <summary>
    /// CSV file required to generate all the data on this asset
    /// </summary>
    [SerializeField] TextAsset csvFile;
    [SerializeField] string soAssetPath = "Assets/Resources/Galaxy/";
    List<string> splitList;
    private string[] headers;
    public List<CSVData> mainData;
    public string SoAssetPath { get => soAssetPath; set => soAssetPath = value; }
    public string[] Headers { get => headers; set => headers = value; }

    private void Awake()
    {
        SerializeIt();
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Serializes the csv file
    /// </summary>
    internal void SerializeIt()
    {
        //init main data
        mainData = new List<CSVData>();
        //get csv file to string
        string textFileString = csvFile.text;
        //split the file by line ending
        string[] localsplit = textFileString.Split("\n");
        //add to split list
        splitList = localsplit.ToList();
        //remove the last element if empty 
        if (string.IsNullOrEmpty(splitList[splitList.Count - 1]))
        {
            splitList.RemoveAt(splitList.Count - 1);
        }
       
        //split the headers
        Headers = localsplit[0].Split(",");

        //generate maindata list
        for (int i = 1; i <= splitList.Count - 1; i++)
        {
            List<BaseData> bd = new List<BaseData>();
            string[] splitListSplit = splitList[i].Split(",");
            for (int jj = 0; jj <= Headers.Length - 1; jj++)
            {
                BaseData temp;
                temp.header = Headers[jj];
                temp.data = splitListSplit[jj];
                bd.Add(temp);
            }


            mainData.Add(new CSVData(bd[0].data, bd));
        }
    }

}

[System.Serializable]
public class CSVData
{
    public string dataName = "defualt";
    public List<BaseData> data;

    public CSVData(string dataName, List<BaseData> data)
    {
        this.dataName = dataName;
        this.data = data;
    }

    public string GetData(string header)
    {
        foreach(BaseData s in data)
        {
            if(s.header.Trim() == header.Trim())
            {
                return s.data.Trim();
            }
        }

        return null;
    }
}

[System.Serializable]
public struct BaseData
{
    public string header;
    public string data;
}


