using UnityEngine;
using UnityEngine.UI;

public class GalaxyInfoDisplay : MonoBehaviour
{
    [SerializeField] private Text galaxyName;
    [SerializeField] private Text galaxyUI;
    [SerializeField] private Text galaxyEquatorialCoordinates;
    [SerializeField] private Text galaxyRadialVelocity;
    [SerializeField] private Text galaxyLuminosity;
    [SerializeField] private Text galaxyAngularSize;
    [SerializeField] private Text galaxyCamera;
    [SerializeField] private GameObject galaxyInfoPanel;
    [SerializeField] private GameObject galaxyInfoButton;

    private GalaxyInfos _galaxyInfos;

    public void InitGalaxyInfo(GalaxyInfos galaxyInfos)
    {
        _galaxyInfos = galaxyInfos;
    }

    private void SetGalaxyInfos()
    {
        galaxyName.text = _galaxyInfos.GalaxyName;
        galaxyUI.text = _galaxyInfos.GalaxyUI;
        galaxyEquatorialCoordinates.text = _galaxyInfos.GalaxyEquatorialCoordinates;
        galaxyRadialVelocity.text = _galaxyInfos.GalaxyRadialVelocity;
        galaxyLuminosity.text = _galaxyInfos.GalaxyLuminosity;
        galaxyAngularSize.text = _galaxyInfos.GalaxyAngularSize;
        galaxyCamera.text = _galaxyInfos.GalaxyCamera;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.I))
        {
            DisplayInfos(true);
        }

        if (Input.GetKey(KeyCode.X))
        {
            DisplayInfos(false);
        }

        if(Input.GetKey(KeyCode.F))
        {
            DisplayUI(false);
            DisplayInfos(false);
        }
    }

    public void DisplayUI(bool show)
    {

        gameObject.SetActive(show);
    }

    public void DisplayInfos(bool show)
    {
        if (_galaxyInfos != null)
            SetGalaxyInfos();
        galaxyInfoPanel.SetActive(show);
        galaxyInfoButton.SetActive(!show);
    }
}
