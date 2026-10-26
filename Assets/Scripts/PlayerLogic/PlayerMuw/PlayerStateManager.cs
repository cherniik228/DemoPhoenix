using UnityEngine;
using System;

/// <summary>
/// Система здоровья и частей тела игрока.
/// Наследуется от StateManagerBase (общая логика HP с врагами),
/// добавляет части тела, статусные эффекты и смерть от отрыва головы/торса.
/// </summary>
public class PlayerStateManager : StateManagerBase, IDamageable
{
    [Header("Части тела")]
    [SerializeField] private BodyPart[] bodyParts;

    public BodyPart[] BodyParts => bodyParts;

    [Header("Смерть от отрыва")]
    [SerializeField] private bool dieOnHeadSevered = true;
    [SerializeField] private bool dieOnTorsoSevered = true;

    [Header("Статусные эффекты")]
    [SerializeField] private StatusEffectSystem statusEffects;

    public event Action<BodyPart> OnPartDamaged;
    public event Action<BodyPart> OnPartBroken;
    public event Action<BodyPart> OnPartSevered;

    public BodyPart Head => GetPart(BodyPart.PartType.Head);
    public BodyPart Torso => GetPart(BodyPart.PartType.Torso);
    public BodyPart LeftArm => GetPart(BodyPart.PartType.LeftArm);
    public BodyPart RightArm => GetPart(BodyPart.PartType.RightArm);
    public BodyPart LeftLeg => GetPart(BodyPart.PartType.LeftLeg);
    public BodyPart RightLeg => GetPart(BodyPart.PartType.RightLeg);

    public bool HasBothArms => LeftArm?.IsActive == true && RightArm?.IsActive == true;
    public bool HasBothLegs => LeftLeg?.IsActive == true && RightLeg?.IsActive == true;
    public bool HasHead => Head?.IsActive == true;

    protected override void Awake()
    {
        base.Awake();

        if (statusEffects == null)
            statusEffects = GetComponent<StatusEffectSystem>();
        if (statusEffects == null)
            statusEffects = gameObject.AddComponent<StatusEffectSystem>();

        SubscribeToBodyParts();
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

    /// <summary>Враг бьёт по конкретной части тела; если её нет — урон по общему HP.</summary>
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

    public override void Revive()
    {
        base.Revive();

        // Возвращаем в работу компоненты, отключённые при смерти
        if (TryGetComponent<PlayerInputHandler>(out var input)) input.enabled = true;
        if (TryGetComponent<PlayerMovement>(out var move)) move.enabled = true;
        if (TryGetComponent<PlayerCombat>(out var combat)) combat.enabled = true;

        Debug.Log("[PlayerStateManager] Игрок воскрешён!");
    }

    private void HandlePartDamaged(BodyPart part, int currentHP, int maxHP)
    {
        OnPartDamaged?.Invoke(part);
    }

    private void HandlePartBroken(BodyPart part)
    {
        Debug.Log($"[PlayerStateManager] Часть сломана: {part.PartName}");
        OnPartBroken?.Invoke(part);
    }

    private void HandlePartSevered(BodyPart part)
    {
        Debug.Log($"[PlayerStateManager] Часть оторвана: {part.PartName}");
        OnPartSevered?.Invoke(part);
        CheckCriticalDeath(part);
    }

    private void CheckCriticalDeath(BodyPart severedPart)
    {
        bool shouldDie =
            (severedPart.Type == BodyPart.PartType.Head && dieOnHeadSevered) ||
            (severedPart.Type == BodyPart.PartType.Torso && dieOnTorsoSevered);

        if (shouldDie)
        {
            currentHealth = 0;
            Die();
        }
    }

    protected override void Die()
    {
        base.Die();
        Debug.Log("[PlayerStateManager] Игрок мёртв!");

        if (TryGetComponent<PlayerInputHandler>(out var input)) input.enabled = false;
        if (TryGetComponent<PlayerMovement>(out var move)) move.enabled = false;
        if (TryGetComponent<PlayerCombat>(out var combat)) combat.enabled = false;
    }

    // ---------- Статусные эффекты (делегированы в StatusEffectSystem) ----------

    public void AddEffect(StatusEffect newEffect) => statusEffects.AddEffect(newEffect);

    public bool HasEffect(string effectName) => statusEffects.HasEffect(effectName);

    public void RemoveEffect(string effectName) => statusEffects.RemoveEffect(effectName);

    public float GetSpeedMultiplier() => statusEffects.GetSpeedMultiplier();

    /// <summary>Множитель исходящего урона игрока (баффы типа EnhancedRegenBuff).</summary>
    public float GetDamageMultiplier() => statusEffects.GetDamageMultiplier();

    private void OnDestroy()
    {
        if (bodyParts == null) return;
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
