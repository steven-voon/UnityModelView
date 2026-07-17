using NaughtyAttributes;
using UnityEngine;
using UnityEngine.EventSystems;

public class ObjectTransfromer : MonoBehaviour
{
    public bool fixedX = false;
    public bool fixedY = false;
    public bool fixedZ = false;

    [Header("Layer Settings")]
    [SerializeField] private LayerMask interactionLayer;
    [SerializeField] private float maxRaycastDistance = 100f; // OPTIMIZATION: Caps ray length

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
    [SerializeField] private bool isRotateOnStart = false;

    private float idleTimer;
    private bool isInteracting = false;
    private bool isPanning = false;
    private bool isResetting = false;
    private bool isAutoRotating = false;

    private Quaternion originalRotation;
    private Vector3 lastMousePosition;
    private float currentScaleFactor = 0.5f;
    private float originalScaleFactor;
    private float previousScaleFactor;

    private Vector3 initialRotationPivot;
    private float lastTapTime;

    [Header("New Interaction Settings")]
    [SerializeField] private float mousePanSensitivity = 0.05f;
    [SerializeField] private float touchPanSensitivity = 0.05f;
    [SerializeField] private float doubleTapThreshold = 0.3f;

    private Vector3 originalPosition;
    private Camera mainCamera;
    private Vector3 panVelocity;
    private float cachedDpiFactor; // OPTIMIZATION: Caches native OS query result

    private void Start()
    {
        mainCamera = Camera.main;
        originalRotation = transform.rotation;
        originalPosition = transform.position;

        currentScaleFactor = Mathf.InverseLerp(minScale, maxScale, transform.localScale.x);
        originalScaleFactor = currentScaleFactor;
        previousScaleFactor = currentScaleFactor;

        cachedDpiFactor = (Screen.dpi > 0 ? Screen.dpi : 100f);
    }

    private void OnEnable()
    {
        if (isRotateOnStart)
        {
            isAutoRotating = true;
            idleTimer = idleWaitTime + 1;
        }
    }

    private void Update()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        HandleInput();
        HandlePositionSnapping();

#if UNITY_EDITOR
        if (!Input.GetMouseButton(0)) isInteracting = false;
        if (!Input.GetMouseButton(1)) isPanning = false;
#else
        if (Input.touchCount == 0)
        {
            isInteracting = false;
            isPanning = false;
        }
#endif

        if (isInteracting || isPanning)
        {
            isResetting = false;
            isAutoRotating = false;
        }

        if (isResetting)
        {
            isAutoRotating = false;
            SmoothReturnToBaseline(); // Smoothly return without applying auto-spin

            // Turn off the reset state once we are close enough to baseline values
            if (Vector3.Distance(transform.position, originalPosition) < 0.01f &&
                Quaternion.Angle(transform.rotation, originalRotation) < 0.5f &&
                Mathf.Abs(currentScaleFactor - originalScaleFactor) < 0.01f)
            {
                isResetting = false;
            }
        }
        else if (!isInteracting && !isPanning)
        {
            idleTimer += Time.deltaTime;

            if (idleTimer >= idleWaitTime)
            {
                if (isAutoRotating)
                {
                    transform.Rotate(Vector3.right * autoRotationSpeed.y * Time.deltaTime, Space.Self);
                }
                else
                {
                    bool positionHome = Vector3.Distance(transform.position, originalPosition) < 0.05f;
                    bool scaleHome = Mathf.Abs(currentScaleFactor - originalScaleFactor) < 0.01f;
                    bool rotationHome = Quaternion.Angle(transform.rotation, originalRotation) < 1.0f;

                    if (positionHome && scaleHome && rotationHome)
                    {
                        isAutoRotating = true;
                        transform.Rotate(Vector3.right * autoRotationSpeed.y * Time.deltaTime, Space.Self);
                    }
                    else
                    {
                        SmoothReturnToBaseline();
                    }
                }
                
            }
        }
        previousScaleFactor = currentScaleFactor;
    }

    private void SmoothReturnToBaseline()
    {
        // 1. Smoothly interpolate rotation back to pristine original layout
        transform.rotation = Quaternion.Lerp(transform.rotation, originalRotation, Time.deltaTime * returnSpeed);

        // 2. Smoothly return to center position
        transform.position = Vector3.Lerp(transform.position, originalPosition, Time.deltaTime * returnSpeed);

        // 3. Smoothly scale back to default size centered on itself
        currentScaleFactor = Mathf.MoveTowards(currentScaleFactor, originalScaleFactor, Time.deltaTime * returnSpeed * 0.5f);
        ScaleObject(currentScaleFactor, transform.position);
    }

    void HandleInput()
    {
#if !UNITY_EDITOR
        if (Input.touchCount > 0)
        {
            idleTimer = 0;

            if (Input.touchCount == 2)
            {
                HandleTwoFingerTouch(); 
            }
            else if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);
                
                if (touch.phase == TouchPhase.Began) 
                {
                    if (Time.time - lastTapTime < doubleTapThreshold)
                    {
                        if (EvaluateTargetHit(touch.position))
                        {
                            ResetToInitialState();
                            return; 
                        }
                    }
                    lastTapTime = Time.time;

                    isInteracting = true;
                    initialRotationPivot = GetWorldPivotFromScreen(touch.position);
                }

                if (touch.phase == TouchPhase.Moved && isInteracting)
                {
                    float factor = touchSensitivity / cachedDpiFactor; // OPTIMIZATION: Uses cached float
                    RotateObjectAroundPivot(touch.deltaPosition.x * factor * 10f, touch.deltaPosition.y * factor * 10f, initialRotationPivot);
                }

                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) isInteracting = false;
            }
        }
#endif

#if UNITY_EDITOR
        // --- ROTATION (Left Click) ---
        if (Input.GetMouseButtonDown(0))
        {
            if (Time.time - lastTapTime < doubleTapThreshold)
            {
                if (EvaluateTargetHit(Input.mousePosition))
                {
                    ResetToInitialState();
                    return;
                }
            }
            lastTapTime = Time.time;

            isInteracting = true;
            lastMousePosition = Input.mousePosition;
            initialRotationPivot = GetWorldPivotFromScreen(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0) && isInteracting)
        {
            idleTimer = 0;
            Vector3 delta = Input.mousePosition - lastMousePosition;
            RotateObjectAroundPivot(delta.x * mouseSensitivity, delta.y * mouseSensitivity, initialRotationPivot);
            lastMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isInteracting = false;
        }

        // --- PANNING (Right Click) ---
        if (Input.GetMouseButtonDown(1))
        {
            isPanning = true;
            lastMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButton(1) && isPanning)
        {
            idleTimer = 0;
            Vector3 delta = Input.mousePosition - lastMousePosition;
            PanObject(delta.x * mousePanSensitivity, delta.y * mousePanSensitivity);
            lastMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            isPanning = false;
        }

        // --- ZOOM (Scroll Wheel) ---
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            isResetting = false;
            isAutoRotating = false;
            idleTimer = 0;
            currentScaleFactor = Mathf.Clamp01(currentScaleFactor + scroll);

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            float distanceToObject;

            // OPTIMIZATION: Ray length capped at maxRaycastDistance
            if (Physics.Raycast(ray, out RaycastHit hit, maxRaycastDistance, interactionLayer))
            {
                distanceToObject = Vector3.Distance(mainCamera.transform.position, hit.point);
            }
            else
            {
                distanceToObject = Vector3.Dot(transform.position - mainCamera.transform.position, mainCamera.transform.forward);
            }

            Vector3 mouseWorldPoint = ray.GetPoint(distanceToObject);
            ScaleObject(currentScaleFactor, mouseWorldPoint);
        }
#endif
    }

    [Button]
    public void ResetToInitialState()
    {
        idleTimer = 0f;
        isInteracting = false;
        isPanning = false;
        isResetting = true;
    }

    private bool EvaluateTargetHit(Vector2 screenPosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        return Physics.Raycast(ray, out _, maxRaycastDistance, interactionLayer);
    }

    private Vector3 GetWorldPivotFromScreen(Vector2 screenPosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        if (Physics.Raycast(ray, out RaycastHit hit, maxRaycastDistance, interactionLayer))
        {
            return hit.point;
        }

        float distanceToObject = Vector3.Dot(transform.position - mainCamera.transform.position, mainCamera.transform.forward);
        return ray.GetPoint(distanceToObject);
    }

    private void RotateObjectAroundPivot(float xDelta, float yDelta, Vector3 pivotPoint)
    {
        if (mainCamera == null) return;
        //Vector3 originalPosBeforeDrag = transform.position;
        if (!fixedY) transform.RotateAround(pivotPoint, Vector3.up, -xDelta);
        if (!fixedX) transform.RotateAround(pivotPoint, mainCamera.transform.right, yDelta);
        //transform.position = originalPosBeforeDrag;
    }

    private void PanObject(float xDelta, float yDelta)
    {
        if (mainCamera == null) return;

        Vector3 panDirection = mainCamera.transform.right * xDelta + mainCamera.transform.up * yDelta;
        transform.position += panDirection;
    }

    public void ScaleObject(float sliderValue, Vector3 zoomPivotWorld)
    {
        Vector3 oldScale = transform.localScale;

        currentScaleFactor = Mathf.Clamp01(sliderValue);
        float targetScale = Mathf.Lerp(minScale, maxScale, currentScaleFactor);
        Vector3 newScale = new Vector3(targetScale, targetScale, targetScale);

        float scaleRatio = newScale.x / oldScale.x;
        Vector3 distToPivot = transform.position - zoomPivotWorld;

        transform.localScale = newScale;
        transform.position = zoomPivotWorld + (distToPivot * scaleRatio);
    }

    private void HandlePositionSnapping()
    {
        float snapThreshold = 0.4f;

        if (currentScaleFactor < snapThreshold && !isInteracting && !isPanning && currentScaleFactor < previousScaleFactor)
        {
            float centerWeight = Mathf.InverseLerp(snapThreshold, 0f, currentScaleFactor);
            centerWeight = Mathf.SmoothStep(0f, 1f, centerWeight);

            float dynamicDampTime = Mathf.Lerp(0.3f, 0.05f, centerWeight);

            transform.position = Vector3.SmoothDamp(
                transform.position,
                originalPosition,
                ref panVelocity,
                dynamicDampTime
            );
        }
    }

    private void HandleTwoFingerTouch()
    {
        Touch touchZero = Input.GetTouch(0);
        Touch touchOne = Input.GetTouch(1);

        Vector2 currentCenter = (touchZero.position + touchOne.position) * 0.5f;

        if (touchZero.phase == TouchPhase.Began || touchOne.phase == TouchPhase.Began)
        {
            isPanning = true;
        }

        if (isPanning)
        {
            Vector2 prevCenter = ((touchZero.position - touchZero.deltaPosition) + (touchOne.position - touchOne.deltaPosition)) * 0.5f;
            Vector2 centerDelta = currentCenter - prevCenter;
            PanObject(centerDelta.x / cachedDpiFactor * touchPanSensitivity * 10f, centerDelta.y / cachedDpiFactor * touchPanSensitivity * 10f);
        }

        Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
        Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

        float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
        float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;
        float deltaMagnitudeDiff = touchDeltaMag - prevTouchDeltaMag;

        currentScaleFactor = Mathf.Clamp01(currentScaleFactor + (deltaMagnitudeDiff * pinchSensitivity));

        Vector2 pinchCenterScreen = (touchZero.position + touchOne.position) * 0.5f;
        Ray touchRay = mainCamera.ScreenPointToRay(pinchCenterScreen);
        float distToObj = Vector3.Dot(transform.position - mainCamera.transform.position, mainCamera.transform.forward);
        Vector3 pinchCenterWorld = touchRay.GetPoint(distToObj);

        ScaleObject(currentScaleFactor, pinchCenterWorld);

        if (touchZero.phase == TouchPhase.Ended || touchOne.phase == TouchPhase.Ended)
        {
            isPanning = false;
        }

    }
}
