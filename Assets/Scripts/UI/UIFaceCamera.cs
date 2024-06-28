using UnityEngine;
using System.Collections;

public class UIFaceCamera : MonoBehaviour
{
    public float delay = 2.5f; // Delay before starting the animation
    public float duration = 1f; // Duration of the animation
    public float targetScale = 0.007f; // Target scale to animate to
    public float targetAlpha = 1f; // Target alpha to animate to

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private void OnEnable()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        rectTransform.localScale = Vector3.zero;
        canvasGroup.alpha = 0;

        // Start the animation coroutine
        StartCoroutine(ScaleAnimationCoroutine());
    }

    private IEnumerator ScaleAnimationCoroutine()
    {
        // Wait for the delay before starting the animation
        yield return new WaitForSeconds(delay);

        // Get the initial scale and alpha of the RectTransform and CanvasGroup
        Vector3 initialScale = rectTransform.localScale;
        float initialAlpha = canvasGroup.alpha;

        // Calculate the rate of change per second
        float rate = 1f / duration;

        float t = 0f; // Time variable for the animation

        // Animate the scale and alpha from the initial values to the target values
        while (t < 1f)
        {
            t += Time.deltaTime * rate;

            // Interpolate the scale using Mathf.Lerp
            Vector3 newScale = Vector3.Lerp(initialScale, new Vector3(targetScale, targetScale, targetScale), t);

            // Interpolate the alpha using Mathf.Lerp
            float newAlpha = Mathf.Lerp(initialAlpha, targetAlpha, t);

            // Apply the new scale and alpha to the RectTransform and CanvasGroup
            rectTransform.localScale = newScale;
            canvasGroup.alpha = newAlpha;

            yield return null; // Wait for the next frame
        }
    }
}
