using UnityEngine;
using System;

public class EnemyStateManager : MonoBehaviour, IDamageable
{
    [Header("Здоровье")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;
    public bool IsDead { get; private set; }

    [Header("Конечности")]
    [SerializeField] private EnemyBodyPart[] bodyParts;

    public event Action<int, int> OnHealthChanged;
    public event Action OnDeath;

    public float HealthPercent => maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount, Vector3 hitDirection)
    {
        if (IsDead) return;

        currentHealth -= amount;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    // ДОБАВЛЕНО: Метод для дебафа
    public void ApplyDamageMultiplier(float multiplier, float duration)
    {
        StartCoroutine(DamageMultiplierCoroutine(multiplier, duration));
    }

    private System.Collections.IEnumerator DamageMultiplierCoroutine(float multiplier, float duration)
    {
        // Здесь можно применить множитель урона
        Debug.Log($"[Враг] Урон x{multiplier} на {duration} сек");
        yield return new WaitForSeconds(duration);
        Debug.Log("[Враг] Дебаф закончился");
    }

    private void Die()
    {
        IsDead = true;
        Debug.Log("[Враг] Погиб!");
        OnDeath?.Invoke();

        if (TryGetComponent<EnemyCombat>(out var combat)) combat.enabled = false;
    }
}