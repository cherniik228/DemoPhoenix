using UnityEngine;

public class PlayerLockOn : MonoBehaviour
{
    [Header("Зависимости")]
    [SerializeField] private PlayerInputHandler inputHandler;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerStateManager stateManager;
    [SerializeField] private CameraController cameraController; // Перетащите камеру сюда!

    [Header("Настройки Lock-On")]
    [SerializeField] private float lockOnRadius = 10f;
    [SerializeField] private LayerMask enemyMask;

    private Transform currentLockOnTarget;
    public bool IsLockedOn { get; private set; }
    public Transform CurrentTarget => currentLockOnTarget;

    private void Update()
    {
        if (stateManager != null && stateManager.IsDead) return;
        if (movement != null && movement.IsDashing) return; // Нельзя лочиться во время рывка

        HandleLockOn();
    }

    private void HandleLockOn()
    {
        if (inputHandler.LockOnTriggered)
        {
            if (IsLockedOn) Unlock();
            else LockOn();
        }
    }

    private void LockOn()
    {
        Collider[] targets = Physics.OverlapSphere(transform.position, lockOnRadius, enemyMask);
        float closestDistance = Mathf.Infinity;
        Transform closestTarget = null;

        foreach (var target in targets)
        {
            float dist = Vector3.Distance(transform.position, target.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestTarget = target.transform;
            }
        }

        if (closestTarget != null)
        {
            currentLockOnTarget = closestTarget;
            IsLockedOn = true;
            if (cameraController != null) cameraController.SetLockOnTarget(closestTarget);
        }
    }

    private void Unlock()
    {
        IsLockedOn = false;
        currentLockOnTarget = null;
        if (cameraController != null) cameraController.SetLockOnTarget(null);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, lockOnRadius);
    }
}