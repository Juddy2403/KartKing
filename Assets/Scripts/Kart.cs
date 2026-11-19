using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(KartEffects))]
public class Kart : MonoBehaviour {
    [SerializeField] private Transform ballSocket;
    [SerializeField] private Transform crownSocket;

    public Transform GetCrownSocket() => crownSocket;
    public Transform GetBallSocket() => ballSocket;
    
    
    public UnityEvent onCapturedCrown;
    public UnityEvent onLostCrown;

    public  UnityEvent onBallPickedUp;
    public  UnityEvent onBallShot;
    public  PlayerSlot Slot { get; private set; }
    private Ball       ball;
    private Crown      crown;

    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private int      materialIndex = 1;

    private void SetColor(Color color)
    {
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
        ball = FindFirstObjectByType<Ball>();
        if (!ball) Debug.LogError("No ball found in scene");

        crown = FindFirstObjectByType<Crown>();
        if (!crown) Debug.LogError("No crown found in scene");

        GetComponent<ShootingController>().shootRequest.AddListener(OnShootRequest);
    }

    private void OnShootRequest(Vector3 direction, float force) {
        if (ball.Holder == this) {
            ball.Shoot(direction, force);
            onBallShot?.Invoke();
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("Ball")) {
            if (crown.Holder == this) {
                // If no one shot the ball, keep the crown
                if (!ball.Shooter) return;
                
                crown.PickUp(ball.Shooter);
                onLostCrown?.Invoke();
                ball.PickUp(gameObject);
                onBallPickedUp?.Invoke();
            }
            else {
                ball.PickUp(gameObject);
                onBallPickedUp?.Invoke();
            }
        }
        else if (other.gameObject.CompareTag("Crown")) {
            if (ball.Holder == this) {
                ball.Drop();
                onBallShot?.Invoke();
            }
            crown.PickUp(this);
            onCapturedCrown?.Invoke();
        }
    }
}