using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Зависимости")]
    [SerializeField] private PlayerInputHandler inputHandler;
    [SerializeField] private PlayerLockOn lockOnSystem;
    [SerializeField] private PlayerStateManager stateManager;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private CameraController cameraController;

    [Header("Визуальная модель")]
    [SerializeField] private Transform modelTransform;

    [Header("Настройки Движения")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float backwardSpeedMult = 0.5f;

    [Header("Настройки Поворота")]
    [SerializeField] private float rotationSpeed = 720f;
    [SerializeField] private float rotationStopThreshold = 2f;

    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundDistance = 0.4f;

    [Header("Прыжок и Гравитация")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -15f;

    [Header("Рывок (Dash)")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;

    private CharacterController characterController;
    private Vector3 velocity;
    public bool IsGrounded { get; private set; }
    public bool IsDashing { get; private set; }

    private float dashCooldownTimer;
    private Vector3 lastMoveDirection = Vector3.forward;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        if (modelTransform == null)
        {
            Debug.LogError("Model Transform не назначен! Персонаж не будет поворачиваться.");
        }
    }

    private void Update()
    {
        if (stateManager != null && stateManager.IsDead) return;

        CheckGround();
        HandleTimers();

        if (!IsDashing)
        {
            HandleMovement();
            HandleJump();
            HandleDash();
        }

        ApplyGravity();
        characterController.Move(velocity * Time.deltaTime);
    }

    private void CheckGround()
    {
        IsGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (IsGrounded && velocity.y < 0) velocity.y = -2f;
    }

    private void HandleMovement()
    {
        Vector2 moveInput = inputHandler.MoveInput;
        float speedMult = stateManager != null ? stateManager.GetSpeedMultiplier() : 1f;
        Transform rotTarget = modelTransform != null ? modelTransform : transform;

        if (moveInput.magnitude < 0.1f)
        {
            velocity.x = 0f;
            velocity.z = 0f;

            if (lockOnSystem != null && lockOnSystem.IsLockedOn && lockOnSystem.CurrentTarget != null)
            {
                Vector3 dirToTarget = lockOnSystem.CurrentTarget.position - transform.position;
                dirToTarget.y = 0f;
                if (dirToTarget.magnitude > 0.01f)
                {
                    RotateToTarget(rotTarget, Quaternion.LookRotation(dirToTarget));
                }
            }
            return;
        }

        if (lockOnSystem != null && lockOnSystem.IsLockedOn && lockOnSystem.CurrentTarget != null)
        {
            Vector3 toTarget = lockOnSystem.CurrentTarget.position - transform.position;
            toTarget.y = 0f;
            toTarget.Normalize();

            Vector3 rightDir = Vector3.Cross(Vector3.up, toTarget).normalized;
            Vector3 moveDirection = (toTarget * moveInput.y) + (rightDir * moveInput.x);
            moveDirection.Normalize();

            bool isRunning = inputHandler.IsRunning;
            float baseSpeed = (isRunning ? runSpeed : walkSpeed) * speedMult;

            float currentSpeed = baseSpeed;
            if (moveInput.y < 0f && Mathf.Abs(moveInput.x) < 0.5f)
            {
                currentSpeed *= backwardSpeedMult;
            }

            velocity.x = moveDirection.x * currentSpeed;
            velocity.z = moveDirection.z * currentSpeed;
            lastMoveDirection = moveDirection;

            RotateToTarget(rotTarget, Quaternion.LookRotation(moveDirection));
        }
        else
        {
            Vector3 cameraForward;
            Vector3 cameraRight;

            if (cameraController != null)
            {
                cameraForward = cameraController.GetCameraForward();
                cameraRight = cameraController.GetCameraRight();
            }
            else
            {
                cameraForward = Vector3.forward;
                cameraRight = Vector3.right;
            }

            Vector3 moveDirection = (cameraForward * moveInput.y) + (cameraRight * moveInput.x);

            if (moveDirection.magnitude > 0.01f)
            {
                moveDirection.Normalize();
            }

            bool isRunning = inputHandler.IsRunning;
            float baseSpeed = (isRunning ? runSpeed : walkSpeed) * speedMult;

            float currentSpeed = baseSpeed;
            if (moveInput.y < 0f && Mathf.Abs(moveInput.x) < 0.5f)
            {
                currentSpeed *= backwardSpeedMult;
            }

            velocity.x = moveDirection.x * currentSpeed;
            velocity.z = moveDirection.z * currentSpeed;
            lastMoveDirection = moveDirection;

            RotateToTarget(rotTarget, Quaternion.LookRotation(moveDirection));
        }
    }

    // ==========================================
    // ИСПРАВЛЕННЫЙ МЕТОД ПОВОРОТА
    // ==========================================
    private void RotateToTarget(Transform target, Quaternion destination)
    {
        // КОРРЕКЦИЯ: Модель из Blender повёрнута на 90 градусов.
        // Мы поворачиваем целевой кватернион на -90 градусов по оси Y, 
        // чтобы локальная ось "вперёд" модели совпала с направлением движения.
        Quaternion correctedDestination = destination * Quaternion.Euler(0, -90f, 0);

        float angleDiff = Quaternion.Angle(target.rotation, correctedDestination);

        if (angleDiff < rotationStopThreshold)
        {
            target.rotation = correctedDestination;
            return;
        }

        target.rotation = Quaternion.RotateTowards(
            target.rotation,
            correctedDestination,
            rotationSpeed * Time.deltaTime
        );
    }
    // ==========================================

    private void HandleJump()
    {
        if (inputHandler.JumpTriggered && IsGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    private void ApplyGravity()
    {
        if (!IsGrounded && !IsDashing)
        {
            velocity.y += gravity * Time.deltaTime;
        }
    }

    private void HandleDash()
    {
        if (inputHandler.DashTriggered && dashCooldownTimer <= 0)
        {
            StartCoroutine(PerformDash());
        }
    }

    private IEnumerator PerformDash()
    {
        IsDashing = true;
        dashCooldownTimer = dashCooldown;

        Vector3 dashDirection = lastMoveDirection;
        float originalGravity = velocity.y;
        float timer = 0f;

        while (timer < dashDuration)
        {
            velocity.x = dashDirection.x * dashSpeed;
            velocity.z = dashDirection.z * dashSpeed;
            velocity.y = 0f;
            timer += Time.deltaTime;
            yield return null;
        }

        IsDashing = false;
        velocity.y = originalGravity;
    }

    private void HandleTimers()
    {
        dashCooldownTimer -= Time.deltaTime;
        if (dashCooldownTimer < 0) dashCooldownTimer = 0;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}