using UnityEngine;

public class SlowRotate : MonoBehaviour
{
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

    void Start()
    {
        originalRotation = transform.rotation;
    }

    void Update()
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

    void HandleInput()
    {
        // 1. MOBILE TOUCH SUPPORT
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            idleTimer = 0;

            if (touch.phase == TouchPhase.Began) isInteracting = true;

            if (touch.phase == TouchPhase.Moved)
            {
                float factor = touchSensitivity / (Screen.dpi > 0 ? Screen.dpi : 100);
                RotateObject(touch.deltaPosition.x * factor * 10f, touch.deltaPosition.y * factor * 10f);
            }

            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) isInteracting = false;
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
        }
    }

    void RotateObject(float xDelta, float yDelta)
    {
        // Y-axis swipe/mouse moves the object around X-axis
        // X-axis swipe/mouse moves the object around Y-axis
        transform.Rotate(Vector3.up, -xDelta, Space.World);
        transform.Rotate(Vector3.right, yDelta, Space.World);
    }
}
