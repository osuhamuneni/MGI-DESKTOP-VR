using Cinemachine;
using UnityEngine;

public class ZoomPC : MonoBehaviour
{
    [Range(1, 30)]
    public float maxZoomValue;                  //The maximum value to zoom-in
    public bool zoomInButton;
    public bool zoomOutButton;

    public CinemachineVirtualCamera virtualCamera;

    public GameObject offset;                  //The object parenting the Camera
    private GalaxyManager _galaxyManager;


    private void Start()
    {
        _galaxyManager = FindObjectOfType<GalaxyManager>();
    }

    private void Update()
    {
        float value = Input.GetAxis("Mouse ScrollWheel");

        float scrollValue =+ value;

        if (value >= 0.1f)          // up
        {
            zoomInButton = true;
            zoomOutButton = false;
        }
        else if (value <= -0.1f)    // down
        {
            zoomOutButton = true;
            zoomInButton = false;
        }
        else if(value == 0f)        // no action
        {
            zoomInButton = false;
            zoomOutButton = false;
        }

        bool isBelowThanMax = Mathf.Abs(offset.transform.localPosition.x) < maxZoomValue
                && Mathf.Abs(offset.transform.localPosition.y) < maxZoomValue
                && Mathf.Abs(offset.transform.localPosition.z) < maxZoomValue;

        bool isHigherThanMin = (offset.transform.localPosition.x != 0 && offset.transform.localPosition.y != 0 && offset.transform.localPosition.z != 0);

        //If the user is scrolling up, and none of the localPosition axes are equal to the maximum zoom value.
        if (zoomInButton && isBelowThanMax && virtualCamera)
        {
            virtualCamera.m_Lens.FieldOfView = virtualCamera.m_Lens.FieldOfView - (scrollValue * 4.0f);
            offset.transform.localPosition = offset.transform.localPosition + (Camera.main.transform.forward / 3);
           // Debug.Log("Attempting to zoom-in");
        }
        //Else if the user is scrolling down, and none of the localPosition axes are equal to 0 (the default, original position).
        else if (zoomOutButton && isHigherThanMin && virtualCamera)
        {
            virtualCamera.m_Lens.FieldOfView = virtualCamera.m_Lens.FieldOfView - (scrollValue * 4.0f);
            offset.transform.localPosition = Vector3.MoveTowards(offset.transform.localPosition, Vector3.zero, 0.3f);   //Zoom out
           // Debug.Log("Attempting to zoom-out");
        }

        if (scrollValue != 0 && !_galaxyManager.IsGalaxySelected)
        {
            float correctedFOV = Mathf.Clamp(Camera.main.fieldOfView - (scrollValue * 8f), 10f, 100f);
            Camera.main.fieldOfView = correctedFOV;
            Debug.Log($"Camera fov : {Camera.main.fieldOfView} and camera FOV value : {correctedFOV}");
        }
    }
}