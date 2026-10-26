using System;
using UnityEngine;

/// <summary>
/// Базовая система здоровья: общая для игрока и врага.
/// Хранит HP, события изменения здоровья и смерть.
/// </summary>
public abstract class StateManagerBase : MonoBehaviour, IDamageable
{
    [Header("Здоровье")]
    [SerializeField] protected int maxHealth = 100;
    protected int currentHealth;

    public bool IsDead { get; protected set; }

    public event Action<int, int> OnHealthChanged; // (current, max)
    public event Action OnDeath;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public float HealthPercent => maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }

    public virtual void TakeDamage(int amount, Vector3 hitDirection)
    {
        if (IsDead || amount <= 0) return;

        int finalDamage = Mathf.RoundToInt(amount * GetIncomingDamageMultiplier());
        currentHealth -= finalDamage;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        HandleDamaged(finalDamage, hitDirection);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    public virtual void Heal(int amount)
    {
        if (IsDead || amount <= 0) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>Полное восстановление и оживление.</summary>
    public virtual void Revive()
    {
        IsDead = false;
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    protected virtual void Die()
    {
        IsDead = true;
        OnDeath?.Invoke();
    }

    /// <summary>Хук: вызывается после нанесения урона (для логов/эффектов).</summary>
    protected virtual void HandleDamaged(int appliedDamage, Vector3 hitDirection) { }

    /// <summary>Множитель входящего урона (например, дебафф слабости). По умолчанию 1.</summary>
    protected virtual float GetIncomingDamageMultiplier() => 1f;
}
