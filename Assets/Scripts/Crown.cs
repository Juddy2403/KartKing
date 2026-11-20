using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

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
        if (!Holder) return;
        transform.DOKill();

        transform.DOMove(socket.transform.position, 0.2f)
            .SetEase(Ease.OutQuad);
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