using UnityEngine;

public class GalaxyDataBase : MonoBehaviour
{
    [SerializeField] GalaxyDataBaseAsset galaxyData;

    public GalaxyDataBaseAsset GalaxyData { get => galaxyData; set => galaxyData = value; }
}
