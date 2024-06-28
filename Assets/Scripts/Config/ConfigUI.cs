using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using FitsReader;

public class ConfigUI : MonoBehaviour
{
    public GameObject Edit_Panel;
    public GameObject Preview_Panel;

    public TMP_Dropdown dropdown;
    public Slider slider;
    public TMP_Text sliderText;
    public Toggle toggleCrop;
    public Text toggleTextCrop;
    public Toggle toggleAlpha;
    public Text toggleTextAlpha;
    public Toggle toggleDenoise;
    public Text toggleTextDenoise;

    public TMP_Text dropdownDisplayText;
    public TMP_Text sliderDisplayText;
    public TMP_Text toggleCropDisplayText;
    public TMP_Text toggleAlphaDisplayText;
    public TMP_Text toggleDenoiseDisplayText;

    public Toggle UseVR;

    private SceneController sc;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (ConfigManager.Instance == null)
            return;

        sc = FindObjectOfType<SceneController>();
        if (sc != null)
            UseVR.gameObject.SetActive(false);

        // Clear any existing options in the dropdown
        dropdown.ClearOptions();

        // Get the names of the enum values
        string[] colorizeModeNames = System.Enum.GetNames(typeof(ColorizeMode));

        // Convert the names to a list of Dropdown options
        var options = new List<TMP_Dropdown.OptionData>();
        foreach (string modeName in colorizeModeNames)
        {
            options.Add(new TMP_Dropdown.OptionData(modeName));
        }

        // Add the options to the dropdown
        dropdown.AddOptions(options);

        DisplayValue();

        dropdown.onValueChanged.AddListener(SetColorizeMode);
        slider.onValueChanged.AddListener(SetScale);
        toggleCrop.onValueChanged.AddListener(SetCropGalaxies);
        toggleAlpha.onValueChanged.AddListener(SetGalaxiesAlpha);
        toggleDenoise.onValueChanged.AddListener(SetDenoiseColorisation);
    }

    private void Update()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void DisplayValue()
    {
        dropdown.value = (int)ConfigManager.Instance.configData.colorizeMode;
        dropdownDisplayText.text = ConfigManager.Instance.configData.colorizeMode.ToString();

        slider.value  = ConfigManager.Instance.configData.scale;
        sliderDisplayText.text = sliderText.text = ConfigManager.Instance.configData.scale.ToString();

        toggleCrop.isOn = ConfigManager.Instance.configData.cropGalaxies;
        toggleCropDisplayText.text = ConfigManager.Instance.configData.cropGalaxies ? "Yes" : "No";

        toggleAlpha.isOn = ConfigManager.Instance.configData.galaxiesAlpha;
        toggleAlphaDisplayText.text = ConfigManager.Instance.configData.galaxiesAlpha ? "Yes" : "No";

        toggleDenoise.isOn = ConfigManager.Instance.configData.denoiseColorisation;
        toggleDenoiseDisplayText.text = ConfigManager.Instance.configData.denoiseColorisation ? "Yes" : "No";
    }

    public void EditValues()
    {
        Preview_Panel.SetActive(false);
        Edit_Panel.SetActive(true);
    }

    public void LoadScene()
    {
        ConfigManager.Instance.SaveConfig();

        sc = FindObjectOfType<SceneController>();
        if (sc != null)
        {
            string sceneToLoad = sc.SceneToReload;
            Destroy(sc.gameObject);
            SceneManager.LoadScene(sceneToLoad);
            return;
        }

        if(UseVR.isOn)
            SceneManager.LoadScene(1);
        else
            SceneManager.LoadScene(2);
    }

    // Set colorize mode
    public void SetColorizeMode(int mode)
    {
        ConfigManager.Instance.SetColorizeMode((ColorizeMode)mode);
    }

    // Set scale
    public void SetScale(float scale)
    {
        sliderText.text = scale.ToString();
        ConfigManager.Instance.SetScale((int)scale);
    }

    // Set crop galaxies
    public void SetCropGalaxies(bool crop)
    {
        toggleTextCrop.text = crop ? "Yes" : "No";

        ConfigManager.Instance.SetCropGalaxies(crop);
    }

    // Set galaxies alpha
    public void SetGalaxiesAlpha(bool alpha)
    {
        toggleTextAlpha.text = alpha ? "Yes" : "No";

        ConfigManager.Instance.SetGalaxiesAlpha(alpha);
    }

    // Set denoise/colorisation
    public void SetDenoiseColorisation(bool denoise)
    {
        toggleTextDenoise.text = denoise ? "Yes" : "No";

        ConfigManager.Instance.SetDenoiseColorisation(denoise);
    }
}
