using UnityEngine;

public class KeepCanvasSizeOnZoom : MonoBehaviour
{
    public Camera mainCamera;
    public Canvas canvas;
    public float baseFOV = 60f;
    public float baseScale = 0.1f;

    private float initialCanvasScale;

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (canvas == null)
        {
            canvas = GetComponent<Canvas>();
        }

        // Store the initial scale of the canvas
        initialCanvasScale = canvas.transform.localScale.x;
    }

    private void LateUpdate()
    {
        // Calculate the scale factor based on the current FOV and the base FOV
        float scaleFactor = mainCamera.fieldOfView / baseFOV;

        // Calculate the desired scale of the canvas based on the base scale and the scale factor
        float desiredScale = baseScale * scaleFactor;

        // Set the scale of the canvas to the desired scale
        canvas.transform.localScale = Vector3.one * desiredScale * initialCanvasScale;
    }
}
