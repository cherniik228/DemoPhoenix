using UnityEngine;

public class DebugEnergy : MonoBehaviour
{
    private PlayerCore playerCore;

    private void Start()
    {
        playerCore = GetComponent<PlayerCore>();
    }

    private void Update()
    {
        if (playerCore != null)
        {
            Debug.Log($"[ENERGY] Current: {playerCore.CurrentEnergy}, Max: {playerCore.MaxEnergy}, Percent: {playerCore.EnergyPercent:P0}");
            Debug.Log($"[ENERGY] IsOverheated: {playerCore.IsOverheated}");
        }
    }
}