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
    public  PlayerSlot  Slot { get; private set; }
    private KartEffects kartEffects;
    private Ball        heldBall = null;
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

        if (slot.IsBot) {
            // gameObject.AddComponent<BotKartInput>();   // your bot implements IKartInput
            Destroy(GetComponent<HumanKartInput>());
        }
        else {
            GetComponent<HumanKartInput>().Bind(slot);
        }
    }

    private void OnDestroy() {
        onBallShot.RemoveAllListeners();
        onBallPickedUp.RemoveAllListeners();
        onCapturedCrown.RemoveAllListeners();
        onLostCrown.RemoveAllListeners();
    }

    private void Start() {
        crown = FindFirstObjectByType<Crown>();
        if (!crown) Debug.LogError("No crown found in scene");

        kartEffects = GetComponent<KartEffects>();
        
        GetComponent<ShootingController>().shootRequest.AddListener(OnShootRequest);
    }

    private void OnShootRequest(Vector3 direction, float force) {
        if (!heldBall) return;
        ShootBall(direction, force);
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("Ball")) {
            // Can only pick up one ball at a time
            if (heldBall) return;
            
            var ball = other.gameObject.GetComponent<Ball>();
            if (crown.Holder == this) {
                // If no one shot the ball, keep the crown
                if (!ball.Shooter) return;
                if(kartEffects.HasImmunity) return;

                // Handing the crown to the attacker
                crown.PickUp(ball.Shooter);
                onLostCrown?.Invoke();
                
                PickUpBall(ball);
            }
            else {
                PickUpBall(ball);
            }
        }
        else if (other.gameObject.CompareTag("Crown")) {
            if (heldBall) {
                DropBall();
            }

            crown.PickUp(this);
            onCapturedCrown?.Invoke();
        }
    }

    private void DropBall() {
        heldBall.Drop();
        heldBall = null;
        onBallShot?.Invoke();
    }

    private void ShootBall(Vector3 direction, float force) {
        heldBall.Shoot(direction, force);
        heldBall = null;
        onBallShot?.Invoke();
    }
    
    private void PickUpBall(Ball ball) {
        ball.PickUp(gameObject);
        heldBall = ball;
        onBallPickedUp?.Invoke();
    }
}