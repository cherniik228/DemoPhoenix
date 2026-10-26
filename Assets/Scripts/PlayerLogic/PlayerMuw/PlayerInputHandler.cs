using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerInputActions playerInputActions;

    private void Awake()
    {
        playerInputActions = new PlayerInputActions();
    }

    private void OnEnable() => playerInputActions.Player.Enable();
    private void OnDisable() => playerInputActions.Player.Disable();

    // Свойства для чтения ввода другими компонентами
    public Vector2 MoveInput => playerInputActions.Player.Move.ReadValue<Vector2>();
    public bool IsRunning => playerInputActions.Player.Run.IsPressed();

    public bool JumpTriggered => playerInputActions.Player.Jump.triggered;
    public bool DashTriggered => playerInputActions.Player.Dash.triggered;

    public bool AttackTriggered => playerInputActions.Player.Attack.triggered;
    public bool IsChargeAttackPressed => playerInputActions.Player.ChargeAttack.IsPressed();

    public bool LockOnTriggered => playerInputActions.Player.LockOn.triggered;
}