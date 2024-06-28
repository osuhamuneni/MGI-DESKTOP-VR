using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class Teleport : MonoBehaviour
{
    public enum TeleportHand
    {
        LeftHand,
        RightHand
    }
    public TeleportHand teleportHand;               //The hand that will be used for teleportation.
    private XRNode teleportHandNode;                //The XRInput node corresponding to the hand selected.

    private Vector2 joystickValue;                  //Vector2 representing the position of the joystick. X (horizontal axis) and Y (vertical axis) goes from -1 to 1.

    private XRRayInteractor leftControllerRay;      //The left hand's XRRayInteractor component, used to render the teleportation ray.
    private XRRayInteractor rightControllerRay;     //The right hand's XRRayInteractor component, used to render the teleportation ray.
    private XRRayInteractor selectedRay;            //The XRRayInteractor reference selected, depending on the hand chosen for teleportation movement.

    private void Start()
    {
        leftControllerRay = GameObject.FindGameObjectWithTag("LeftHand").GetComponent<XRRayInteractor>();
        rightControllerRay = GameObject.FindGameObjectWithTag("RightHand").GetComponent<XRRayInteractor>();
        
        if (teleportHand == TeleportHand.LeftHand)  //If the hand selected for teleportation is the left hand.
        {
            teleportHandNode = XRNode.LeftHand;
            selectedRay = leftControllerRay;
            rightControllerRay.enabled = false;     //Disable the other controller's ray, as it is not used.
        }
        else
        {
            teleportHandNode = XRNode.RightHand;
            selectedRay = rightControllerRay;       
            leftControllerRay.enabled = false;     //Disable the other controller's ray, as it is not used.
        }
    }

    private void Update()
    {
        InputDevices.GetDeviceAtXRNode(teleportHandNode).TryGetFeatureValue(CommonUsages.primary2DAxis, out joystickValue);     //Store the current value of the joystick as a Vector2.
    }

    private void FixedUpdate()
    {
        if (joystickValue.y >= 0.7f)        //If the user is pressing the joystick up at more than 70 %.
        {
            selectedRay.enabled = true;     //Enable the teleportation ray.
            Debug.Log("Teleport ray enabled");
        }
        else
        {
            selectedRay.enabled = false;
            Debug.Log("Teleport ray disabled");
        }
    }
}