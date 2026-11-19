using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(IKartInput), typeof(KartEffects))]
public class ShootingController : MonoBehaviour {
    [SerializeField] private LineRenderer aimIndicator;
    [SerializeField] private float        indicatorLength;
    [SerializeField] private float        shootingForce;

    /// <summary>
    /// ShootRequest(direction, force)
    /// </summary>
    public UnityEvent<Vector3, float> shootRequest;

    private IKartInput input;
    private KartEffects effects;

    private void Start() {
        input = GetComponent<IKartInput>();
        effects = GetComponent<KartEffects>();
    }

    private void OnDestroy() {
        shootRequest.RemoveAllListeners();
    }

    private void Update() {
        if (!effects.HoldsBall) {
            aimIndicator.enabled = false;
            return;
        }
        
        Vector2 aim        = input.Aim;
        bool    isShooting = input.Shoot;

        Vector3 aimDirection = new Vector3(aim.x, 0f, aim.y);

        if (Vector3.Magnitude(aimDirection) < 0.1f) {
            aimIndicator.enabled = false;
        }
        else {
            aimIndicator.enabled = true;
            aimIndicator.SetPosition(0, aimIndicator.transform.position);
            aimIndicator.SetPosition(1, aimIndicator.transform.position + aimDirection * indicatorLength);

            if (isShooting) shootRequest?.Invoke(aimDirection, shootingForce);
        }
    }
}