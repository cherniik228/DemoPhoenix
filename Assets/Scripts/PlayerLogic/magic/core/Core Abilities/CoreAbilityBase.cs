using UnityEngine;
using System;

public abstract class CoreAbilityBase : ScriptableObject
{
    public enum AbilityType { Regeneration, Combat, BuffDebuff, Ultimate }
    public enum HeatLevel { Low, Medium, High, Critical }

    [Header("Основное")]
    public string abilityName = "New Ability";
    public string description = "";
    public AbilityType abilityType;
    public Sprite icon;

    [Header("Настройки")]
    public int energyCost = 20;
    public float cooldown = 10f;
    public HeatLevel heatLevel = HeatLevel.Medium;

    [Header("Нагрев ядра")]
    public float heatGenerated = 10f; // Сколько тепла добавляет способность

    [System.NonSerialized] private float cooldownEndTime;

    // Событие активации
    public event Action<CoreAbilityBase> OnAbilityActivated;

    /// <summary>Осталось секунд до готовности способности.</summary>
    public float CooldownRemaining => Mathf.Max(0f, cooldownEndTime - Time.time);
    public bool IsOnCooldown => CooldownRemaining > 0f;

    private void StartCooldown()
    {
        cooldownEndTime = Time.time + cooldown;
    }

    /// <summary>
    /// Активировать способность
    /// </summary>
    public void Activate(PlayerCore core, PlayerStateManager stateManager, PlayerMovement movement)
    {
        if (CanActivate(core))
        {
            Execute(core, stateManager, movement);
            StartCooldown();
            OnAbilityActivated?.Invoke(this);
            Debug.Log($"[ЯДРО] Активировано: {abilityName}");
        }
    }

    /// <summary>
    /// Можно ли активировать
    /// </summary>
    public virtual bool CanActivate(PlayerCore core)
    {
        return core.CurrentEnergy >= energyCost && !core.IsOverheated && !IsOnCooldown;
    }

    /// <summary>
    /// Выполнить способность (переопределяется в наследниках)
    /// </summary>
    protected abstract void Execute(PlayerCore core, PlayerStateManager stateManager, PlayerMovement movement);

    /// <summary>
    /// Получить цвет ядра для этой способности
    /// </summary>
    public Color GetCoreColor()
    {
        switch (abilityType)
        {
            case AbilityType.Regeneration: return Color.green;
            case AbilityType.Combat: return Color.red;
            case AbilityType.BuffDebuff: return Color.yellow;
            case AbilityType.Ultimate: return new Color(0.5f, 0f, 1f); // Фиолетовый
            default: return Color.white;
        }
    }
}