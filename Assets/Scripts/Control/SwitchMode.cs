using UnityEngine;
using UnityEngine.SpatialTracking;

public class SwitchMode : MonoBehaviour 
{
	[SerializeField] private GameObject VRController;
	[SerializeField] private GameObject VRCameraOffset;

	[SerializeField] private GameObject MainCamera;

	[SerializeField] private GameObject PCController;
	[SerializeField] private GameObject PCCameraOffset;

	[SerializeField] private Mouse mouseScript;

	public bool isVREnabled;

	private void Start()
	{
		VRCameraOffset.transform.eulerAngles = Vector3.zero;
		UpdateControllerMode();
	}

	private void Update()
	{
		if(!Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.V))
		{
			mouseScript.isCursorFree = false;		//Lock the mouse to control it.
			mouseScript.UpdateCursorMode();

			isVREnabled = !isVREnabled;
			UpdateControllerMode();
		}
	}
	
	private void UpdateControllerMode()
	{
		if(isVREnabled)
		{
			EnableVR();
			VRController.SetActive(true);
			PCController.SetActive(false);
		}
		else
		{
			EnablePC();
			VRController.SetActive(false);
			PCController.SetActive(true);
		}
	}

	private void EnableVR()
	{
		Debug.Log("Setting VR position and rotation");
		VRController.transform.position = PCController.transform.position;					//Set position

		VRCameraOffset.transform.eulerAngles = MainCamera.transform.localEulerAngles;		//Offset position and rotation depending on where the user is looking
		VRCameraOffset.transform.localPosition = Vector3.zero;								//Reset zoom

		MainCamera.transform.parent = VRCameraOffset.transform;                             //Change parent
		MainCamera.GetComponent<TrackedPoseDriver>().enabled = true;						//Enable headset tracking
	}

	private void EnablePC()
	{
		Debug.Log("Setting PC position and rotation");
		PCCameraOffset.transform.localPosition = Vector3.zero;                              //Reset zoom
		MainCamera.transform.parent = PCCameraOffset.transform;                             //Change parent
		MainCamera.GetComponent<TrackedPoseDriver>().enabled = false;                       //Disable headset tracking
	}
}