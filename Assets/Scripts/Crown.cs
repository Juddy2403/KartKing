using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Crown : MonoBehaviour {
    [SerializeField] private float enableColliderAfter = 0.5f;

    private Collider   triggerCollider;
    public  GameObject Holder  { get; private set; } = null;
    private Transform  socket = null;

    private void Awake() {
        triggerCollider = GetComponent<Collider>();
    }

    private void EnableCollider() {
        triggerCollider.enabled = true;
    }

    private void Update() {
        if (Holder) {
            transform.position = socket.transform.position;
        }
    }

    public void PickUp(GameObject kart) {
        Holder = kart;
        socket = kart.GetComponent<Kart>().GetCrownSocket();

        triggerCollider.enabled = false;
    }
}