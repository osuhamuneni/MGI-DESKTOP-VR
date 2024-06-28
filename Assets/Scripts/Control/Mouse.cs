using Cinemachine;
using UnityEngine;

public class Mouse : MonoBehaviour
{
    private Vector2 rotation = Vector2.zero;
    public float rotationSpeed = 3f;

    public bool isCursorFree = false;


    [SerializeField] private SwitchMode switchMode;

    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (isCursorFree == false && switchMode.isVREnabled == false)       //Only allow camera movement if the mode is Non-VR and cursor is unlocked
        {
            rotation.y += Input.GetAxis("Mouse X");
            rotation.x += -Input.GetAxis("Mouse Y");
            transform.localEulerAngles = (Vector2)rotation * rotationSpeed;
           
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            isCursorFree = !isCursorFree;
            UpdateCursorMode();
        }
    }

    public void UpdateCursorMode()
    {
        if (isCursorFree)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            //Debug.Log("Unlocked cursor !");
        }
        else
        {
            //Debug.Log("Locked cursor !");
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    /// <summary>
    /// Set the cursor mode
    /// </summary>
    /// <param name="isLocked"></param>
    public void SetCursorMode(bool isLocked)
    {
        Cursor.lockState = isLocked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !isLocked;
        isCursorFree = !isLocked;
        if (isLocked) Debug.Log("Cursor Locked");
        else Debug.Log("Cursor Unlocked");
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isCursorFree = false;
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isCursorFree = true;
    }
}