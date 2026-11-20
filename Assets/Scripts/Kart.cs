using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(KartEffects))]
public class Kart : MonoBehaviour {
    [SerializeField] private Transform ballSocket;
    [SerializeField] private Transform crownSocket;

    public Transform GetCrownSocket() => crownSocket;
    public Transform GetBallSocket()  => ballSocket;


    public UnityEvent onCapturedCrown;
    public UnityEvent onLostCrown;

    public  UnityEvent  onBallPickedUp;
    public  UnityEvent  onBallShot;
    public  UnityEvent  onShootStarted;
    public  PlayerSlot  Slot { get; private set; }
    private KartEffects kartEffects;
    public  Ball        HeldBall { get; private set; } = null;
    private Crown       crown;

    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private int      materialIndex = 1;

    private void SetColor(Color color) {
        Material[] materials = targetRenderer.materials;
        materials[materialIndex].color = color;
    }

    public void Initialize(PlayerSlot slot) {
        Slot = slot;

        SetColor(Slot.Color);

        if (slot.IsBot) gameObject.AddComponent<BotKartInput>();
        else gameObject.AddComponent<HumanKartInput>().Bind(slot);
    }

    private void OnDestroy() {
        onBallShot.RemoveAllListeners();
        onBallPickedUp.RemoveAllListeners();
        onShootStarted.RemoveAllListeners();
        onCapturedCrown.RemoveAllListeners();
        onLostCrown.RemoveAllListeners();
    }

    private void Start() {
        crown = FindFirstObjectByType<Crown>();
        if (!crown) Debug.LogError("No crown found in scene");

        kartEffects = GetComponent<KartEffects>();

        var shootingController = GetComponent<ShootingController>();
        shootingController.shootRequest.AddListener(OnShootRequest);
        shootingController.shootStarted.AddListener(OnShootStarted);
    }

    private void OnShootStarted() {
        onShootStarted?.Invoke();
    }


    private void OnShootRequest(Vector3 direction, float force) {
        if (!HeldBall) return;
        ShootBall(direction, force);
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("Ball")) {
            // Can only pick up one ball at a time
            if (HeldBall) return;

            var ball = other.gameObject.GetComponent<Ball>();
            if (crown.Holder == this) {
                // If no one shot the ball, keep the crown
                if (!ball.Shooter) return;
                if (kartEffects.HasImmunity) return;

                // Handing the crown to the attacker
                ball.Shooter.CaptureCrown();
                
                onLostCrown?.Invoke();
                PickUpBall(ball);
            }
            else {
                PickUpBall(ball);
            }
        }
        else if (other.gameObject.CompareTag("Crown")) {
            CaptureCrown();
        }
    }

    public void CaptureCrown() {
        if (HeldBall) DropBall();
        crown.PickUp(this);
        onCapturedCrown?.Invoke();
    }
    public void DropBall() {
        HeldBall.Drop();
        HeldBall = null;
        onBallShot?.Invoke();
    }

    private void ShootBall(Vector3 direction, float force) {
        HeldBall.Shoot(direction, force);
        HeldBall = null;
        onBallShot?.Invoke();
    }

    private void PickUpBall(Ball ball) {
        ball.PickUp(gameObject);
        HeldBall = ball;
        onBallPickedUp?.Invoke();
    }
}