using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    [Header("Зависимости")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask playerLayerMask;

    [Header("Настройки Атаки")]
    [SerializeField] private float attackRadius = 2f;
    [SerializeField] private int attackDamage = 15;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float attackRange = 3f; // Дистанция для начала атаки

    [Header("Настройки Урона по Конечностям")]
    [Tooltip("Если true, урон наносится случайной конечности. Если false - по торсу")]
    [SerializeField] private bool targetRandomBodyPart = true;
    [SerializeField] private float bodyPartDamageMultiplier = 1f;

    [Header("Визуализация")]
    [SerializeField] private Color attackRadiusColor = Color.red;
    [SerializeField] private Color attackRangeColor = Color.yellow;

    private float attackCooldownTimer;
    private bool isAttacking;
    private System.Collections.Generic.List<BodyPart> activePartsCache;

    private void Update()
    {
        HandleTimers();

        if (!isAttacking && attackCooldownTimer <= 0)
        {
            TryAttack();
        }
    }

    private void TryAttack()
    {
        // Проверяем, есть ли игрок в радиусе атаки
        Collider[] hits = Physics.OverlapSphere(attackPoint.position, attackRadius, playerLayerMask);

        if (hits.Length > 0)
        {
            PerformAttack(hits);
        }
    }

    private void PerformAttack(Collider[] hits)
    {
        isAttacking = true;
        attackCooldownTimer = attackCooldown;

        Debug.Log($"[ВРАГ] Атака! Найдено целей: {hits.Length}");

        foreach (var hit in hits)
        {
            // Ищем PlayerStateManager на игроке
            PlayerStateManager playerState = hit.GetComponentInParent<PlayerStateManager>();

            if (playerState != null)
            {
                Vector3 hitDirection = (hit.transform.position - transform.position).normalized;

                if (targetRandomBodyPart)
                {
                    // Наносим урон случайной конечности
                    DamageRandomBodyPart(playerState, hitDirection);
                }
                else
                {
                    // Наносим урон по общему HP
                    playerState.TakeDamage(attackDamage, hitDirection);
                }
            }
        }

        // Здесь можно запустить анимацию атаки
        // animator.SetTrigger("Attack");

        Invoke(nameof(ResetAttack), 0.5f);
    }

    /// <summary>
    /// Нанести урон случайной конечности игрока
    /// </summary>
    private void DamageRandomBodyPart(PlayerStateManager playerState, Vector3 hitDirection)
    {
        // Получаем все активные конечности игрока
        BodyPart[] allParts = new BodyPart[]
        {
            playerState.Head,
            playerState.Torso,
            playerState.LeftArm,
            playerState.RightArm,
            playerState.LeftLeg,
            playerState.RightLeg
        };

        // Фильтруем только активные (не оторванные)
        if (activePartsCache == null)
            activePartsCache = new System.Collections.Generic.List<BodyPart>(8);
        activePartsCache.Clear();

        foreach (var part in allParts)
        {
            if (part != null && part.IsActive)
            {
                activePartsCache.Add(part);
            }
        }

        if (activePartsCache.Count == 0)
        {
            // Если все конечности оторваны, наносим урон по общему HP
            playerState.TakeDamage(attackDamage, hitDirection);
            Debug.Log("[ВРАГ] Все конечности оторваны! Урон по общему HP.");
            return;
        }

        // Выбираем случайную конечность
        BodyPart randomPart = activePartsCache[Random.Range(0, activePartsCache.Count)];

        int finalDamage = Mathf.RoundToInt(attackDamage * bodyPartDamageMultiplier);
        randomPart.TakeDamage(finalDamage, hitDirection);

        Debug.Log($"[ВРАГ] Урон {finalDamage} по конечности: {randomPart.PartName}");
    }

    private void ResetAttack()
    {
        isAttacking = false;
    }

    private void HandleTimers()
    {
        if (attackCooldownTimer > 0)
        {
            attackCooldownTimer -= Time.deltaTime;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            // Радиус поражения
            Gizmos.color = attackRadiusColor;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);

            // Дистанция начала атаки
            Gizmos.color = attackRangeColor;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}