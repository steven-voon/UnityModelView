using UnityEngine;

public class ObjectScaler : MonoBehaviour
{
    [Header("Settings")]
    public Transform targetObject; // Drag your 3D object here
    public float minScale = 0.5f;   // Smallest size
    public float maxScale = 3.0f;   // Largest size

    // This method will be called by the Slider's OnValueChanged event
    public void ScaleObject(float sliderValue)
    {
        if (targetObject == null) return;

        // Linearly interpolate between min and max scale based on slider (0 to 1)
        float currentScale = Mathf.Lerp(minScale, maxScale, sliderValue);

        // Apply the scale uniformly to X, Y, and Z
        targetObject.localScale = new Vector3(currentScale, currentScale, currentScale);
    }
}
