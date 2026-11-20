using Interfaces;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(KartEffects))]
public class ShootingController : MonoBehaviour {
    [SerializeField] private LineRenderer aimIndicator;

    [SerializeField] private float minIndicatorLength = 5;
    [SerializeField] private float maxIndicatorLength = 10;
    [SerializeField] private float minShootingForce   = 20;
    [SerializeField] private float maSshootingForce   = 40;
    [SerializeField] private float chargeTime         = 3f;

    private float   chargeStartTime;
    private bool    isCharging;
    private Vector3 lastAimDirection;
    private float   lastShootingForce;
    
    /// <summary>
    /// ShootRequest(direction, force)
    /// </summary>
    public UnityEvent<Vector3, float> shootRequest;
    public UnityEvent shootStarted;

    private IKartInput  input;
    private KartEffects effects;

    private void Start() {
        input   = GetComponent<IKartInput>();
        effects = GetComponent<KartEffects>();
    }

    private void OnDestroy() {
        shootRequest.RemoveAllListeners();
        shootStarted.RemoveAllListeners();
    }

    private void Update() {
        if (!effects.HoldsBall)
        {
            aimIndicator.enabled = false;
            isCharging           = false;
            return;
        }

        Vector2 aim          = input.Aim;
        Vector3 aimDirection = new Vector3(aim.x, 0f, aim.y);

        if (aimDirection.magnitude < 0.1f) {
            aimIndicator.enabled = false;
            if (!isCharging) return;
        }
        else {
            aimDirection.Normalize();
            lastAimDirection = aimDirection;
            aimIndicator.enabled = true;

            if (input.ShootStarted) {
                shootStarted?.Invoke();
                isCharging      = true;
                chargeStartTime = Time.time;
            }

            var charge = isCharging ? Mathf.Clamp01((Time.time - chargeStartTime) / chargeTime) : 0f;

            float indicatorLength = Mathf.Lerp(minIndicatorLength, maxIndicatorLength, charge);
            float shootingForce   = Mathf.Lerp(minShootingForce, maSshootingForce, charge);
            lastShootingForce = shootingForce;
            
            aimIndicator.SetPosition(0, aimIndicator.transform.position);
            aimIndicator.SetPosition(1, aimIndicator.transform.position + aimDirection * indicatorLength);
        }

        if (input.ShootPerformed && isCharging) {
            shootRequest?.Invoke(lastAimDirection, lastShootingForce);
            isCharging = false;
        }
    }
}