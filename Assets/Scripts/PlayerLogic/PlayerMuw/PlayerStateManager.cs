using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerStateManager : MonoBehaviour, IDamageable
{
    [Header("Здоровье")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;
    public bool IsDead { get; private set; }

    [Header("Конечности")]
    [SerializeField] private BodyPart[] bodyParts;

    public BodyPart[] BodyParts => bodyParts;

    [Header("Критические условия смерти")]
    [SerializeField] private bool dieOnHeadSevered = true;
    [SerializeField] private bool dieOnTorsoSevered = true;

    [Header("Бафы и Дебафы")]
    private List<StatusEffect> activeEffects = new List<StatusEffect>();

    public event Action<int, int> OnHealthChanged;
    public event Action<BodyPart> OnPartDamaged;
    public event Action<BodyPart> OnPartBroken;
    public event Action<BodyPart> OnPartSevered;
    public event Action OnDeath;

    public BodyPart Head => GetPart(BodyPart.PartType.Head);
    public BodyPart Torso => GetPart(BodyPart.PartType.Torso);
    public BodyPart LeftArm => GetPart(BodyPart.PartType.LeftArm);
    public BodyPart RightArm => GetPart(BodyPart.PartType.RightArm);
    public BodyPart LeftLeg => GetPart(BodyPart.PartType.LeftLeg);
    public BodyPart RightLeg => GetPart(BodyPart.PartType.RightLeg);

    public float HealthPercent => maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
    public bool HasBothArms => LeftArm?.IsActive == true && RightArm?.IsActive == true;
    public bool HasBothLegs => LeftLeg?.IsActive == true && RightLeg?.IsActive == true;
    public bool HasHead => Head?.IsActive == true;

    private void Awake()
    {
        currentHealth = maxHealth;
        SubscribeToBodyParts();
    }

    private void Update()
    {
        if (IsDead) return;
        UpdateEffects();
    }

    private void SubscribeToBodyParts()
    {
        foreach (var part in bodyParts)
        {
            if (part != null)
            {
                part.OnPartDamaged += HandlePartDamaged;
                part.OnPartBroken += HandlePartBroken;
                part.OnPartSevered += HandlePartSevered;
            }
        }
    }

    public BodyPart GetPart(BodyPart.PartType type)
    {
        foreach (var part in bodyParts)
        {
            if (part != null && part.Type == type)
                return part;
        }
        return null;
    }

    public void TakeDamage(int amount, Vector3 hitDirection)
    {
        if (IsDead) return;

        int finalDamage = Mathf.RoundToInt(amount * GetDamageMultiplier());
        currentHealth -= finalDamage;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        Debug.Log($"Игрок получил урон: {finalDamage}. HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    public void DamageBodyPart(BodyPart.PartType partType, int amount, Vector3 hitDirection)
    {
        BodyPart part = GetPart(partType);
        if (part != null && part.IsActive)
        {
            part.TakeDamage(amount, hitDirection);
        }
        else
        {
            TakeDamage(amount, hitDirection);
        }
    }

    public void Revive()
    {
        IsDead = false;
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log("[StateManager] Игрок возрожден!");
    }

    private void HandlePartDamaged(BodyPart part, int currentHP, int maxHP)
    {
        OnPartDamaged?.Invoke(part);
    }

    private void HandlePartBroken(BodyPart part)
    {
        Debug.Log($"[StateManager] Конечность сломана: {part.PartName}");
        OnPartBroken?.Invoke(part);
    }

    private void HandlePartSevered(BodyPart part)
    {
        Debug.Log($"[StateManager] Конечность оторвана: {part.PartName}");
        OnPartSevered?.Invoke(part);
        CheckCriticalDeath(part);
    }

    private void CheckCriticalDeath(BodyPart severedPart)
    {
        bool shouldDie = false;

        if (severedPart.Type == BodyPart.PartType.Head && dieOnHeadSevered)
        {
            shouldDie = true;
        }
        else if (severedPart.Type == BodyPart.PartType.Torso && dieOnTorsoSevered)
        {
            shouldDie = true;
        }

        if (shouldDie)
        {
            currentHealth = 0;
            Die();
        }
    }

    private void Die()
    {
        IsDead = true;
        Debug.Log("Игрок погиб!");
        OnDeath?.Invoke();

        if (TryGetComponent<PlayerInputHandler>(out var input)) input.enabled = false;
        if (TryGetComponent<PlayerMovement>(out var move)) move.enabled = false;
        if (TryGetComponent<PlayerCombat>(out var combat)) combat.enabled = false;
    }

    public void Heal(int amount)
    {
        if (IsDead) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void AddEffect(StatusEffect newEffect)
    {
        StatusEffect effectInstance = new StatusEffect
        {
            effectName = newEffect.effectName,
            duration = newEffect.duration,
            speedMultiplier = newEffect.speedMultiplier,
            damageMultiplier = newEffect.damageMultiplier
        };
        effectInstance.timer = effectInstance.duration;
        activeEffects.Add(effectInstance);
    }

    private void UpdateEffects()
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            activeEffects[i].timer -= Time.deltaTime;
            if (activeEffects[i].timer <= 0)
            {
                activeEffects.RemoveAt(i);
            }
        }
    }

    public float GetSpeedMultiplier()
    {
        float mult = 1f;
        foreach (var eff in activeEffects) mult *= eff.speedMultiplier;
        return mult;
    }

    public float GetDamageMultiplier()
    {
        float mult = 1f;
        foreach (var eff in activeEffects) mult *= eff.damageMultiplier;
        return mult;
    }

    private void OnDestroy()
    {
        foreach (var part in bodyParts)
        {
            if (part != null)
            {
                part.OnPartDamaged -= HandlePartDamaged;
                part.OnPartBroken -= HandlePartBroken;
                part.OnPartSevered -= HandlePartSevered;
            }
        }
    }
}