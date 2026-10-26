using UnityEngine;

[CreateAssetMenu(fileName = "ChainsawAbility", menuName = "Core Abilities/Combat/Chainsaw")]
public class ChainsawAbility : CoreAbilityBase
{
    [Header("Бензопила")]
    [SerializeField] private float chainsawDamage = 10f; // Урон в секунду
    [SerializeField] private float duration = 3f;
    [SerializeField] private float attackRadius = 1.5f;
    [SerializeField] private LayerMask enemyMask;

    private float damageTimer;

    public ChainsawAbility()
    {
        abilityName = "Бензопила";
        abilityType = AbilityType.Combat;
        energyCost = 20;
        heatGenerated = 20f; // Высокий нагрев
    }

    protected override void Execute(PlayerCore core, PlayerStateManager stateManager, PlayerMovement movement)
    {
        if (stateManager.LeftArm == null || !stateManager.LeftArm.IsActive)
        {
            Debug.Log("[БЕНЗОПИЛА] Левая рука отсутствует!");
            return;
        }

        // Запускаем бензопилу на duration секунд
        core.StartCoroutine(ChainsawCoroutine(core, stateManager));
    }

    private System.Collections.IEnumerator ChainsawCoroutine(PlayerCore core, PlayerStateManager stateManager)
    {
        float timer = 0f;
        float damageInterval = 0.5f; // Урон каждые 0.5 сек

        Debug.Log("[БЕНЗОПИЛА] Активирована!");

        while (timer < duration)
        {
            if (timer >= damageTimer)
            {
                // Наносим урон всем в радиусе
                Collider[] enemies = Physics.OverlapSphere(core.transform.position, attackRadius, enemyMask);

                foreach (var enemy in enemies)
                {
                    if (enemy.TryGetComponent<IDamageable>(out IDamageable damageable))
                    {
                        Vector3 dir = (enemy.transform.position - core.transform.position).normalized;
                        damageable.TakeDamage(Mathf.RoundToInt(chainsawDamage), dir);
                    }
                }

                damageTimer = timer + damageInterval;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        Debug.Log("[БЕНЗОПИЛА] Завершена!");
    }
}