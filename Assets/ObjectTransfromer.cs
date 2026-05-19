using UnityEngine;
using UnityEngine.EventSystems;

public class ObjectTransfromer : MonoBehaviour
{

    public bool fixedX = false;
    public bool fixedY = false;
    public bool fixedZ = false;

    [Header("Scaling Settings")]
    public float minScale = 0.5f;
    public float maxScale = 3.0f;
    [SerializeField] private float pinchSensitivity = 0.01f;

    [Header("Auto Rotation")]
    [SerializeField] private Vector3 autoRotationSpeed = new Vector3(0, 50, 0);
    [SerializeField] private float idleWaitTime = 5f;
    [SerializeField] private float returnSpeed = 2f;

    [Header("Interaction Settings")]
    [SerializeField] private float mouseSensitivity = 0.5f;
    [SerializeField] private float touchSensitivity = 15f;

    private float idleTimer;
    private bool isInteracting = false;
    private Quaternion originalRotation;
    private Vector3 lastMousePosition;
    private float currentScaleFactor = 0.5f;

    void Start()
    {
        originalRotation = transform.rotation;
        currentScaleFactor = Mathf.InverseLerp(minScale, maxScale, transform.localScale.x);
    }

    void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            HandleInput();

            if (!isInteracting)
            {
                idleTimer += Time.deltaTime;

                if (idleTimer >= idleWaitTime)
                {
                    // 1. First, we apply the rotation to our "saved" original rotation variable
                    originalRotation *= Quaternion.Euler(autoRotationSpeed * Time.deltaTime);

                    // 2. Then, we smoothly move the current rotation toward that moving target
                    // This prevents the "snapping" or "stuck" behavior
                    transform.rotation = Quaternion.Slerp(transform.rotation, originalRotation, Time.deltaTime * returnSpeed);
                }
            }
        }
    }

    void HandleInput()
    {
        // 1. MOBILE TOUCH SUPPORT
        if (Input.touchCount > 0)
        {
            idleTimer = 0;

            if (Input.touchCount == 2)
            {
                isInteracting = true;
                HandlePinch();
            }
            else if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began) isInteracting = true;

                if (touch.phase == TouchPhase.Moved)
                {
                    float factor = touchSensitivity / (Screen.dpi > 0 ? Screen.dpi : 100);
                    RotateObject(touch.deltaPosition.x * factor * 10f, touch.deltaPosition.y * factor * 10f);
                }

                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) isInteracting = false;
            }
        }
        // 2. PC MOUSE SUPPORT (Only runs if no touches are detected)
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                isInteracting = true;
                lastMousePosition = Input.mousePosition;
            }
            else if (Input.GetMouseButton(0))
            {
                idleTimer = 0;
                Vector3 delta = Input.mousePosition - lastMousePosition;
                RotateObject(delta.x * mouseSensitivity, delta.y * mouseSensitivity);
                lastMousePosition = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isInteracting = false;
            }

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                currentScaleFactor = Mathf.Clamp01(currentScaleFactor + scroll);
                ScaleObject(currentScaleFactor);
            }
        }
    }

    void RotateObject(float xDelta, float yDelta)
    {
        // Y-axis swipe/mouse moves the object around X-axis
        // X-axis swipe/mouse moves the object around Y-axis
        transform.Rotate(Vector3.up, -xDelta, Space.World);
        transform.Rotate(Vector3.right, yDelta, Space.World);
    }

    public void ScaleObject(float sliderValue)
    {
        //// sliderValue is expected to be 0.0 to 1.0 from the UI Slider
        //float targetScale = Mathf.Lerp(minScale, maxScale, sliderValue);

        //// Apply the scale to the object this script is attached to
        //transform.localScale = new Vector3(targetScale, targetScale, targetScale);

        currentScaleFactor = sliderValue;
        float targetScale = Mathf.Lerp(minScale, maxScale, sliderValue);
        transform.localScale = new Vector3(targetScale, targetScale, targetScale);
    }


    void HandlePinch()
    {
        Touch touchZero = Input.GetTouch(0);
        Touch touchOne = Input.GetTouch(1);

        // Find the position in the previous frame of each touch.
        Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
        Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

        // Find the magnitude of the vector (the distance) between the touches in each frame.
        float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
        float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

        // Find the difference in the distances between each frame.
        float deltaMagnitudeDiff = touchDeltaMag - prevTouchDeltaMag;

        // Update the scale factor (0 to 1 range)
        currentScaleFactor = Mathf.Clamp01(currentScaleFactor + (deltaMagnitudeDiff * pinchSensitivity));
        ScaleObject(currentScaleFactor);
    }
}
