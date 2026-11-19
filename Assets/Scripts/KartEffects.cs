using UnityEngine;

[RequireComponent(typeof(Kart))]
public class KartEffects : MonoBehaviour {
    [SerializeField] private float crownSpeedMultiplier = 1.5f;

    public bool  HoldsBall       { get; private set; } = false;
    public float SpeedMultiplier { get; set; }         = 1f;
    public float GripMultiplier  { get; set; }         = 1f;

    private void Start() {
        var kart = GetComponent<Kart>();
        kart.onBallPickedUp.AddListener(OnBallPickedUp);
        kart.onBallShot.AddListener(OnBallShot);
        kart.onCapturedCrown.AddListener(OnCapturedCrown);
        kart.onLostCrown.AddListener(OnLostCrown);
    }

    private void OnLostCrown() {
        SpeedMultiplier = 1f;
    }

    private void OnCapturedCrown() {
        SpeedMultiplier = crownSpeedMultiplier;
    }

    private void OnBallShot() {
        HoldsBall = false;
    }

    private void OnBallPickedUp() {
        HoldsBall = true;
    }
}