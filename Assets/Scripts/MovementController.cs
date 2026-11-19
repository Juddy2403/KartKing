using Interfaces;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(KartEffects))]
public class MovementController : MonoBehaviour {
    [Header("Movement")] [SerializeField] private float acceleration      = 15f;
    [SerializeField]                      private float brakeDeceleration = 25f;
    [SerializeField]                      private float maxSpeed          = 20f;
    [SerializeField]                      private float drag              = 2f;
    [SerializeField, Range(0f, 0.5f)]     private float deadzone          = 0.1f;

    [Header("Steering")] [SerializeField] private float steeringSpeed = 100f;

    [Header("Drift")] [Tooltip("How fast sideways sliding dies out. Lower = more drift.")] [SerializeField]
    private float grip = 5f;

    private Rigidbody   rigidBody;
    private IKartInput  input;
    private KartEffects effects;

    private void Awake() {
        rigidBody = GetComponent<Rigidbody>();
        effects = GetComponent<KartEffects>();
    }

    // Start (not Awake) so Kart.Initialize has already attached the right input source
    private void Start() => input = GetComponent<IKartInput>();

    private void FixedUpdate() {
        Vector2 move       = input.Steering;
        float   magnitude  = Mathf.Min(move.magnitude, 1f);
        bool    hasInput   = magnitude > deadzone;
        Vector3 desiredDir = hasInput ? new Vector3(move.x, 0f, move.y).normalized : Vector3.zero;

        Quaternion rotation = Steer(desiredDir, hasInput);

        Vector3 velocity = rigidBody.linearVelocity;
        Vector3 vertical = Vector3.Project(velocity, Vector3.up);
        Vector3 planar   = velocity - vertical;

        Vector3 forward = rotation * Vector3.forward;
        Vector3 right   = rotation * Vector3.right;

        float forwardSpeed = Vector3.Dot(planar, forward);
        float lateralSpeed = Vector3.Dot(planar, right);

        forwardSpeed =  UpdateForwardSpeed(forwardSpeed, forward, desiredDir, magnitude, hasInput);
        lateralSpeed *= Mathf.Exp(-grip * effects.GripMultiplier * Time.fixedDeltaTime);

        rigidBody.linearVelocity = forward * forwardSpeed + right * lateralSpeed + vertical;
    }

    private Quaternion Steer(Vector3 desiredDir, bool hasInput) {
        float yaw = rigidBody.rotation.eulerAngles.y;

        if (hasInput) {
            float targetYaw = Mathf.Atan2(desiredDir.x, desiredDir.z) * Mathf.Rad2Deg;
            yaw = Mathf.MoveTowardsAngle(yaw, targetYaw, steeringSpeed * Time.fixedDeltaTime);
        }

        Quaternion rotation = Quaternion.Euler(0f, yaw, 0f);
        rigidBody.MoveRotation(rotation);
        return rotation;
    }

    private float UpdateForwardSpeed(
        float   forwardSpeed,
        Vector3 forward,
        Vector3 desiredDir,
        float   magnitude,
        bool    hasInput) {
        
        float targetSpeed = 0f;
        float speedChange = drag;

        if (hasInput) {
            float alignment = Mathf.Clamp01(Vector3.Dot(forward, desiredDir));
            targetSpeed = maxSpeed * effects.SpeedMultiplier * magnitude * alignment;
            speedChange = targetSpeed > forwardSpeed ? acceleration : brakeDeceleration;
        }

        return Mathf.MoveTowards(forwardSpeed, targetSpeed, speedChange * Time.fixedDeltaTime);
    }
}