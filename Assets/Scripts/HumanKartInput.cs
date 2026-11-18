using UnityEngine;
using UnityEngine.InputSystem;

public class HumanKartInput : MonoBehaviour, IKartInput {
    private PlayerSlot     slot;
    private InputActionMap map;
    private InputAction    steer, accelerate, back, aim, shoot;

    private bool Active => slot != null && slot.IsConnected && map != null;

    public float   Steering   => Active ? steer.ReadValue<Vector2>().x : 0f;
    public bool    Accelerate => Active && accelerate.IsPressed();
    public bool    Brake      => Active && back.IsPressed();
    public Vector2 Aim        => Active ? aim.ReadValue<Vector2>() : Vector2.zero;
    public bool    Shoot      => Active && shoot.IsPressed();

    public void Bind(PlayerSlot playerSlot) {
        slot       = playerSlot;
        map        = slot.Actions.FindActionMap("Kart", true);
        steer      = map.FindAction("Steering", true);
        accelerate = map.FindAction("Accelerate", true);
        back       = map.FindAction("Back", true);
        aim        = map.FindAction("Aiming", true);
        shoot      = map.FindAction("Shoot", true);
        map.Enable();
    }

    private void OnDestroy() => map?.Disable();
}