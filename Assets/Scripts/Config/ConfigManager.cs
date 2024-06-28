using FitsReader;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConfigManager : MonoBehaviour
{
    private static ConfigManager instance;

    private string configFileName = "config.cfg";
    private string configFilePath;

    public ConfigData configData = new ConfigData();

    public static ConfigManager Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.LogError("ConfigManager instance is null.");
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this; // Set the static instance
        }
        else
        {
            Destroy(gameObject); // If an instance already exists, destroy this one
            return;
        }

        configFilePath = Path.Combine(Application.streamingAssetsPath, configFileName);
        LoadConfig();
    }

    public void SaveConfig()
    {
        string json = JsonUtility.ToJson(configData);
        File.WriteAllText(configFilePath, json);
    }

    public void LoadConfig()
    {
        if (File.Exists(configFilePath))
        {
            string json = File.ReadAllText(configFilePath);
            configData = JsonUtility.FromJson<ConfigData>(json);
            //show config UI
            FindObjectOfType<ConfigUI>().Preview_Panel.SetActive(true);
        }
        else
        {
            CreateDefaultConfig();
            //Show Edit UI
            FindObjectOfType<ConfigUI>().Edit_Panel.SetActive(true);
        }
    }

    private void CreateDefaultConfig()
    {
        configData.colorizeMode = ColorizeMode.None;
        configData.scale = 20; // Default scale value
        configData.cropGalaxies = true;
        configData.galaxiesAlpha = false;
        configData.denoiseColorisation = false;

        SaveConfig(); // Save the default configuration
    }

    public void LoadScene()
    {
        SceneManager.LoadScene(1);
    }

    // Set colorize mode
    public void SetColorizeMode(ColorizeMode mode)
    {
        configData.colorizeMode = mode;
        SaveConfig();
    }

    // Set scale
    public void SetScale(int scale)
    {
        configData.scale = (int)Mathf.Clamp(scale, 1f, 100f);
        SaveConfig();
    }

    // Set crop galaxies
    public void SetCropGalaxies(bool crop)
    {
        configData.cropGalaxies = crop;
        SaveConfig();
    }

    // Set galaxies alpha
    public void SetGalaxiesAlpha(bool alpha)
    {
        configData.galaxiesAlpha = alpha;
        SaveConfig();
    }

    // Set denoise/colorisation
    public void SetDenoiseColorisation(bool denoise)
    {
        configData.denoiseColorisation = denoise;
        SaveConfig();
    }
}
