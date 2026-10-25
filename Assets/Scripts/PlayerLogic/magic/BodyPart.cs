using UnityEngine;
using System;

public class BodyPart : MonoBehaviour, IDamageable
{
    public enum PartType { Head, LeftArm, RightArm, LeftLeg, RightLeg, Torso, Core }
    public enum PartState { Intact, Broken, Severed }

    [Header("Настройки части тела")]
    [SerializeField] private PartType partType = PartType.Torso;
    [SerializeField] private string partName = "Unknown";
    [SerializeField] private int maxHealth = 50;
    [SerializeField] private bool isCritical = false;
    [SerializeField] private float severThreshold = 0f; // Урон для отрыва (0 = нельзя оторвать)

    [Header("Визуальные эффекты")]
    [SerializeField] private GameObject breakEffectPrefab;
    [SerializeField] private GameObject severEffectPrefab;
    [SerializeField] private Color brokenColor = Color.yellow;
    [SerializeField] private Color severedColor = Color.red;

    [Header("Физика при отрыве")]
    [SerializeField] private bool usePhysics = true;
    [SerializeField] private float breakForce = 10f;
    [SerializeField] private float severForce = 20f;

    private int currentHealth;
    private PartState currentState = PartState.Intact;

    private Renderer partRenderer;
    private Collider partCollider;
    private Rigidbody partRigidbody;
    private MaterialPropertyBlock propertyBlock;

    // События
    public event Action<BodyPart> OnPartBroken;
    public event Action<BodyPart> OnPartSevered;
    public event Action<BodyPart, int, int> OnPartDamaged;

    // Public свойства
    public PartType Type => partType;
    public string PartName => partName;
    public PartState State => currentState;
    public float HealthPercent => maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
    public bool IsBroken => currentState == PartState.Broken;
    public bool IsSevered => currentState == PartState.Severed;
    public bool IsActive => currentState != PartState.Severed;
    public bool IsCritical => isCritical;

    private void Awake()
    {
        currentHealth = maxHealth;
        partRenderer = GetComponent<Renderer>();
        partCollider = GetComponent<Collider>();
        partRigidbody = GetComponent<Rigidbody>();

        if (partRenderer != null)
        {
            propertyBlock = new MaterialPropertyBlock();
        }
    }

    public void TakeDamage(int amount, Vector3 hitDirection)
    {
        if (currentState == PartState.Severed) return;

        currentHealth -= amount;
        OnPartDamaged?.Invoke(this, currentHealth, maxHealth);

        Debug.Log($"[{partName}] Урон: {amount}. HP: {currentHealth}/{maxHealth}");

        // Визуальное повреждение
        UpdateVisuals();

        // Проверка на отрыв
        if (severThreshold > 0 && currentHealth <= severThreshold)
        {
            SeverPart(hitDirection);
        }
        // Проверка на поломку
        else if (currentHealth <= 0 && currentState == PartState.Intact)
        {
            BreakPart(hitDirection);
        }
    }

    /// <summary>
    /// Сломать часть (можно починить ядром)
    /// </summary>
    private void BreakPart(Vector3 hitDirection)
    {
        currentState = PartState.Broken;
        Debug.Log($"[{partName}] СЛОМАНА (можно починить)!");

        if (breakEffectPrefab != null)
        {
            Instantiate(breakEffectPrefab, transform.position, transform.rotation);
        }

        if (partCollider != null && partType != PartType.Core)
        {
            partCollider.enabled = false;
        }

        if (usePhysics && partType != PartType.Core)
        {
            SetupPhysics(hitDirection, breakForce);
        }

        UpdateVisuals();
        OnPartBroken?.Invoke(this);
    }

    /// <summary>
    /// Оторвать часть (нельзя восстановить)
    /// </summary>
    private void SeverPart(Vector3 hitDirection)
    {
        currentState = PartState.Severed;
        Debug.Log($"[{partName}] ОТОРВАНА (необратимо)!");

        if (severEffectPrefab != null)
        {
            Instantiate(severEffectPrefab, transform.position, transform.rotation);
        }
        else if (breakEffectPrefab != null)
        {
            Instantiate(breakEffectPrefab, transform.position, transform.rotation);
        }

        if (partCollider != null)
        {
            partCollider.enabled = false;
        }

        if (usePhysics)
        {
            SetupPhysics(hitDirection, severForce);
        }

        // Отделяем от родителя
        transform.SetParent(null, true);

        UpdateVisuals();
        OnPartSevered?.Invoke(this);
    }

    /// <summary>
    /// Починить часть ядром (только если сломана, не оторвана!)
    /// </summary>
    public void RepairWithCore()
    {
        if (currentState == PartState.Severed)
        {
            Debug.Log($"[{partName}] Невозможно починить - оторвана!");
            return;
        }

        if (currentState == PartState.Broken)
        {
            currentState = PartState.Intact;
            currentHealth = maxHealth;

            Debug.Log($"[{partName}] ПОЧИНЕНА ядром!");

            // Восстанавливаем коллайдер
            if (partCollider != null)
            {
                partCollider.enabled = true;
            }

            // Отключаем физику
            if (partRigidbody != null)
            {
                Destroy(partRigidbody);
                partRigidbody = null;
            }

            // Возвращаем на место (если была отделена)
            // Примечание: нужно сохранить изначального родителя

            UpdateVisuals();
        }
        else
        {
            Debug.Log($"[{partName}] Не требует починки!");
        }
    }

    private void SetupPhysics(Vector3 force, float forceMultiplier)
    {
        if (partRigidbody == null)
        {
            partRigidbody = gameObject.AddComponent<Rigidbody>();
        }

        partRigidbody.isKinematic = false;
        partRigidbody.mass = 1f;
        partRigidbody.linearDamping = 0.5f;
        partRigidbody.angularDamping = 0.5f;
        partRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;

        partRigidbody.AddForce(force * forceMultiplier, ForceMode.Impulse);
        partRigidbody.AddTorque(UnityEngine.Random.insideUnitSphere * 5f, ForceMode.Impulse);
    }

    private void UpdateVisuals()
    {
        if (partRenderer == null) return;

        partRenderer.GetPropertyBlock(propertyBlock);

        Color targetColor = Color.white;

        switch (currentState)
        {
            case PartState.Broken:
                targetColor = brokenColor;
                break;
            case PartState.Severed:
                targetColor = severedColor;
                break;
            default:
                // Плавный переход от белого к цвету повреждения
                float damagePercent = 1f - HealthPercent;
                targetColor = Color.Lerp(Color.white, brokenColor, damagePercent * 0.5f);
                break;
        }

        propertyBlock.SetColor("_BaseColor", targetColor);
        propertyBlock.SetColor("_Color", targetColor);
        partRenderer.SetPropertyBlock(propertyBlock);
    }

    private void OnDestroy()
    {
        propertyBlock = null;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(partName))
        {
            partName = partType.ToString();
        }
    }
#endif
}