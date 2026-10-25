using UnityEngine;
using System.Collections.Generic;

public class LimbSwapSystem : MonoBehaviour
{
    [Header("Текущие части тела")]
    [SerializeField] private GameObject currentHead;
    [SerializeField] private GameObject currentTorso;
    [SerializeField] private GameObject currentArmL;
    [SerializeField] private GameObject currentArmR;
    [SerializeField] private GameObject currentLegL;
    [SerializeField] private GameObject currentLegR;

    [Header("Доступные префабы для замены")]
    [SerializeField] private List<GameObject> availableHeads = new List<GameObject>();
    [SerializeField] private List<GameObject> availableTorsos = new List<GameObject>();
    [SerializeField] private List<GameObject> availableArmsL = new List<GameObject>();
    [SerializeField] private List<GameObject> availableArmsR = new List<GameObject>();
    [SerializeField] private List<GameObject> availableLegsL = new List<GameObject>();
    [SerializeField] private List<GameObject> availableLegsR = new List<GameObject>();

    [Header("Кости-якоря на скелете")]
    [SerializeField] private Transform boneHead;
    [SerializeField] private Transform boneShoulderL;
    [SerializeField] private Transform boneShoulderR;
    [SerializeField] private Transform boneHipL;
    [SerializeField] private Transform boneHipR;

    private Dictionary<string, GameObject> currentLimbs = new Dictionary<string, GameObject>();
    private Dictionary<string, Transform> anchors = new Dictionary<string, Transform>();

    private void Start()
    {
        currentLimbs["Head"] = currentHead;
        currentLimbs["Torso"] = currentTorso;
        currentLimbs["ArmL"] = currentArmL;
        currentLimbs["ArmR"] = currentArmR;
        currentLimbs["LegL"] = currentLegL;
        currentLimbs["LegR"] = currentLegR;

        anchors["Head"] = boneHead;
        anchors["ArmL"] = boneShoulderL;
        anchors["ArmR"] = boneShoulderR;
        anchors["LegL"] = boneHipL;
        anchors["LegR"] = boneHipR;

        Debug.Log("[LimbSwap] Система инициализирована");
    }

    public void SwapLimb(string limbType, GameObject newLimbPrefab)
    {
        if (!currentLimbs.ContainsKey(limbType))
        {
            Debug.LogError($"[LimbSwap] Неизвестный тип: {limbType}");
            return;
        }

        if (newLimbPrefab == null)
        {
            Debug.LogError($"[LimbSwap] Префаб не назначен для {limbType}!");
            return;
        }

        Debug.Log($"[LimbSwap] Меняем {limbType} на {newLimbPrefab.name}");

        // Получаем якорь ДО удаления старой части
        Transform anchor = anchors.ContainsKey(limbType) ? anchors[limbType] : null;
        if (anchor == null)
        {
            Debug.LogError($"[LimbSwap] Якорь не найден для {limbType}!");
            return;
        }

        // Сохраняем позицию и ротацию якоря
        Vector3 anchorPosition = anchor.position;
        Quaternion anchorRotation = anchor.rotation;

        // Удаляем старую часть
        if (currentLimbs[limbType] != null)
        {
            Destroy(currentLimbs[limbType].gameObject);
        }

        // Создаём новую
        GameObject newLimb = Instantiate(newLimbPrefab, anchorPosition, anchorRotation);
        newLimb.name = limbType + "_Instance";

        // Делаем дочерним объектом торса
        newLimb.transform.SetParent(currentTorso.transform);

        // ВАЖНО: Сбрасываем локальную позицию и ротацию
        newLimb.transform.localPosition = Vector3.zero;
        newLimb.transform.localRotation = Quaternion.identity;

        currentLimbs[limbType] = newLimb;

        Debug.Log($"[LimbSwap] {limbType} заменена! Позиция якоря: {anchorPosition}");
    }

    public GameObject GetCurrentLimb(string limbType)
    {
        if (currentLimbs.ContainsKey(limbType))
            return currentLimbs[limbType];
        return null;
    }

    public List<GameObject> GetAvailableLimbs(string limbType)
    {
        switch (limbType)
        {
            case "Head": return availableHeads;
            case "Torso": return availableTorsos;
            case "ArmL": return availableArmsL;
            case "ArmR": return availableArmsR;
            case "LegL": return availableLegsL;
            case "LegR": return availableLegsR;
            default: return new List<GameObject>();
        }
    }
}