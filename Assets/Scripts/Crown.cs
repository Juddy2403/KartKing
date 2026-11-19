using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class Crown : MonoBehaviour {
    [SerializeField] private float enableColliderAfter = 0.5f;

    public  UnityEvent<Kart> onPickUp;
    private Collider         triggerCollider;
    public  Kart             Holder  { get; private set; } = null;
    private Transform        socket = null;

    private void Awake() {
        triggerCollider = GetComponent<Collider>();
    }

    private void Update() {
        if (Holder) {
            transform.position = socket.transform.position;
        }
    }

    private void OnDestroy() {
        onPickUp.RemoveAllListeners();
    }
    public void PickUp(Kart kart) {
        Holder = kart.GetComponent<Kart>();
        socket = Holder.GetCrownSocket();
        onPickUp.Invoke(kart);
        
        triggerCollider.enabled = false;
    }
}