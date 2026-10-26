using UnityEngine;

[CreateAssetMenu(fileName = "ReviveAbility", menuName = "Core Abilities/Ultimate/Revive")]
public class ReviveAbility : CoreAbilityBase
{
    [Header("Возрождение")]
    [SerializeField] private float invincibilityDuration = 5f;

    public ReviveAbility()
    {
        abilityName = "Возрождение";
        abilityType = AbilityType.Ultimate;
        energyCost = 100; // Максимальная стоимость
        heatGenerated = 100f; // Максимальный нагрев!
    }

    protected override void Execute(PlayerCore core, PlayerStateManager stateManager, PlayerMovement movement)
    {
        // Сохраняем позицию смерти
        Vector3 deathPosition = core.transform.position;

        // Возрождаем игрока
        stateManager.Revive();

        // Восстанавливаем все конечности
        foreach (var part in stateManager.BodyParts)
        {
            if (part != null && part.State != BodyPart.PartState.Severed)
            {
                part.RepairWithCore();
            }
        }

        // Телепортируем на место смерти
        core.transform.position = deathPosition;

        // Даем неуязвимость
        core.ApplyInvincibility(invincibilityDuration);

        // Визуальный эффект
        if (core.CoreVisual != null)
        {
            core.CoreVisual.PlayUltimateEffect();
        }

        Debug.Log($"[УЛЬТИМЕЙТ] Возрождение! Неуязвимость на {invincibilityDuration} сек!");
    }
}