using UnityEngine;
using UnityEngine.InputSystem;

public class TestLimbSwap : MonoBehaviour
{
    [SerializeField] private LimbSwapSystem limbSwapSystem;
    private int currentHeadIndex = 0;
    private int currentArmLIndex = 0;
    private int currentArmRIndex = 0;

    private Keyboard keyboard;

    private void Awake()
    {
        keyboard = Keyboard.current;
    }

    private void Start()
    {
        if (limbSwapSystem == null)
        {
            Debug.LogError("[TestLimbSwap] LimbSwapSystem не назначен!");
        }
        else
        {
            Debug.Log("[TestLimbSwap] Система найдена");
            Debug.Log($"[TestLimbSwap] Доступно голов: {limbSwapSystem.GetAvailableLimbs("Head").Count}");
            Debug.Log($"[TestLimbSwap] Доступно левых рук: {limbSwapSystem.GetAvailableLimbs("ArmL").Count}");
            Debug.Log($"[TestLimbSwap] Доступно правых рук: {limbSwapSystem.GetAvailableLimbs("ArmR").Count}");
        }
    }

    private void Update()
    {
        if (limbSwapSystem == null || keyboard == null) return;

        // Клавиша 1 - заменить голову
        if (keyboard[Key.Digit1].wasPressedThisFrame)
        {
            var heads = limbSwapSystem.GetAvailableLimbs("Head");
            if (heads.Count > 0)
            {
                currentHeadIndex = (currentHeadIndex + 1) % heads.Count;
                Debug.Log($"[TestLimbSwap] Меняем голову на индекс {currentHeadIndex}: {heads[currentHeadIndex].name}");
                limbSwapSystem.SwapLimb("Head", heads[currentHeadIndex]);
            }
            else
            {
                Debug.LogWarning("[TestLimbSwap] Нет доступных голов!");
            }
        }

        // Клавиша 2 - заменить левую руку
        if (keyboard[Key.Digit2].wasPressedThisFrame)
        {
            var arms = limbSwapSystem.GetAvailableLimbs("ArmL");
            if (arms.Count > 0)
            {
                currentArmLIndex = (currentArmLIndex + 1) % arms.Count;
                Debug.Log($"[TestLimbSwap] Меняем левую руку на индекс {currentArmLIndex}: {arms[currentArmLIndex].name}");
                limbSwapSystem.SwapLimb("ArmL", arms[currentArmLIndex]);
            }
            else
            {
                Debug.LogWarning("[TestLimbSwap] Нет доступных левых рук!");
            }
        }

        // Клавиша 3 - заменить правую руку
        if (keyboard[Key.Digit3].wasPressedThisFrame)
        {
            var arms = limbSwapSystem.GetAvailableLimbs("ArmR");
            if (arms.Count > 0)
            {
                currentArmRIndex = (currentArmRIndex + 1) % arms.Count;
                Debug.Log($"[TestLimbSwap] Меняем правую руку на индекс {currentArmRIndex}: {arms[currentArmRIndex].name}");
                limbSwapSystem.SwapLimb("ArmR", arms[currentArmRIndex]);
            }
            else
            {
                Debug.LogWarning("[TestLimbSwap] Нет доступных правых рук!");
            }
        }

        // Клавиша R - сброс
        if (keyboard[Key.R].wasPressedThisFrame)
        {
            Debug.Log("[TestLimbSwap] Сброс всех частей");
        }
    }
}