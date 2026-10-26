using UnityEngine;

[CreateAssetMenu(fileName = "CloneAbility", menuName = "Core Abilities/Ultimate/Clone")]
public class CloneAbility : CoreAbilityBase
{
    [Header("Клон")]
    [SerializeField] private GameObject clonePrefab;
    [SerializeField] private float cloneDuration = 30f;
    [SerializeField] private float cloneDamageMultiplier = 0.5f; // Урон клона / 2

    public CloneAbility()
    {
        abilityName = "Клонирование";
        abilityType = AbilityType.Ultimate;
        energyCost = 80;
        heatGenerated = 80f; // Очень высокий нагрев
    }

    protected override void Execute(PlayerCore core, PlayerStateManager stateManager, PlayerMovement movement)
    {
        if (clonePrefab == null)
        {
            Debug.LogError("[КЛОН] Префаб клона не назначен!");
            return;
        }

        // Создаем клона
        GameObject clone = Instantiate(clonePrefab, core.transform.position + Vector3.right * 2, core.transform.rotation);

        CloneController cloneController = clone.GetComponent<CloneController>();
        if (cloneController != null)
        {
            cloneController.Setup(core, stateManager, cloneDuration, cloneDamageMultiplier);
        }

        // Уничтожаем клона через duration
        Destroy(clone, cloneDuration);

        Debug.Log($"[УЛЬТИМЕЙТ] Клон создан на {cloneDuration} сек!");
    }
}