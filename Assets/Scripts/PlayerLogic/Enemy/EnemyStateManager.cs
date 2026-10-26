using UnityEngine;
using System;

/// <summary>
/// Система здоровья врага. Наследует общую логику HP у StateManagerBase,
/// добавляет поддержку дебаффов (WeaknessDebuff) через StatusEffectSystem.
/// </summary>
public class EnemyStateManager : StateManagerBase, IDamageable
{
    [Header("Части тела (опционально)")]
    [SerializeField] private EnemyBodyPart[] bodyParts;

    public EnemyBodyPart[] BodyParts => bodyParts;

    [Header("Статусные эффекты")]
    [SerializeField] private StatusEffectSystem statusEffects;

    protected override void Awake()
    {
        base.Awake();

        if (statusEffects == null)
            statusEffects = GetComponent<StatusEffectSystem>();
        if (statusEffects == null)
            statusEffects = gameObject.AddComponent<StatusEffectSystem>();
    }

    /// <summary>
    /// Накладывает множитель входящего урона на время (используется WeaknessDebuff).
    /// Реализовано через реальную систему эффектов вместо заглушки-коротины.
    /// </summary>
    public void ApplyDamageMultiplier(float multiplier, float duration)
    {
        statusEffects.AddEffect(new StatusEffect
        {
            effectName = "Weakness",
            duration = duration,
            speedMultiplier = 1f,
            damageMultiplier = Mathf.Max(0f, multiplier)
        });
        Debug.Log($"[EnemyStateManager] {name}: входящий урон x{multiplier} на {duration}с");
    }

    /// <summary>Базовый TakeDamage уже применяет GetIncomingDamageMultiplier в StateManagerBase.</summary>
    protected override float GetIncomingDamageMultiplier()
    {
        return statusEffects != null ? statusEffects.GetDamageMultiplier() : 1f;
    }

    protected override void Die()
    {
        base.Die();
        Debug.Log($"[EnemyStateManager] {name} мёртв!");

        if (TryGetComponent<EnemyCombat>(out var combat)) combat.enabled = false;
    }
}
