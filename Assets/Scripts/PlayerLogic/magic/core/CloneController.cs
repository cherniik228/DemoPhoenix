using UnityEngine;

public class CloneController : MonoBehaviour
{
    private PlayerCore ownerCore;
    private PlayerStateManager ownerState;
    private float duration;
    private float damageMultiplier;
    private float timer;

    public void Setup(PlayerCore core, PlayerStateManager state, float dur, float dmgMult)
    {
        ownerCore = core;
        ownerState = state;
        duration = dur;
        damageMultiplier = dmgMult;
        timer = 0f;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        // Клон следует за игроком
        if (ownerCore != null)
        {
            Vector3 targetPos = ownerCore.transform.position + Vector3.right * 3;
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 2f);
            transform.rotation = ownerCore.transform.rotation;
        }

        // Автоуничтожение
        if (timer >= duration)
        {
            Destroy(gameObject);
        }
    }

    public float GetDamageMultiplier() => damageMultiplier;
}