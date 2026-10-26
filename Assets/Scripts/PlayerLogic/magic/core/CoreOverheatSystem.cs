using UnityEngine;
using System;

public class CoreOverheatSystem : MonoBehaviour
{
    public enum CoreState { Active, Overheated, Charging }

    [Header("Настройки нагрева")]
    [SerializeField] private float maxHeat = 100f;
    [SerializeField] private float coolingRate = 15f; // ×1.5 к скорости остывания (было 10)
    [SerializeField] private float overheatThreshold = 80f;

    [Header("Продвинутое охлаждение")]
    [SerializeField] private float advancedCoolingRate = 30f; // ×1.5 (было 20)
    [SerializeField] private bool hasAdvancedCooling = false;

    [Header("Логирование состояния ядра")]
    [SerializeField] private bool logStateChanges = true;

    [Header("Периодический лог в консоль")]
    [Tooltip("Как часто печатать текущее состояние ядра в Console (сек). 0 — выключить периодический лог.")]
    [SerializeField] private float stateLogInterval = 1f;

    private float currentHeat;
    private bool isOverheated;
    private CoreState coreState = CoreState.Active;
    private float periodicLogTimer;

    public float CurrentHeat => currentHeat;
    public float HeatPercent => maxHeat > 0 ? currentHeat / maxHeat : 0;
    public bool IsOverheated => isOverheated;
    public float MaxHeat => maxHeat;
    public float CoolingRate => coolingRate;
    public CoreState State => coreState;

    public event Action<float> OnHeatChanged;
    public event Action OnOverheated;
    public event Action OnCooled;
    public event Action<CoreState> OnStateChanged;

    private void Awake()
    {
        LogState($"Инициализация: {coreState}, тепло {currentHeat}/{maxHeat}, остывание {coolingRate}/сек");
    }

    private void Update()
    {
        HandleCooling();
        HandlePeriodicLog();
    }

    private void HandlePeriodicLog()
    {
        if (!logStateChanges || stateLogInterval <= 0f) return;

        periodicLogTimer += Time.deltaTime;
        if (periodicLogTimer >= stateLogInterval)
        {
            periodicLogTimer = 0f;
            float rate = isOverheated
                ? (hasAdvancedCooling ? advancedCoolingRate : coolingRate * 2f)
                : coolingRate;
            LogState($"[периодический] тепло {currentHeat:F1}/{maxHeat}, остывание {rate}/сек, ~{Mathf.CeilToInt(currentHeat / Mathf.Max(0.01f, rate))} сек до нуля");
        }
    }

    private void SetState(CoreState newState, string reason)
    {
        if (coreState == newState) return;
        coreState = newState;
        LogState($"{newState} ({reason}). Тепло: {currentHeat:F1}/{maxHeat}");
        OnStateChanged?.Invoke(newState);
    }

    private void LogState(string message)
    {
        if (logStateChanges)
            Debug.Log($"[ЯДРО][{coreState}] {message}");
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

            // Если было перегрето, но тепло упало ниже порога — снимаем перегрев
            if (isOverheated && currentHeat <= overheatThreshold)
            {
                currentHeat = Mathf.Max(0, currentHeat);
                isOverheated = false;
                SetState(CoreState.Active, "температура упала ниже порога");
                OnCooled?.Invoke();
                OnHeatChanged?.Invoke(currentHeat);
                return;
            }

            // Проверяем минимальное значение
            if (currentHeat <= 0)
            {
                currentHeat = 0;

                // Если было перегрето — снимаем статус
                if (isOverheated)
                {
                    isOverheated = false;
                    SetState(CoreState.Active, "ядро полностью остыло");
                    OnCooled?.Invoke();
                }
                else
                {
                    LogState($"Полностью остыло. Тепло: 0/{maxHeat}");
                }
            }

            OnHeatChanged?.Invoke(currentHeat);
        }
    }

    public void AddHeat(float amount)
    {
        currentHeat += amount;
        currentHeat = Mathf.Min(maxHeat, currentHeat);

        LogState($"Нагрев +{amount:F1}. Тепло: {currentHeat:F1}/{maxHeat}");

        if (currentHeat >= overheatThreshold && !isOverheated)
        {
            isOverheated = true;
            SetState(CoreState.Overheated, $"превышен порог {overheatThreshold}");
            OnOverheated?.Invoke();
        }
    }

    public void CoolDown(float amount)
    {
        currentHeat -= amount;
        currentHeat = Mathf.Max(0, currentHeat);
        LogState($"Принудительное охлаждение -{amount:F1}. Тепло: {currentHeat:F1}/{maxHeat}");
    }

    public void ResetHeat()
    {
        currentHeat = 0;
        isOverheated = false;
        SetState(CoreState.Active, "тепло сброшено вручную");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = currentHeat > overheatThreshold ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}