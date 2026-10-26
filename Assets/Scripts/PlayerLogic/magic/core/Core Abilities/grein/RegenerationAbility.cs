using UnityEngine;

[CreateAssetMenu(fileName = "RegenerationAbility", menuName = "Core Abilities/Regeneration")]
public class RegenerationAbility : CoreAbilityBase
{
    [Header("Регенерация")]
    [SerializeField] private bool prioritizeCritical = true; // Приоритет голове/торсу/ядру
    [SerializeField] private int healAmount = 50;

    public RegenerationAbility()
    {
        abilityName = "Регенерация";
        abilityType = AbilityType.Regeneration;
        energyCost = 30;
        heatGenerated = 5f; // Малый нагрев
    }

    protected override void Execute(PlayerCore core, PlayerStateManager stateManager, PlayerMovement movement)
    {
        BodyPart targetPart = null;

        if (prioritizeCritical)
        {
            // Ищем самую важную сломанную часть
            targetPart = FindCriticalBrokenPart(stateManager);
        }

        // Если нет критических, ищем самую поврежденную
        if (targetPart == null)
        {
            targetPart = FindMostDamagedPart(stateManager);
        }

        if (targetPart != null)
        {
            targetPart.RepairWithCore();

            // Визуальный эффект регенерации
            if (core.CoreVisual != null)
            {
                core.CoreVisual.PlayRegenerationEffect(targetPart.transform.position);
            }

            Debug.Log($"[РЕГЕНЕРАЦИЯ] Восстановлена: {targetPart.PartName}");
        }
        else
        {
            Debug.Log("[РЕГЕНЕРАЦИЯ] Все конечности целы!");
        }
    }

    private BodyPart FindCriticalBrokenPart(PlayerStateManager stateManager)
    {
        // Приоритет: Голова > Торс > Ядро > Остальное
        if (stateManager.Head != null && stateManager.Head.IsBroken)
            return stateManager.Head;

        if (stateManager.Torso != null && stateManager.Torso.IsBroken)
            return stateManager.Torso;

        // Ядро не может быть сломано (оно либо цело, либо оторвано)

        return null;
    }

    private BodyPart FindMostDamagedPart(PlayerStateManager stateManager)
    {
        BodyPart mostDamaged = null;
        float lowestHealth = 1f;

        foreach (var part in stateManager.BodyParts)
        {
            if (part != null && part.IsBroken && part.HealthPercent < lowestHealth)
            {
                lowestHealth = part.HealthPercent;
                mostDamaged = part;
            }
        }

        return mostDamaged;
    }
}