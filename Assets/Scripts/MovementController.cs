using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovementController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float acceleration        = 15f;
    [SerializeField] private float backingAcceleration = 5f;
    [SerializeField] private float brakeDeceleration   = 25f;
    [SerializeField] private float maxSpeed            = 20f;
    [SerializeField] private float maxBacking          = 5f;
    [SerializeField] private float drag                = 2f;

    [Header("Steering")]
    [SerializeField] private float steeringSpeed     = 100f;
    [SerializeField] private float minSteeringFactor = 0.2f;

    private Rigidbody     rigidBody;
    private IKartInput    input;
    private MovementState movementState;
    private void Awake() => rigidBody = GetComponent<Rigidbody>();

    // Start (not Awake) so Kart.Initialize has already attached the right input source
    private void Start() => input = GetComponent<IKartInput>();

    private enum MovementState {
        idle,
        accelerate,
        brake,
        backing
    }
    private void FixedUpdate()
    {
        float steering        = input.Steering;
        bool  accelerateValue = input.Accelerate;
        bool  brakeValue      = input.Brake;
        
        Vector3 velocity       = rigidBody.linearVelocity;
        float   forwardSpeed   = Vector3.Dot(velocity, transform.forward);

        movementState = !accelerateValue && !brakeValue ? MovementState.idle :
            brakeValue && forwardSpeed > 0              ? MovementState.brake :
            brakeValue                                  ? MovementState.backing :
                                                          MovementState.accelerate;
        
        UpdateForwardSpeed(ref forwardSpeed);

        // Update velocity while preserving vertical velocity
        Vector3 verticalVelocity = Vector3.Project(velocity, Vector3.up);
        // lateral = Vector3.Lerp(lateral, Vector3.zero, grip * Time.fixedDeltaTime) and lower grip -> on ice, for drift
        rigidBody.linearVelocity = transform.forward * forwardSpeed + verticalVelocity;

        if (Mathf.Abs(forwardSpeed) > 0.1f) {
            Steer(steering, forwardSpeed);
        }
    }

    private void UpdateForwardSpeed(ref float forwardSpeed) {
        float targetSpeed;
        float speedChange;
        
        switch (movementState) {
            case MovementState.idle:
                targetSpeed = 0f;
                speedChange = drag;
                break;
            case MovementState.accelerate:
                targetSpeed = maxSpeed;
                speedChange = acceleration;
                break;
            case MovementState.brake:
                targetSpeed = -maxBacking;
                speedChange = brakeDeceleration;
                break;
            case MovementState.backing:
                targetSpeed = -maxBacking;
                speedChange = backingAcceleration;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        forwardSpeed = Mathf.MoveTowards(
            forwardSpeed,
            targetSpeed,
            speedChange * Time.fixedDeltaTime
        );
    }

    private void Steer(float steering, float forwardSpeed) {
        if (Mathf.Abs(forwardSpeed) < 0.1f) return;   // no turning on the spot

        // Less steering at low speed, full at top speed 
        float speedFactor = Mathf.Max(minSteeringFactor, Mathf.Abs(forwardSpeed) / maxSpeed);
        float direction   = Mathf.Sign(forwardSpeed); // inverted when reversing

        float steeringAmount = steering * steeringSpeed * speedFactor * direction * Time.fixedDeltaTime;
        rigidBody.MoveRotation(rigidBody.rotation * Quaternion.Euler(0f, steeringAmount, 0f));
    }
}
