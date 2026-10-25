using UnityEngine;

[CreateAssetMenu(fileName = "WeaknessDebuff", menuName = "Core Abilities/Debuff/Weakness")]
public class WeaknessDebuff : CoreAbilityBase
{
    [Header("Дебаф")]
    [SerializeField] private float duration = 15f;
    [SerializeField] private float damageMultiplier = 0.5f; // Урон врага / 2

    public WeaknessDebuff()
    {
        abilityName = "Слабость";
        abilityType = AbilityType.BuffDebuff;
        energyCost = 20;
        heatGenerated = 10f;
    }

    protected override void Execute(PlayerCore core, PlayerStateManager stateManager, PlayerMovement movement)
    {
        // Находим ближайшего врага
        Collider[] enemies = Physics.OverlapSphere(core.transform.position, 20f, LayerMask.GetMask("Enemy"));

        if (enemies.Length > 0)
        {
            // Применяем дебаф к ближайшему врагу
            EnemyStateManager nearestEnemy = null;
            float nearestDistance = Mathf.Infinity;

            foreach (var enemy in enemies)
            {
                EnemyStateManager enemyState = enemy.GetComponent<EnemyStateManager>();
                if (enemyState != null)
                {
                    float dist = Vector3.Distance(enemy.transform.position, core.transform.position);
                    if (dist < nearestDistance)
                    {
                        nearestDistance = dist;
                        nearestEnemy = enemyState;
                    }
                }
            }

            if (nearestEnemy != null)
            {
                nearestEnemy.ApplyDamageMultiplier(damageMultiplier, duration);
                Debug.Log($"[ДЕБАФ] Слабость применена к {nearestEnemy.name} на {duration} сек!");
            }
        }
    }
}