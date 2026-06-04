using UnityEngine;

/// <summary>
/// Mouse ve touch ile avatar etrafýnda kamera döndürme + zoom kontrolü.
/// Bu script Main Camera üzerine eklenir.
/// </summary>
public class OrbitCameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Orbit Settings")]
    [SerializeField] private float distance = 4f;
    [SerializeField] private float minDistance = 1.6f;
    [SerializeField] private float maxDistance = 6f;
    [SerializeField] private float rotationSpeed = 0.25f;
    [SerializeField] private float touchRotationSpeed = 0.18f;

    [Header("Zoom Settings")]
    [SerializeField] private float mouseZoomSpeed = 2f;
    [SerializeField] private float pinchZoomSpeed = 0.01f;

    [Header("Angle Limits")]
    [SerializeField] private float minVerticalAngle = -25f;
    [SerializeField] private float maxVerticalAngle = 65f;

    [Header("Smoothing")]
    [SerializeField] private bool smoothMovement = true;
    [SerializeField] private float smoothSpeed = 12f;

    private float yaw;
    private float pitch = 10f;

    private Vector3 desiredPosition;
    private Quaternion desiredRotation;

    private float previousPinchDistance;

    private void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("OrbitCameraController: Target is not assigned.");
            return;
        }

        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;

        UpdateCameraPosition(true);
    }

    private void Update()
    {
        if (target == null)
            return;

        HandleMouseInput();
        HandleTouchInput();
        UpdateCameraPosition(false);
    }

    private void HandleMouseInput()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButton(0))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            yaw += mouseX * rotationSpeed * 100f;
            pitch -= mouseY * rotationSpeed * 100f;
            pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.001f)
        {
            distance -= scroll * mouseZoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }
#endif
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Moved)
            {
                Vector2 delta = touch.deltaPosition;

                yaw += delta.x * touchRotationSpeed;
                pitch -= delta.y * touchRotationSpeed;
                pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);
            }
        }
        else if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            float currentPinchDistance = Vector2.Distance(t0.position, t1.position);

            if (t0.phase == TouchPhase.Began || t1.phase == TouchPhase.Began)
            {
                previousPinchDistance = currentPinchDistance;
                return;
            }

            float pinchDelta = currentPinchDistance - previousPinchDistance;

            distance -= pinchDelta * pinchZoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);

            previousPinchDistance = currentPinchDistance;
        }
    }

    private void UpdateCameraPosition(bool instant)
    {
        desiredRotation = Quaternion.Euler(pitch, yaw, 0f);
        desiredPosition = target.position - desiredRotation * Vector3.forward * distance;

        if (instant || !smoothMovement)
        {
            transform.position = desiredPosition;
            transform.rotation = desiredRotation;
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * smoothSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * smoothSpeed);
        }
    }

    public void SetView(float targetYaw, float targetPitch, float targetDistance)
    {
        yaw = targetYaw;
        pitch = Mathf.Clamp(targetPitch, minVerticalAngle, maxVerticalAngle);
        distance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
    }

    public void SetFrontView()
    {
        SetView(0f, 10f, distance);
    }

    public void SetSideView()
    {
        SetView(90f, 10f, distance);
    }

    public void SetBackView()
    {
        SetView(180f, 10f, distance);
    }

    public void ResetView()
    {
        SetView(0f, 10f, 4f);
    }
}