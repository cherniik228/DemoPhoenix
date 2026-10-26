using UnityEngine;
using System;

public class EnemyBodyPart : MonoBehaviour, IDamageable
{
    public enum PartType { Head, LeftArm, RightArm, LeftLeg, RightLeg, Torso }

    [Header("Настройки")]
    [SerializeField] private PartType partType = PartType.Torso;
    [SerializeField] private int maxHealth = 50;
    [SerializeField] private float severThreshold = 0f;

    [Header("Визуальные эффекты")]
    [SerializeField] private GameObject breakEffectPrefab;
    [SerializeField] private Color brokenColor = Color.yellow;

    private int currentHealth;
    private bool isBroken;
    private bool isSevered;
    private Renderer partRenderer;
    private Collider partCollider;
    private Rigidbody partRigidbody;
    private MaterialPropertyBlock propertyBlock;

    public event Action<EnemyBodyPart> OnPartBroken;
    public event Action<EnemyBodyPart> OnPartSevered;

    public PartType Type => partType;
    public bool IsBroken => isBroken;
    public bool IsSevered => isSevered;
    public bool IsActive => !isSevered;

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
        if (isSevered) return;

        currentHealth -= amount;
        Debug.Log($"[Враг-{partType}] Урон: {amount}. HP: {currentHealth}/{maxHealth}");

        UpdateVisuals();

        if (severThreshold > 0 && currentHealth <= severThreshold)
        {
            SeverPart(hitDirection);
        }
        else if (currentHealth <= 0 && !isBroken)
        {
            BreakPart(hitDirection);
        }
    }

    private void BreakPart(Vector3 hitDirection)
    {
        isBroken = true;
        Debug.Log($"[Враг-{partType}] СЛОМАНА!");

        if (breakEffectPrefab != null)
        {
            Instantiate(breakEffectPrefab, transform.position, transform.rotation);
        }

        if (partCollider != null)
        {
            partCollider.enabled = false;
        }

        UpdateVisuals();
        OnPartBroken?.Invoke(this);
    }

    private void SeverPart(Vector3 hitDirection)
    {
        isSevered = true;
        Debug.Log($"[Враг-{partType}] ОТОРВАНА!");

        if (breakEffectPrefab != null)
        {
            Instantiate(breakEffectPrefab, transform.position, transform.rotation);
        }

        if (partCollider != null)
        {
            partCollider.enabled = false;
        }

        if (partRigidbody == null)
        {
            partRigidbody = gameObject.AddComponent<Rigidbody>();
        }

        partRigidbody.isKinematic = false;
        partRigidbody.AddForce(hitDirection * 10f, ForceMode.Impulse);

        transform.SetParent(null, true);
        UpdateVisuals();
        OnPartSevered?.Invoke(this);
    }

    private void UpdateVisuals()
    {
        if (partRenderer == null) return;

        partRenderer.GetPropertyBlock(propertyBlock);
        Color targetColor = isSevered ? Color.red : (isBroken ? brokenColor : Color.white);

        propertyBlock.SetColor("_BaseColor", targetColor);
        propertyBlock.SetColor("_Color", targetColor);
        partRenderer.SetPropertyBlock(propertyBlock);
    }
}