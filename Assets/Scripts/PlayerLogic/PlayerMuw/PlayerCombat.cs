using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Зависимости")]
    [SerializeField] private PlayerInputHandler inputHandler;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerStateManager stateManager;
    [SerializeField] private Transform attackPoint;

    [Header("Настройки Атак")]
    [SerializeField] private float attackRadius = 1.5f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackCooldown = 0.5f;

    [Header("Заряженная Атака")]
    [SerializeField] private float maxChargeTime = 2f;
    [SerializeField] private float chargedAttackRadius = 2.5f;
    [SerializeField] private int chargedDamage = 30;
    [SerializeField] private float chargedAttackCooldown = 1.5f;

    [SerializeField] private LayerMask enemyMask;

    private float attackCooldownTimer;
    private float chargedAttackCooldownTimer;
    private bool isCharging;
    private float currentChargeTime;

    private void Update()
    {
        if (stateManager != null && stateManager.IsDead) return;
        if (movement != null && movement.IsDashing) return;

        HandleTimers();
        HandleCombat();
    }

    private void HandleCombat()
    {
        if (inputHandler.AttackTriggered && attackCooldownTimer <= 0)
        {
            int dmg = attackDamage;
            if (stateManager != null)
            {
                // ИСПРАВЛЕНО: используем множитель урона, а не скорости!
                dmg = Mathf.RoundToInt(dmg * stateManager.GetDamageMultiplier());
            }
            PerformAttack(attackRadius, dmg);
            attackCooldownTimer = attackCooldown;
        }

        if (inputHandler.IsChargeAttackPressed)
        {
            if (!isCharging && chargedAttackCooldownTimer <= 0)
            {
                isCharging = true;
                currentChargeTime = 0;
            }
            if (isCharging)
            {
                currentChargeTime += Time.deltaTime;
                if (currentChargeTime >= maxChargeTime)
                {
                    PerformAttack(chargedAttackRadius, chargedDamage);
                    isCharging = false;
                    chargedAttackCooldownTimer = chargedAttackCooldown;
                }
            }
        }
        else
        {
            if (isCharging && currentChargeTime > 0.2f)
            {
                float damageMultiplier = currentChargeTime / maxChargeTime;
                int finalDamage = Mathf.RoundToInt(chargedDamage * damageMultiplier);
                PerformAttack(chargedAttackRadius, finalDamage);
                chargedAttackCooldownTimer = chargedAttackCooldown;
            }
            isCharging = false;
            currentChargeTime = 0;
        }
    }

    private void PerformAttack(float radius, int damage)
    {
        Collider[] enemies = Physics.OverlapSphere(attackPoint.position, radius, enemyMask);
        foreach (var enemy in enemies)
        {
            if (enemy.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                Vector3 dir = (enemy.transform.position - transform.position).normalized;
                damageable.TakeDamage(damage, dir);
            }
        }
    }

    private void HandleTimers()
    {
        attackCooldownTimer -= Time.deltaTime;
        chargedAttackCooldownTimer -= Time.deltaTime;
        if (attackCooldownTimer < 0) attackCooldownTimer = 0;
        if (chargedAttackCooldownTimer < 0) chargedAttackCooldownTimer = 0;
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(attackPoint.position, chargedAttackRadius);
        }
    }
}