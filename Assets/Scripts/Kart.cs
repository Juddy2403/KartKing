using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(KartEffects))]
public class Kart : MonoBehaviour {
    [SerializeField] private Transform ballSocket;
    [SerializeField] private GameObject crownMesh;

    public UnityEvent onCapturedCrown;
    public UnityEvent onLostCrown;

    public UnityEvent onBallPickedUp;
    public UnityEvent onBallShot;
    public        PlayerSlot             Slot { get; private set; }
    private       Ball                   ball;


    public void Initialize(PlayerSlot slot) {
        Slot = slot;

        foreach (Renderer renderer in GetComponentsInChildren<Renderer>()) {
            renderer.material.color = slot.Color;
        }

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
        GetComponent<ShootingController>().shootRequest.AddListener(OnShootRequest);
    }

    private void OnShootRequest(Vector3 direction, float force) {
        if (ball.Holder == gameObject) {
            ball.Shoot(direction, force);
            onBallShot?.Invoke();
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("Ball")) {
            // if (Slot.HasCrown) {
            //     onLostCrown?.Invoke();
            //     crownMesh.SetActive(false);
            // }
            // else {
                ball.PickUp(gameObject, ballSocket);
                onBallPickedUp?.Invoke();
            //}
        }
        else if (other.gameObject.CompareTag("Crown")) {
            onCapturedCrown?.Invoke();
            crownMesh.SetActive(true);
        }
    }
}