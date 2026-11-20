using UnityEngine;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class Ball : MonoBehaviour {
    [SerializeField] private float enableColliderAfter = 0.5f;
    [SerializeField] private float timeTillShooterLost = 6f;
    

    private float     curentShooterLostTime;
    
    private Collider  triggerCollider;
    private Rigidbody rigidbody;
    public  Kart      Holder  { get; private set; } = null;
    public  Kart      Shooter { get; private set; } = null;
    public  bool      IsFree  => Holder == null;
    private Transform socket = null;

    private void Awake() {
        triggerCollider = GetComponent<Collider>();
        rigidbody = GetComponent<Rigidbody>();
    }

    private void EnableCollider() {
        triggerCollider.enabled = true;
    }

    private void Update() {
        if (Holder) {
            transform.position = socket.transform.position;
        }

        if (!Shooter) return;
        curentShooterLostTime -= Time.deltaTime;
        if (curentShooterLostTime <= 0f) Shooter = null;
    }

    public void PickUp(GameObject kart) {
        Holder  = kart.GetComponent<Kart>();
        Shooter = null;
        socket  = Holder.GetBallSocket();

        triggerCollider.enabled = false;
    }

    public void Drop() {
        Holder = null;
        Shooter = null;
        socket  = null;
        
        // In order to avoid immediately picking up the ball you shot
        Invoke(nameof(EnableCollider), enableColliderAfter);
    }
    
    public void Shoot(Vector3 direction, float force) {
        rigidbody.AddForce(direction * force, ForceMode.Impulse);
        Shooter = Holder;
        curentShooterLostTime = timeTillShooterLost;
        Holder  = null;
        
        // In order to avoid immediately picking up the ball you shot
        Invoke(nameof(EnableCollider), enableColliderAfter);
    }
}