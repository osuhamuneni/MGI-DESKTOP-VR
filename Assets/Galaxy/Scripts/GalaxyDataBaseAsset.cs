using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Galaxy Data", menuName = "Create Galaxy Asset")]
public class GalaxyDataBaseAsset : ScriptableObject
{
    //Place
    [SerializeField] CSVData data;

    public CSVData Data { get => data; set => data = value; }
}
