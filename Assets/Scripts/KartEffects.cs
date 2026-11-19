using UnityEngine;

[RequireComponent(typeof(Kart))]
public class KartEffects : MonoBehaviour {
    public bool HoldsBall { get; private set; } = false;

    private void Start() {
        var kart = GetComponent<Kart>();
        kart.onBallPickedUp.AddListener(OnBallPickedUp);
        kart.onBallShot.AddListener(OnBallShot);
    }

    private void OnBallShot() {
        HoldsBall = false;
    }

    private void OnBallPickedUp() {
        HoldsBall = true;
    }
}