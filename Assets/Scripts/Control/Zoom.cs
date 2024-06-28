using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class Zoom : MonoBehaviour
{
    public enum ZoomHand
    {
        LeftHand,
        RightHand
    }
    public ZoomHand zoomHand;                   //The hand that will be used for zooming.
    private XRNode zoomHandNode;                //The XRInput node corresponding to the zoomHand.
    
    [Range(1, 30)]
    public float maxZoomValue;                  //The maximum value to zoom-in.
    private bool zoomInButton;
    private bool zoomOutButton;

    public GameObject offset;                  //The object parenting the Camera and controllers.

    private void Start()
    {
        offset = GameObject.FindGameObjectWithTag("CameraOffset");

        if(zoomHand == ZoomHand.LeftHand)       //If the hand selected for teleportation is the left hand.
        {
            zoomHandNode = XRNode.LeftHand;
        }
        else
        {
            zoomHandNode = XRNode.RightHand;
        }
    }

    private void Update()
    {
        InputDevices.GetDeviceAtXRNode(zoomHandNode).TryGetFeatureValue(CommonUsages.primaryButton, out zoomOutButton);
        InputDevices.GetDeviceAtXRNode(zoomHandNode).TryGetFeatureValue(CommonUsages.secondaryButton, out zoomInButton);

    }

    private void FixedUpdate()
    {
        bool isBelowThanMax = Mathf.Abs(offset.transform.localPosition.x) < maxZoomValue 
            && Mathf.Abs(offset.transform.localPosition.y) < maxZoomValue 
            && Mathf.Abs(offset.transform.localPosition.z) < maxZoomValue;

        bool isHigherThanMin = (offset.transform.localPosition.x != 0 && offset.transform.localPosition.y != 0 && offset.transform.localPosition.z != 0);
        //If the user is pressing the joystick up at more than 70 %, and none of the localPosition axes are equal to the maximum zoom value.
        if (zoomInButton && isBelowThanMax)
        {
            ZoomIn();
            Debug.Log("Attempting to zoom-in");
        }

        //Else if the user is pressing the joystick up at less than -70 %, and none of the localPosition axes are equal to 0 (the default, original position).
        else if (zoomOutButton && isHigherThanMin)
        {
            offset.transform.localPosition = Vector3.MoveTowards(offset.transform.localPosition, Vector3.zero, 0.1f);   //Zoom out
            Debug.Log("Attempting to zoom-out");
        }
    }

    private void ZoomIn()
    {
        //Move the Camera Offset in the forward direction of the camera. Increase the divider to slow down the zoom.
        offset.transform.localPosition = offset.transform.localPosition + (Camera.main.transform.forward/3);
    }

    //Called when teleporting.
    public void ResetZoom()
    {
        offset.transform.localPosition = Vector3.zero;
    }
}