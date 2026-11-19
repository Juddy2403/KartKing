using UnityEngine;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class Ball : MonoBehaviour {
    [SerializeField] private float enableColliderAfter = 0.5f;

    private Collider  triggerCollider;
    private Rigidbody rigidbody;
    private Kart      holder = null;
    public  Kart      Shooter { get; private set; } = null;
    private Transform socket = null;

    private void Awake() {
        triggerCollider = GetComponent<Collider>();
        rigidbody = GetComponent<Rigidbody>();
    }

    private void EnableCollider() {
        triggerCollider.enabled = true;
    }

    private void Update() {
        if (holder) {
            transform.position = socket.transform.position;
        }
    }

    public void PickUp(GameObject kart) {
        holder  = kart.GetComponent<Kart>();
        Shooter = null;
        socket  = holder.GetBallSocket();

        triggerCollider.enabled = false;
    }

    public void Drop() {
        holder = null;
        Shooter = null;
        socket  = null;
        
        // In order to avoid immediately picking up the ball you shot
        Invoke(nameof(EnableCollider), enableColliderAfter);
    }
    
    public void Shoot(Vector3 direction, float force) {
        rigidbody.AddForce(direction * force, ForceMode.Impulse);
        Shooter = holder;
        holder  = null;
        
        // In order to avoid immediately picking up the ball you shot
        Invoke(nameof(EnableCollider), enableColliderAfter);
    }
}