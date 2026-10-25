using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerCore : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private PlayerStateManager stateManager;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private BodyPart[] bodyParts;
    [SerializeField] private CoreOverheatSystem overheatSystem;
    [SerializeField] private CoreVisual coreVisual; // <-- Добавлено!

    [Header("Настройки ядра")]
    [SerializeField] private int maxCoreEnergy = 100;
    [SerializeField] private float energyRegenRate = 5f;
    [SerializeField] private float overheatedDuration = 5f;

    [Header("Починка")]
    [SerializeField] private int repairEnergyCost = 30;
    [SerializeField] private float repairCooldown = 2f;

    [Header("Способности")]
    [SerializeField] private List<CoreAbilityBase> abilities = new List<CoreAbilityBase>();
    [SerializeField] private KeyCode[] abilityKeys;

    private int currentEnergy;
    private CoreOverheatSystem.CoreState coreState = CoreOverheatSystem.CoreState.Active;
    private float overheatedTimer;
    private float repairCooldownTimer;
    private bool isInvincible;
    private float invincibilityTimer;

    // Публичные свойства
    public int CurrentEnergy => currentEnergy;
    public int MaxEnergy => maxCoreEnergy;
    public float EnergyPercent => (float)currentEnergy / maxCoreEnergy;
    public bool IsOverheated => overheatSystem != null && overheatSystem.IsOverheated;
    public bool IsInvincible => isInvincible;
    public CoreVisual CoreVisual => coreVisual; // <-- Добавлено!

    public bool CanUseAbility => coreState == CoreOverheatSystem.CoreState.Active && currentEnergy > 0 && !isInvincible;
    public bool CanRepair => coreState == CoreOverheatSystem.CoreState.Active &&
                             currentEnergy >= repairEnergyCost &&
                             repairCooldownTimer <= 0;

    private Keyboard keyboard;

    private void Awake()
    {
        currentEnergy = maxCoreEnergy;
        keyboard = Keyboard.current;

        if (overheatSystem == null)
        {
            overheatSystem = GetComponent<CoreOverheatSystem>();
        }
        if (coreVisual == null)
        {
            coreVisual = GetComponent<CoreVisual>();
        }
    }

    private void Update()
    {
        HandleTimers();
        HandleInput();
        RegenerateEnergy();
        HandleInvincibility();
    }

    private void HandleInput()
    {
        if (keyboard == null)
        {
            Debug.LogWarning("[PlayerCore] Keyboard не инициализирован!");
            return;
        }

        // Починка
        if (keyboard[Key.Q].wasPressedThisFrame)
        {
            Debug.Log("[INPUT] Нажата Q");
            if (CanRepair)
            {
                RepairAllParts();
            }
            else
            {
                Debug.Log($"[PlayerCore] Нельзя починить. Energy: {currentEnergy}, Cooldown: {repairCooldownTimer}");
            }
        }

        // Способности 1-7
        for (int i = 0; i < abilities.Count && i < abilityKeys.Length; i++)
        {
            KeyCode expectedKey = abilityKeys[i];
            Key unityKey = (Key)expectedKey;

            if (keyboard[unityKey].wasPressedThisFrame)
            {
                Debug.Log($"[INPUT] Нажата клавиша {i + 1} ({expectedKey})");
                Debug.Log($"[PlayerCore] Способностей: {abilities.Count}, CanUse: {CanUseAbility}");

                if (i < abilities.Count)
                {
                    CoreAbilityBase ability = abilities[i];
                    Debug.Log($"[PlayerCore] Попытка активировать: {ability.abilityName}");
                    Debug.Log($"[PlayerCore] Energy: {currentEnergy}/{maxCoreEnergy}, Cost: {ability.energyCost}");

                    bool result = UseAbility(i);
                    Debug.Log($"[PlayerCore] Результат: {result}");
                }
            }
        }
    }

    public void RepairAllParts()
    {
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
            currentEnergy -= repairEnergyCost;
            repairCooldownTimer = repairCooldown;

            if (stateManager != null)
            {
                stateManager.Heal(10);
            }

            Debug.Log($"[ЯДРО] Починено частей: {brokenCount}");
        }
    }

    public bool UseAbility(int index)
    {
        if (index < 0 || index >= abilities.Count) return false;

        CoreAbilityBase ability = abilities[index];

        if (!ability.CanActivate(this))
        {
            Debug.Log($"[ЯДРО] Нельзя активировать {ability.abilityName}!");
            return false;
        }

        ability.Activate(this, stateManager, movement);
        currentEnergy -= ability.energyCost;

        if (overheatSystem != null)
        {
            overheatSystem.AddHeat(ability.heatGenerated);
        }

        // Меняем цвет ядра
        if (coreVisual != null)
        {
            coreVisual.SetCoreColor(ability.GetCoreColor());
        }

        return true;
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
    }

    private void HandleInvincibility()
    {
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0)
            {
                isInvincible = false;
            }
        }
    }

    public void OverheatCore()
    {
        coreState = CoreOverheatSystem.CoreState.Overheated;
        overheatedTimer = overheatedDuration;
    }

    private void RegenerateEnergy()
    {
        if (currentEnergy < maxCoreEnergy)
        {
            int oldEnergy = currentEnergy;
            currentEnergy = Mathf.Min(maxCoreEnergy, currentEnergy + (int)(energyRegenRate * Time.deltaTime));

            // Только для отладки - раскомментируйте если нужно
             if (oldEnergy != currentEnergy && Time.frameCount % 60 == 0)
                 Debug.Log($"[REGEN] Energy: {oldEnergy} -> {currentEnergy}");
        }
    }

    private void HandleTimers()
    {
        if (coreState == CoreOverheatSystem.CoreState.Overheated)
        {
            overheatedTimer -= Time.deltaTime;
            if (overheatedTimer <= 0)
            {
                coreState = CoreOverheatSystem.CoreState.Active;
            }
        }

        if (repairCooldownTimer > 0)
        {
            repairCooldownTimer -= Time.deltaTime;
        }
    }
}