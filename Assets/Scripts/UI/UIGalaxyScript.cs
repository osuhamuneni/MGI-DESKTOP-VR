using FitsReader;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public struct Int4
{
    public int xStart;
    public int xEnd;
    public int yStart;
    public int yEnd;
}

public class UIGalaxyScript : MonoBehaviour
{
    List<GameObject> galaxies = new List<GameObject>();

    public GameObject galaxySettings;
    bool settingsToggle = true;
    [Header("UI")]
    public List<GameObject> onScreenUi;

    [Header("Buttons")]
    public Button reloadButton;
    public Button hideButton;

    [Header("Intensity group")]
    public UIMultplierGroup brightness;
    public UIMultplierGroup red;
    public UIMultplierGroup green;
    public UIMultplierGroup blue;
    public UIMultplierGroup uv;
    public UIMultplierGroup rad;
    public UIMultplierGroup lowBoost;

    public InputField JsonOrCSVPath;

    void Start()
    {
        //find all gameobjects with galaxies tag then from there change the FITSObject3 settings
        galaxies.AddRange(GameObject.FindGameObjectsWithTag("Galaxy"));
    }

    public void galaxyHideToggle(CanvasGroup canvas)
    {
        settingsToggle = !settingsToggle;

        canvas.alpha = settingsToggle ? 1.0f : 0.0f;
    }
}
