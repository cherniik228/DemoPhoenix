using UnityEngine;
using System;

public class CoreOverheatSystem : MonoBehaviour
{
    public enum CoreState { Active, Overheated, Charging }

    [Header("Настройки нагрева")]
    [SerializeField] private float maxHeat = 100f;
    [SerializeField] private float coolingRate = 10f; // Увеличил скорость
    [SerializeField] private float overheatThreshold = 80f;

    [Header("Продвинутое охлаждение")]
    [SerializeField] private float advancedCoolingRate = 20f;
    [SerializeField] private bool hasAdvancedCooling = false;

    private float currentHeat;
    private bool isOverheated;
    private CoreState coreState = CoreState.Active;

    public float CurrentHeat => currentHeat;
    public float HeatPercent => maxHeat > 0 ? currentHeat / maxHeat : 0;
    public bool IsOverheated => isOverheated;
    public float MaxHeat => maxHeat;
    public CoreState State => coreState;

    public event Action<float> OnHeatChanged;
    public event Action OnOverheated;
    public event Action OnCooled;

    private void Update()
    {
        HandleCooling();
    }

    private void HandleCooling()
    {
        if (currentHeat > 0)
        {
            // Определяем скорость охлаждения
            float rate = coolingRate;

            // Если перегрет — используем продвинутое охлаждение
            if (isOverheated)
            {
                rate = hasAdvancedCooling ? advancedCoolingRate : coolingRate * 2f;
            }

            // Охлаждаем
            currentHeat -= rate * Time.deltaTime;

            // Проверяем минимальное значение
            if (currentHeat <= 0)
            {
                currentHeat = 0;

                // Если было перегрето — снимаем статус
                if (isOverheated)
                {
                    isOverheated = false;
                    coreState = CoreState.Active;
                    Debug.Log("[ЯДРО] Остыло! Ядро активно.");
                    OnCooled?.Invoke();
                }
            }

            // Если было перегрето, но тепло упало ниже порога
            if (isOverheated && currentHeat < overheatThreshold * 0.5f)
            {
                isOverheated = false;
                coreState = CoreState.Active;
                Debug.Log("[ЯДРО] Температура нормализовалась!");
                OnCooled?.Invoke();
            }

            OnHeatChanged?.Invoke(currentHeat);
        }
    }

    public void AddHeat(float amount)
    {
        currentHeat += amount;
        currentHeat = Mathf.Min(maxHeat, currentHeat);

        Debug.Log($"[ЯДРО] Нагрев +{amount}. Текущее тепло: {currentHeat}/{maxHeat}");

        if (currentHeat >= overheatThreshold && !isOverheated)
        {
            isOverheated = true;
            coreState = CoreState.Overheated;
            Debug.LogWarning($"[ЯДРО] ПЕРЕГРЕВ! Тепло: {currentHeat}");
            OnOverheated?.Invoke();
        }
    }

    public void CoolDown(float amount)
    {
        currentHeat -= amount;
        currentHeat = Mathf.Max(0, currentHeat);
        Debug.Log($"[ЯДРО] Принудительное охлаждение -{amount}. Тепло: {currentHeat}");
    }

    public void ResetHeat()
    {
        currentHeat = 0;
        isOverheated = false;
        coreState = CoreState.Active;
        Debug.Log("[ЯДРО] Тепло сброшено!");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = currentHeat > overheatThreshold ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}