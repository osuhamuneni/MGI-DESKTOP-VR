using UnityEngine;
using UnityEngine.UI;

public class UIMultplierGroup : MonoBehaviour
{
    public Text value;
    public Slider slider;

    private GalaxyManager _galaxyManager;

    private void Start()
    {
        _galaxyManager = FindObjectOfType<GalaxyManager>();
        slider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    private void OnSliderValueChanged(float value)
    {
        //Only update the value when moving the slider
        this.value.text = value.ToString("0.000");

        _galaxyManager.ManipulateGalaxyColors();
    }
}
