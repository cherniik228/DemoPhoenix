using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;

public class PlayerCore : MonoBehaviour
{
    [Header("Связанные компоненты")]
    [SerializeField] private PlayerStateManager stateManager;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private BodyPart[] bodyParts;
    [SerializeField] private CoreOverheatSystem overheatSystem;
    [SerializeField] private CoreVisual coreVisual;

    [Header("Энергия ядра")]
    [SerializeField] private int maxCoreEnergy = 100;
    [SerializeField] private float energyRegenRate = 5f;

    [Header("Ремонт")]
    [SerializeField] private int repairEnergyCost = 30;
    [SerializeField] private float repairCooldown = 2f;

    [Header("Способности")]
    [SerializeField] private List<CoreAbilityBase> abilities = new List<CoreAbilityBase>();

    [Header("Клавиши категорий способностей (по умолчанию: 4,5,6,7)")]
    [SerializeField] private Key combatKey = Key.Digit4;      // красные (Combat) — пила/лазеры
    [SerializeField] private Key buffDebuffKey = Key.Digit5;  // жёлтые (BuffDebuff)
    [SerializeField] private Key regenKey = Key.Digit6;       // зелёные (Regeneration)
    [SerializeField] private Key ultimateKey = Key.Digit7;    // фиолетовые (Ultimate)

    private int currentEnergy;
    private float repairCooldownTimer;
    private bool isInvincible;
    private float invincibilityTimer;

    // События для UI и других систем (вместо polling)
    public event Action<int, int> OnEnergyChanged; // (current, max)
    public event Action OnBecameOverheated;
    public event Action OnCooledDown;
    public event Action OnRepairPerformed;

    public int CurrentEnergy => currentEnergy;
    public int MaxEnergy => maxCoreEnergy;
    public float EnergyPercent => maxCoreEnergy > 0 ? (float)currentEnergy / maxCoreEnergy : 0f;
    public bool IsOverheated => overheatSystem != null && overheatSystem.IsOverheated;
    public bool IsInvincible => isInvincible;
    public CoreVisual CoreVisual => coreVisual;

    public bool CanUseAbility => !IsOverheated && currentEnergy > 0 && !isInvincible;
    public bool CanRepair => !IsOverheated &&
                             currentEnergy >= repairEnergyCost &&
                             repairCooldownTimer <= 0f;

    private Keyboard keyboard;

    private void Awake()
    {
        currentEnergy = maxCoreEnergy;
        keyboard = Keyboard.current;

        if (overheatSystem == null)
            overheatSystem = GetComponent<CoreOverheatSystem>();
        if (coreVisual == null)
            coreVisual = GetComponent<CoreVisual>();

        if (overheatSystem != null)
        {
            overheatSystem.OnOverheated += HandleBecameOverheated;
            overheatSystem.OnCooled += HandleCooledDown;
        }
    }

    private void OnDestroy()
    {
        if (overheatSystem != null)
        {
            overheatSystem.OnOverheated -= HandleBecameOverheated;
            overheatSystem.OnCooled -= HandleCooledDown;
        }
    }

    private void HandleBecameOverheated() => OnBecameOverheated?.Invoke();
    private void HandleCooledDown() => OnCooledDown?.Invoke();

    private void Update()
    {
        HandleTimers();
        HandleInput();
        RegenerateEnergy();
        HandleInvincibility();
    }

    private void HandleInput()
    {
        if (keyboard == null) return;

        // Ремонт на Q
        if (keyboard[Key.Q].wasPressedThisFrame && CanRepair)
        {
            RepairAllParts();
        }

        // Способности по категориям: 4 — красные, 5 — жёлтые, 6 — зелёные, 7 — фиолетовые
        TryActivateCategory(combatKey, CoreAbilityBase.AbilityType.Combat);
        TryActivateCategory(buffDebuffKey, CoreAbilityBase.AbilityType.BuffDebuff);
        TryActivateCategory(regenKey, CoreAbilityBase.AbilityType.Regeneration);
        TryActivateCategory(ultimateKey, CoreAbilityBase.AbilityType.Ultimate);
    }

    /// <summary>Активирует первую доступную способность указанной категории.</summary>
    private void TryActivateCategory(Key key, CoreAbilityBase.AbilityType type)
    {
        if (!keyboard[key].wasPressedThisFrame) return;

        foreach (var ability in abilities)
        {
            if (ability == null || ability.abilityType != type) continue;

            if (CanUseAbility && ability.CanActivate(this))
            {
                ActivateAbility(ability);
                Debug.Log($"[СПОСОБНОСТЬ] Категория «{GetCategoryName(type)}» активирована: {ability.abilityName}");
                return;
            }
            else
            {
                string reason = IsOverheated ? "ядро перегрето"
                    : ability.IsOnCooldown ? $"перезарядка {ability.CooldownRemaining:F1} сек"
                    : currentEnergy < ability.energyCost ? $"не хватает энергии ({ability.energyCost})"
                    : "нельзя применить";
                Debug.Log($"[СПОСОБНОСТЬ] Категория «{GetCategoryName(type)}» недоступна: {reason}");
                return;
            }
        }
        Debug.Log($"[СПОСОБНОСТЬ] Категория «{GetCategoryName(type)}» не назначена ни на одну способность.");
    }

    private static string GetCategoryName(CoreAbilityBase.AbilityType type)
    {
        switch (type)
        {
            case CoreAbilityBase.AbilityType.Combat: return "Красные (боевые)";
            case CoreAbilityBase.AbilityType.BuffDebuff: return "Жёлтые (баффы/дебаффы)";
            case CoreAbilityBase.AbilityType.Regeneration: return "Зелёные (регенерация)";
            case CoreAbilityBase.AbilityType.Ultimate: return "Фиолетовые (ультимейт)";
            default: return type.ToString();
        }
    }

    private void ActivateAbility(CoreAbilityBase ability)
    {
        ability.Activate(this, stateManager, movement);
        SpendEnergy(ability.energyCost);

        if (overheatSystem != null)
            overheatSystem.AddHeat(ability.heatGenerated);
    }

    public void RepairAllParts()
    {
        if (!CanRepair) return;

        int brokenCount = 0;

        foreach (var part in bodyParts)
        {
            if (part != null && part.State == BodyPart.PartState.Broken)
            {
                part.RepairWithCore();
                brokenCount++;
            }
        }

        if (brokenCount > 0)
        {
            SpendEnergy(repairEnergyCost);
            repairCooldownTimer = repairCooldown;

            if (stateManager != null)
                stateManager.Heal(10);

            Debug.Log($"[PlayerCore] Починено частей: {brokenCount}");
            OnRepairPerformed?.Invoke();
        }
    }

    public bool UseAbility(int index)
    {
        if (index < 0 || index >= abilities.Count) return false;

        CoreAbilityBase ability = abilities[index];

        if (!CanUseAbility || !ability.CanActivate(this)) return false;

        ActivateAbility(ability);
        return true;
    }

    /// <summary>Списание энергии с событием (используют способности извне).</summary>
    public void SpendEnergy(int amount)
    {
        currentEnergy = Mathf.Clamp(currentEnergy - amount, 0, maxCoreEnergy);
        OnEnergyChanged?.Invoke(currentEnergy, maxCoreEnergy);
    }

    /// <summary>Возврат энергии (например, при отмене способности).</summary>
    public void RefundEnergy(int amount)
    {
        currentEnergy = Mathf.Clamp(currentEnergy + amount, 0, maxCoreEnergy);
        OnEnergyChanged?.Invoke(currentEnergy, maxCoreEnergy);
    }

    public CoreAbilityBase GetAbility(int index)
    {
        if (index >= 0 && index < abilities.Count)
            return abilities[index];
        return null;
    }

    public void ApplyInvincibility(float duration)
    {
        isInvincible = true;
        invincibilityTimer = duration;

        if (stateManager != null)
        {
            // Реальная неуязвимость: входящий урон x0 на время действия
            stateManager.AddEffect(new StatusEffect
            {
                effectName = "Invincibility",
                duration = duration,
                speedMultiplier = 1f,
                damageMultiplier = 0f
            });
        }
    }

    private void HandleInvincibility()
    {
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0f)
                isInvincible = false;
        }
    }

    private void RegenerateEnergy()
    {
        if (currentEnergy < maxCoreEnergy)
        {
            // Дробое накопление, чтобы регенерация не "проглатывалась" округлением
            regenAccumulator += energyRegenRate * Time.deltaTime;
            int whole = Mathf.FloorToInt(regenAccumulator);
            if (whole > 0)
            {
                regenAccumulator -= whole;
                SpendEnergy(-whole); // Refund via clamped helper
            }
        }
    }

    private float regenAccumulator;

    private void HandleTimers()
    {
        if (repairCooldownTimer > 0f)
            repairCooldownTimer -= Time.deltaTime;
    }
}
