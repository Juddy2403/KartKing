using UnityEngine;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class Ball : MonoBehaviour {
    [SerializeField] private float enableColliderAfter = 0.5f;

    private Collider   triggerCollider;
    private Rigidbody  rigidbody;
    public  GameObject Holder { get; private set; } = null;
    public  GameObject Shooter { get; private set; } = null;
    private Transform  socket = null;

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
    }

    public void PickUp(GameObject kart, Transform ballSocket) {
        Holder = kart;
        socket = ballSocket;

        triggerCollider.enabled = false;
    }
    
    public void Shoot(Vector3 direction, float force) {
        rigidbody.AddForce(direction * force, ForceMode.Impulse);
        Shooter = Holder;
        Holder  = null;
        
        // In order to avoid immediately picking up the ball you shot
        Invoke(nameof(EnableCollider), enableColliderAfter);
    }
}