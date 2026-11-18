using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Ball : MonoBehaviour {
    [SerializeField] private float enableColliderAfter = 0.5f;

    private Collider triggerCollider;

    private void OnEnable() {
        triggerCollider         = GetComponent<Collider>();
        triggerCollider.enabled = false;
        Invoke(nameof(EnableCollider), enableColliderAfter);
    }

    private void EnableCollider() {
        triggerCollider.enabled = true;
    }
}