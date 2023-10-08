#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraOrbit : MonoBehaviour
{
    public Transform target;

    [Header("Orbit Settings")]
    public bool canOrbit = true;
    public float orbitSpeed = 1;
    public float minPolarAngle = 0;
    public float maxPolarAngle = 89.9f;
    public float minAzimuthalAngle = -180;
    public float maxAzimuthalAngle = 180;
    public bool clampAzimuthalAngle = false;

    [Header("Zoom Settings")]
    public bool canZoom = true;
    public float minDistance = 1;
    public float maxDistance = 20;
    public CustomSlider zoomSlider;

    private bool eventSystemIsPresent;

    private bool _canOrbit;
    private bool _canZoom;
    private bool waitForCameraController;

    private float polarAngle;
    private float azimuthalAngle;
    private float currentDistance;
    private bool isOrbiting;

    private float speedMultiplier = 1;

    public static event System.Action OnStartOrbit;
    public static event System.Action OnEndOrbit;

    protected virtual void OnEnable()
    {
        if (TryGetComponent<CameraController>(out var controller))
        {
            // Debug.Log(transform.name + " will respond to CameraController");
            waitForCameraController = true;
            CameraController.OnCameraMovementComplete += HandleCameraMovementComplete;
        }

        // Check whether there is an event system present
        eventSystemIsPresent = FindObjectOfType<EventSystem>() != null;

#if UNITY_EDITOR
        if (EditorApplication.isPlaying) speedMultiplier = 10;
#endif
    }

    protected virtual void OnDisable()
    {
        if (TryGetComponent<CameraController>(out var controller))
        {
            CameraController.OnCameraMovementComplete -= HandleCameraMovementComplete;
        }
    }

    public void Initialize()
    {
        if (!target) return;

        transform.LookAt(target);

        Vector3 angles = transform.localEulerAngles;
        azimuthalAngle = angles.y;
        polarAngle = angles.x;

        // Calculate the initial camera position relative to the target object
        Vector3 targetPosition = target ? target.localPosition : Vector3.zero;
        currentDistance = Vector3.Distance(transform.localPosition, targetPosition);
        if (canZoom)
        {
            Vector3 direction = Quaternion.Euler(polarAngle, azimuthalAngle, 0) * Vector3.back;
            currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
            transform.localPosition = currentDistance * direction;
        }
    }

    private void Start()
    {
        if (waitForCameraController)
        {
            // Temporarily disable camera movements from this script
            _canOrbit = canOrbit;
            canOrbit = false;

            _canZoom = canZoom;
            canZoom = false;

            SetZoomSliderInteractability(false);
        }
        else
        {
            Initialize();
        }

        if (zoomSlider)
        {
            // Fix the slider bounds
            zoomSlider.minValue = minDistance;
            zoomSlider.maxValue = maxDistance;

            // Set the initial slider value based on where the camera will end up
            if (TryGetComponent<CameraController>(out var controller))
            {
                Vector3 targetPosition = target ? target.localPosition : Vector3.zero;
                float distance = Vector3.Distance(controller.targetPosition, targetPosition);
                zoomSlider.value = maxDistance - distance + minDistance;
            }
            else
            {
                zoomSlider.value = maxDistance - currentDistance + minDistance;
            }
        }
    }

    private void Update()
    {
        // Check whether the pointer is currently over a UI element
        bool pointerIsOverGameObject = false;
        if (eventSystemIsPresent)
        {
            pointerIsOverGameObject = EventSystem.current.IsPointerOverGameObject();
        }

        if (canOrbit) HandleOrbit(pointerIsOverGameObject);
    }

    private void HandleOrbit(bool pointerIsOverGameObject)
    {
        // Do not use camera orbit if the user clicks on a UI element
        if (Input.GetMouseButtonDown(0) && !pointerIsOverGameObject)
        {
            isOrbiting = true;
            Cursor.visible = false;

            // Broadcast that orbiting has begun
            OnStartOrbit?.Invoke();
        }

        if (isOrbiting)
        {
            // Update polar angle
            polarAngle -= Input.GetAxis("Mouse Y") * orbitSpeed * speedMultiplier;
            polarAngle = Mathf.Clamp(polarAngle, minPolarAngle, maxPolarAngle);

            // Update azimuthal angle
            azimuthalAngle += Input.GetAxis("Mouse X") * orbitSpeed * speedMultiplier;
            azimuthalAngle %= 360f;
            if (clampAzimuthalAngle)
            {
                azimuthalAngle = Mathf.Clamp(azimuthalAngle, minAzimuthalAngle, maxAzimuthalAngle);
            }

            Quaternion rotation = Quaternion.Euler(polarAngle, azimuthalAngle, 0);
            Vector3 direction = rotation * Vector3.back;
            Vector3 targetPosition = target ? target.localPosition : Vector3.zero;
            Vector3 position = targetPosition + direction * currentDistance;

            transform.localPosition = position;
            transform.localRotation = rotation;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isOrbiting = false;
            Cursor.visible = true;

            // Broadcast that orbiting has begun
            OnEndOrbit?.Invoke();
        }
    }

    public void SetZoomValue(float value)
    {
        if (!canZoom) return;

        currentDistance = maxDistance - value + minDistance;

        // Adjust camera position
        Vector3 direction = (transform.position - target.position).normalized;
        transform.position = target.position + direction * currentDistance;
    }

    public void HandleCameraMovementComplete(Vector3 cameraPosition, Quaternion cameraRotation)
    {
        Initialize();
        SetZoomSliderInteractability(true);
        canOrbit = _canOrbit;
        canZoom = _canZoom;
    }

    private void SetZoomSliderInteractability(bool value)
    {
        if (zoomSlider)
        {
            zoomSlider.interactable = value;
            if (zoomSlider.TryGetComponent(out CursorHoverUI cursorHover))
            {
                cursorHover.enabled = value;
            }
        }
    }
}
