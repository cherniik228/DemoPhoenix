using UnityEngine;

[CreateAssetMenu(fileName = "EnhancedRegenBuff", menuName = "Core Abilities/Buff/Enhanced Regeneration")]
public class EnhancedRegenBuff : CoreAbilityBase
{
    [Header("Баф")]
    [SerializeField] private float duration = 10f;
    [SerializeField] private float regenMultiplier = 2f; // x2 регенерация

    public EnhancedRegenBuff()
    {
        abilityName = "Усиленная регенерация";
        abilityType = AbilityType.BuffDebuff;
        energyCost = 15;
        heatGenerated = 8f; // Малый нагрев
    }

    protected override void Execute(PlayerCore core, PlayerStateManager stateManager, PlayerMovement movement)
    {
        // Добавляем баф усиленной регенерации
        StatusEffect buff = new StatusEffect
        {
            effectName = "Усиленная регенерация",
            duration = duration,
            speedMultiplier = 1f,
            damageMultiplier = 1f
        };

        stateManager.AddEffect(buff);

        // Визуальный эффект
        if (core.CoreVisual != null)
        {
            core.CoreVisual.PlayBuffEffect(Color.yellow);
        }

        Debug.Log($"[БАФ] Усиленная регенерация на {duration} сек!");
    }
}