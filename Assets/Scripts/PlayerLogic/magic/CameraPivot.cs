using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private Transform cameraPivot; // Пустой объект на КОРНЕ игрока (НЕ на модели!)
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform target; // Корень игрока (НЕ модель!)

    [Header("Настройки камеры")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float minPitch = -30f;
    [SerializeField] private float maxPitch = 60f;
    [SerializeField] private float distance = 5f;
    [SerializeField] private float pivotHeight = 1.5f;

    [Header("Плавность")]
    [SerializeField] private float positionSmoothTime = 0.2f;

    private float xRotation = 0f;
    private float yRotation = 0f;
    private Vector3 positionSmoothVelocity;

    private bool isLockedOn = false;
    private Transform lockOnTarget;
    private PlayerInputActions inputActions;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        if (target == null) target = cameraPivot != null ? cameraPivot.parent : transform.parent;
        if (mainCamera == null) mainCamera = GetComponentInChildren<Camera>();
        if (mainCamera == null) mainCamera = Camera.main;
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        yRotation = target.eulerAngles.y;
        xRotation = 10f;
    }

    private void LateUpdate()
    {
        if (cameraPivot == null || mainCamera == null || target == null) return;

        Vector2 mouseDelta = Vector2.zero;
        if (Mouse.current != null)
        {
            mouseDelta = Mouse.current.delta.ReadValue();
        }

        // Вращение камеры мышью
        float mouseX = mouseDelta.x * mouseSensitivity * 0.1f;
        float mouseY = mouseDelta.y * mouseSensitivity * 0.1f;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minPitch, maxPitch);

        UpdateCamera();
    }

    private void UpdateCamera()
    {
        // Позиция пивота (центр вращения камеры)
        Vector3 pivotPosition = target.position + Vector3.up * pivotHeight;

        // Позиция камеры относительно пивота
        Vector3 offset = Quaternion.Euler(xRotation, yRotation, 0f) * Vector3.back * distance;
        Vector3 desiredPosition = pivotPosition + offset;

        // Коллизия со стенами
        if (Physics.Linecast(pivotPosition, desiredPosition, out RaycastHit hit))
        {
            desiredPosition = hit.point - (hit.normal * 0.3f);
        }

        // Плавное движение камеры
        mainCamera.transform.position = Vector3.SmoothDamp(
            mainCamera.transform.position,
            desiredPosition,
            ref positionSmoothVelocity,
            positionSmoothTime
        );

        // Камера смотрит на пивот
        mainCamera.transform.LookAt(pivotPosition);
    }

    public void SetLockOnTarget(Transform target)
    {
        lockOnTarget = target;
        isLockedOn = target != null;
    }

    public bool IsLockedOn() => isLockedOn;

    public Vector3 GetCameraForward()
    {
        Vector3 forward = mainCamera.transform.forward;
        forward.y = 0f;
        return forward.normalized;
    }

    public Vector3 GetCameraRight()
    {
        Vector3 right = mainCamera.transform.right;
        right.y = 0f;
        return right.normalized;
    }

    private void OnDrawGizmosSelected()
    {
        if (cameraPivot != null && mainCamera != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(cameraPivot.position, mainCamera.transform.position);
        }
    }
}