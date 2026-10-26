using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Универсальная система статусных эффектов (баффы/дебаффы).
/// Используется и игроком, и врагами — вместо дублирующего кода.
/// </summary>
public class StatusEffectSystem : MonoBehaviour
{
    private readonly List<StatusEffect> activeEffects = new List<StatusEffect>();

    public event Action<StatusEffect> OnEffectApplied;
    public event Action<StatusEffect> OnEffectExpired;

    public IReadOnlyList<StatusEffect> ActiveEffects => activeEffects;

    private void Update()
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            activeEffects[i].timer -= Time.deltaTime;
            if (activeEffects[i].timer <= 0f)
            {
                var expired = activeEffects[i];
                activeEffects.RemoveAt(i);
                OnEffectExpired?.Invoke(expired);
            }
        }
    }

    /// <summary>
    /// Накладывает копию эффекта (чтобы SO-шаблон не менялся).
    /// Повторная накладывание refreshing длительность одноимённого эффекта.
    /// </summary>
    public void AddEffect(StatusEffect template)
    {
        if (template == null) return;

        // Если такой эффект уже есть — обновляем таймер и параметры
        foreach (var existing in activeEffects)
        {
            if (existing.effectName == template.effectName)
            {
                existing.timer = template.duration;
                existing.speedMultiplier = template.speedMultiplier;
                existing.damageMultiplier = template.damageMultiplier;
                OnEffectApplied?.Invoke(existing);
                return;
            }
        }

        StatusEffect instance = new StatusEffect
        {
            effectName = template.effectName,
            duration = template.duration,
            speedMultiplier = template.speedMultiplier,
            damageMultiplier = template.damageMultiplier
        };
        instance.timer = instance.duration;
        activeEffects.Add(instance);
        OnEffectApplied?.Invoke(instance);
    }

    public bool HasEffect(string effectName)
    {
        foreach (var e in activeEffects)
            if (e.effectName == effectName) return true;
        return false;
    }

    public void RemoveEffect(string effectName)
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            if (activeEffects[i].effectName == effectName)
            {
                var removed = activeEffects[i];
                activeEffects.RemoveAt(i);
                OnEffectExpired?.Invoke(removed);
            }
        }
    }

    public void ClearEffects()
    {
        activeEffects.Clear();
    }

    /// <summary>Произведение множителей скорости всех активных эффектов.</summary>
    public float GetSpeedMultiplier()
    {
        float mult = 1f;
        foreach (var eff in activeEffects) mult *= eff.speedMultiplier;
        return mult;
    }

    /// <summary>Произведение множителей урона всех активных эффектов.</summary>
    public float GetDamageMultiplier()
    {
        float mult = 1f;
        foreach (var eff in activeEffects) mult *= eff.damageMultiplier;
        return mult;
    }
}
