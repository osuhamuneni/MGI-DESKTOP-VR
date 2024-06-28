using UnityEngine;

public class PCMovement : MonoBehaviour
{
	public float movementSpeed = 2f;
	public Transform myCamera;

	private bool isMouseVisible = false;

    private void Start()
    {
		Cursor.lockState = CursorLockMode.Locked;
	}

    private void Update()
	{
		if (Input.GetKey(KeyCode.W))
		{
			transform.position += myCamera.forward * movementSpeed * Time.deltaTime;    //forward
		}

		if (Input.GetKey(KeyCode.A))
		{
			transform.position += -myCamera.right * movementSpeed * Time.deltaTime;     //left
		}

		if (Input.GetKey(KeyCode.S))
		{
			transform.position += -myCamera.forward * movementSpeed * Time.deltaTime;   //backwards
		}

		if (Input.GetKey(KeyCode.D))
		{
			transform.position += myCamera.right * movementSpeed * Time.deltaTime;      //right
		}

		if (Input.GetMouseButtonUp(1) || Input.GetKeyUp(KeyCode.Escape))
		{
			isMouseVisible = !isMouseVisible;
			FreeCursorInMainView(isMouseVisible);
		}

		if (!isMouseVisible)
			Cursor.visible = false;
	}

	private void FreeCursorInMainView(bool enable)
    {
		Cursor.lockState = enable ? CursorLockMode.None : CursorLockMode.Locked;
		Cursor.visible = enable ? true : false;
	}
}