using UnityEngine;

public class CoreVisual : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private Renderer coreRenderer;
    [SerializeField] private ParticleSystem regenerationEffect;
    [SerializeField] private ParticleSystem buffEffect;
    [SerializeField] private ParticleSystem ultimateEffect;

    [Header("Цвета состояний")]
    [SerializeField] private Color normalColor = Color.cyan;
    [SerializeField] private Color overheatedColor = Color.red;

    [Header("Цвет от нагрева ядра")]
    [Tooltip("Если включено, цвет ядра показывает только текущий нагрев (синий -> красный), а не категорию способности.")]
    [SerializeField] private bool driveColorFromHeat = true;
    [SerializeField] private Color coldColor = new Color(0.2f, 0.6f, 1f);
    [SerializeField] private Color hotColor = new Color(1f, 0.45f, 0.1f);
    [SerializeField] private CoreOverheatSystem overheatSource;

    private MaterialPropertyBlock propertyBlock;
    private Color currentTargetColor = Color.cyan;
    private float colorTransitionSpeed = 2f;

    private void Awake()
    {
        if (coreRenderer != null)
        {
            propertyBlock = new MaterialPropertyBlock();
        }
    }

    private void Update()
    {
        // Цвет ядра отражает только текущий нагрев (категории больше не влияют на цвет)
        if (driveColorFromHeat && overheatSource != null)
        {
            if (overheatSource.IsOverheated)
            {
                currentTargetColor = overheatedColor;
            }
            else
            {
                float t = Mathf.InverseLerp(0f, Mathf.Max(0.01f, overheatSource.OverheatThreshold),
                                            overheatSource.CurrentHeat);
                currentTargetColor = Color.Lerp(coldColor, hotColor, t);
            }
        }

        // Плавный переход цвета
        if (coreRenderer != null)
        {
            coreRenderer.GetPropertyBlock(propertyBlock);
            Color currentColor = propertyBlock.GetColor("_BaseColor");
            Color newColor = Color.Lerp(currentColor, currentTargetColor, Time.deltaTime * colorTransitionSpeed);

            propertyBlock.SetColor("_BaseColor", newColor);
            propertyBlock.SetColor("_Color", newColor);
            coreRenderer.SetPropertyBlock(propertyBlock);
        }
    }

    public void SetCoreColor(Color color)
    {
        currentTargetColor = color;
    }

    public void SetOverheated(bool overheated)
    {
        currentTargetColor = overheated ? overheatedColor : normalColor;
    }

    public void PlayRegenerationEffect(Vector3 position)
    {
        if (regenerationEffect != null)
        {
            ParticleSystem effect = Instantiate(regenerationEffect, position, Quaternion.identity);
            effect.Play();
            Destroy(effect.gameObject, effect.main.duration);
        }
    }

    public void PlayBuffEffect(Color color)
    {
        if (buffEffect != null)
        {
            ParticleSystem.MainModule main = buffEffect.main;
            main.startColor = color;
            buffEffect.Play();
        }
    }

    public void PlayUltimateEffect()
    {
        if (ultimateEffect != null)
        {
            ultimateEffect.Play();
        }
    }
}