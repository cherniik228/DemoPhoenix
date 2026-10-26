using UnityEngine;

[CreateAssetMenu(fileName = "LaserShotAbility", menuName = "Core Abilities/Combat/Laser Shot")]
public class LaserShotAbility : CoreAbilityBase
{
    [Header("Лазер")]
    [SerializeField] private float laserDamage = 30f;
    [SerializeField] private float laserRange = 50f;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private Transform laserSpawnPoint;

    public LaserShotAbility()
    {
        abilityName = "Лазерный выстрел";
        abilityType = AbilityType.Combat;
        energyCost = 25;
        heatGenerated = 15f; // Средний нагрев
    }

    protected override void Execute(PlayerCore core, PlayerStateManager stateManager, PlayerMovement movement)
    {
        // Проверяем, есть ли правая рука (для лазера)
        if (stateManager.RightArm == null || !stateManager.RightArm.IsActive)
        {
            Debug.Log("[ЛАЗЕР] Правая рука отсутствует!");
            return;
        }

        // Находим цель
        Ray ray = new Ray(laserSpawnPoint != null ? laserSpawnPoint.position : core.transform.position,
                         core.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, laserRange, enemyMask))
        {
            // Наносим урон
            if (hit.collider.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                damageable.TakeDamage(Mathf.RoundToInt(laserDamage), hit.point - ray.origin);
                Debug.Log($"[ЛАЗЕР] Урон {laserDamage} по {hit.collider.name}");
            }

            // Визуальный эффект лазера
            if (laserPrefab != null)
            {
                GameObject laser = Instantiate(laserPrefab, laserSpawnPoint.position, Quaternion.identity);
                LaserBeam beam = laser.GetComponent<LaserBeam>();
                if (beam != null)
                {
                    beam.Setup(laserSpawnPoint.position, hit.point);
                }
                Destroy(laser, 0.5f);
            }
        }
    }
}