using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Kart))]
public class KartEffects : MonoBehaviour {
    [Header("Effects")]
    
    [SerializeField] private float      crownSpeedMultiplier = 1.5f;
    [SerializeField] private float      mudSpeedMultiplier = 0.4f;
    [SerializeField] private float      iceGripMultiplier = 0.4f;
    [Tooltip("Immunity duration right after picking up the crown")]
    [SerializeField] private float      immunityTime = 1.5f;
    [SerializeField] private GameObject shieldObject;

    [Header("Controller vibrations")] 
    
    [SerializeField] [Range(0f, 1f)] private float chargingLowFrequency = 0.1f;
    [SerializeField] [Range(0f, 1f)] private float chargingHighFrequency = 0.2f;
    
    [Space]
    
    [SerializeField] [Range(0f, 1f)] private float crownLowFrequency  = 0.3f;
    [SerializeField] [Range(0f, 1f)] private float crownHighFrequency     = 0.5f;
    [SerializeField]                 private float crownVibrationDuration = 1f;
    
    public                                   bool  HoldsBall       { get; private set; } = false;
    public                                   bool  HasCrown        { get; private set; } = false;
    public                                   bool  HasImmunity     { get; private set; } = false;
    public                                   float SpeedMultiplier { get; set; }         = 1f;
    public                                   float GripMultiplier  { get; set; }         = 1f;

    private float immunityElapsedTime;
    private Kart  kart;

    private void Start() {
        kart = GetComponent<Kart>();
        kart.onBallPickedUp.AddListener(OnBallPickedUp);
        kart.onBallShot.AddListener(OnBallShot);
        kart.onCapturedCrown.AddListener(OnCapturedCrown);
        kart.onLostCrown.AddListener(OnLostCrown);
        kart.onShootStarted.AddListener(OnShootStarted);
    }

    private void OnShootStarted() {
        VibrateController(chargingLowFrequency, chargingHighFrequency);
    }

    private void VibrateController(float minFreq, float maxFreq, float? duration = null) {
        if (kart.Slot.Device is not Gamepad gamepad) return;
        gamepad.SetMotorSpeeds(minFreq, maxFreq);

        if (!duration.HasValue) return;
        Invoke(nameof(ResetControllerVibration), duration.Value);
    }
    
    private void ResetControllerVibration() {
        if (kart.Slot.Device is not Gamepad gamepad) return;
        gamepad.ResetHaptics();
    }

    private void Update() {
        GripMultiplier = CheckTerrainFor("Ice") ? iceGripMultiplier : 1f;
        
        if (CheckTerrainFor("Mud")) {
            SpeedMultiplier = HasCrown ? crownSpeedMultiplier * mudSpeedMultiplier : mudSpeedMultiplier;
        } else SpeedMultiplier = HasCrown ? crownSpeedMultiplier : 1f;
            
        HandleImmunity();
    }

    private void HandleImmunity() {
        if (immunityElapsedTime <= 0f) return;
        immunityElapsedTime -=  Time.deltaTime;
        
        if (immunityElapsedTime > 0f) return;
        shieldObject.SetActive(false);
        HasImmunity = false;
    }

    private void OnLostCrown() {
        VibrateController(crownLowFrequency, crownHighFrequency, crownVibrationDuration);
        HasCrown = false;
        SpeedMultiplier = 1f;
    }

    private void OnCapturedCrown() {
        VibrateController(crownLowFrequency, crownHighFrequency, crownVibrationDuration);
        HasCrown        = true;
        SpeedMultiplier = crownSpeedMultiplier;
        HasImmunity     = true;
        shieldObject.SetActive(true);
        immunityElapsedTime =  immunityTime;
    }

    private void OnBallShot() {
        HoldsBall = false;
        if (kart.Slot.Device is not Gamepad gamepad) return;
        gamepad.ResetHaptics();
    }

    private void OnBallPickedUp() {
        HoldsBall = true;
    }
    
    private bool CheckTerrainFor(string areaName) {
        if (!NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas)) return false;
        int area = NavMesh.GetAreaFromName(areaName);
        return (hit.mask & (1 << area)) != 0;
    }

    private void OnDisable() {
        ResetControllerVibration();
    }
}