using UnityEngine;

public class CoreVisual : MonoBehaviour
{
    [Header("—сылки")]
    [SerializeField] private Renderer coreRenderer;
    [SerializeField] private ParticleSystem regenerationEffect;
    [SerializeField] private ParticleSystem buffEffect;
    [SerializeField] private ParticleSystem ultimateEffect;

    [Header("÷вета состо€ний")]
    [SerializeField] private Color normalColor = Color.cyan;
    [SerializeField] private Color overheatedColor = Color.red;

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
        // ѕлавный переход цвета
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