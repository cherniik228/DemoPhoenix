using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CoreUI : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private PlayerCore playerCore;
    [SerializeField] private CoreOverheatSystem overheatSystem;

    [Header("Элементы UI")]
    [SerializeField] private Slider energySlider;
    [SerializeField] private Image energyFill;
    [SerializeField] private TextMeshProUGUI energyText;
    [SerializeField] private TextMeshProUGUI stateText;
    [SerializeField] private Image coreIcon;

    [Header("Нагрев ядра")]
    [SerializeField] private Slider heatSlider;
    [SerializeField] private Image heatFill;

    [Header("Цвета состояний")]
    [SerializeField] private Color activeColor = Color.cyan;
    [SerializeField] private Color overheatedColor = Color.red;
    [SerializeField] private Color chargingColor = Color.yellow;
    [SerializeField] private Color regenerationColor = Color.green;
    [SerializeField] private Color combatColor = Color.red;
    [SerializeField] private Color buffColor = Color.yellow;
    [SerializeField] private Color ultimateColor = new Color(0.5f, 0f, 1f);

    [Header("Способности")]
    [SerializeField] private CoreAbilityUI[] abilityUIs;

    private void Update()
    {
        if (playerCore == null) return;

        UpdateCoreDisplay();
        UpdateHeatDisplay();
        UpdateAbilitiesDisplay();
    }

    private void UpdateCoreDisplay()
    {
        // Энергия
        if (energySlider != null)
        {
            energySlider.value = playerCore.EnergyPercent;
        }

        if (energyText != null)
        {
            energyText.text = $"{playerCore.CurrentEnergy}/{playerCore.MaxEnergy}";
        }

        // Состояние ядра: перегрев показываем поверх температуры, неуязвимость — отдельным текстом
        if (stateText != null)
        {
            if (playerCore.IsOverheated)
            {
                stateText.color = overheatedColor;
            }
            else if (playerCore.IsInvincible)
            {
                stateText.text = "НЕУЯЗВИМОСТЬ";
                stateText.color = ultimateColor;
            }
            else
            {
                stateText.color = activeColor;
            }
        }

        // Цвет иконки
        if (coreIcon != null)
        {
            if (playerCore.IsOverheated)
            {
                coreIcon.color = overheatedColor;
            }
            else
            {
                coreIcon.color = activeColor;
            }
        }
    }

    private void UpdateHeatDisplay()
    {
        if (overheatSystem == null) return;

        if (heatSlider != null)
        {
            heatSlider.value = overheatSystem.HeatPercent;
        }

        // Подпись с текущим состоянием ядра и температурой (видно без консоли)
        if (stateText != null && !playerCore.IsInvincible)
        {
            stateText.text = $"{overheatSystem.State}  {overheatSystem.CurrentHeat:F0}/{overheatSystem.MaxHeat:F0}";
        }

        if (heatFill != null)
        {
            if (overheatSystem.HeatPercent > 0.8f)
            {
                heatFill.color = Color.red;
            }
            else if (overheatSystem.HeatPercent > 0.5f)
            {
                heatFill.color = Color.yellow;
            }
            else
            {
                heatFill.color = Color.green;
            }
        }
    }

    private void UpdateAbilitiesDisplay()
    {
        for (int i = 0; i < abilityUIs.Length; i++)
        {
            CoreAbilityBase ability = playerCore.GetAbility(i);
            if (ability != null && abilityUIs[i] != null)
            {
                abilityUIs[i].UpdateDisplay(ability);
            }
        }
    }

    public void SetCoreColor(Color color)
    {
        if (coreIcon != null)
        {
            coreIcon.color = color;
        }
    }
}

[System.Serializable]
public class CoreAbilityUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI abilityNameText;
    [SerializeField] private Image cooldownOverlay;
    [SerializeField] private TextMeshProUGUI cooldownText;
    [SerializeField] private Image energyCostIcon;
    [SerializeField] private KeyCode abilityKey;

    public void UpdateDisplay(CoreAbilityBase ability)
    {
        if (abilityNameText != null)
        {
            abilityNameText.text = ability.abilityName;
        }

        if (cooldownOverlay != null)
        {
            // Затемнение по остатку кулдауна (fill-метод Image.Vertical)
            cooldownOverlay.enabled = true;
            cooldownOverlay.fillAmount = ability.cooldown > 0f
                ? ability.CooldownRemaining / ability.cooldown
                : 0f;
        }

        if (cooldownText != null)
        {
            cooldownText.text = ability.IsOnCooldown
                ? ability.CooldownRemaining.ToString("0.0")
                : abilityKey.ToString();
        }
    }
}