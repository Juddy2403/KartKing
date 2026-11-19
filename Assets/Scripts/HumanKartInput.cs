using System.Collections;
using Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;

public class HumanKartInput : MonoBehaviour, IKartInput {
    private PlayerSlot     slot;
    private InputActionMap map;
    private InputAction    steer, aim, shoot;

    private bool Active => slot != null && slot.IsConnected && map != null;
    public Vector2 Steering       => Active ? steer.ReadValue<Vector2>() : Vector2.zero;
    public Vector2 Aim            => Active ? aim.ReadValue<Vector2>() : Vector2.zero;
    public bool    ShootStarted   { get; private set; }
    public bool    ShootPerformed { get; private set; }

    public void Bind(PlayerSlot playerSlot) {
        slot       = playerSlot;
        map        = slot.Actions.FindActionMap("Kart", true);
        steer      = map.FindAction("Steering", true);
        aim        = map.FindAction("Aiming", true);
        shoot      = map.FindAction("Shoot", true);
        map.Enable();

        shoot.started   += OnShootStarted;
        shoot.canceled  += OnShootPerformed;
    }

    private void OnShootStarted(InputAction.CallbackContext obj) {
        ShootStarted = true;
        StartCoroutine(ResetShootState());
    }

    private void OnShootPerformed(InputAction.CallbackContext obj) {
        ShootPerformed = true;
        StartCoroutine(ResetShootState());
    }
    
    private IEnumerator ResetShootState()
    {
        yield return null;
        ShootPerformed =  false;
        ShootStarted  = false;
    }

    private void OnDestroy() {
        shoot.started   -= OnShootStarted;
        shoot.canceled  -= OnShootPerformed;
        map?.Disable();
    } 
}