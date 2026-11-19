using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Kart))]
public class KartEffects : MonoBehaviour {
    [SerializeField] private float      crownSpeedMultiplier = 1.5f;
    [SerializeField] private float      iceGripMultiplier = 0.4f;
    [Tooltip("Immunity duration right after picking up the crown")]
    [SerializeField] private float      immunityTime = 1.5f;
    [SerializeField] private GameObject shieldObject;
    public                   bool       HoldsBall       { get; private set; } = false;
    public                   bool       HasImmunity     { get; private set; } = false;
    public                   float      SpeedMultiplier { get; set; }         = 1f;
    public                   float      GripMultiplier  { get; set; }         = 1f;

    private float immunityElapsedTime;

    private void Start() {
        var kart = GetComponent<Kart>();
        kart.onBallPickedUp.AddListener(OnBallPickedUp);
        kart.onBallShot.AddListener(OnBallShot);
        kart.onCapturedCrown.AddListener(OnCapturedCrown);
        kart.onLostCrown.AddListener(OnLostCrown);
    }

    private void Update() {
        GripMultiplier = IsOnIce() ? iceGripMultiplier : 1f;
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
        SpeedMultiplier = 1f;
    }

    private void OnCapturedCrown() {
        SpeedMultiplier = crownSpeedMultiplier;
        HasImmunity     = true;
        shieldObject.SetActive(true);
        immunityElapsedTime =  immunityTime;
    }

    private void OnBallShot() {
        HoldsBall = false;
    }

    private void OnBallPickedUp() {
        HoldsBall = true;
    }
    
    private bool IsOnIce() {
        return !NavMesh.SamplePosition(transform.position, 
            out _, 1f, NavMesh.GetAreaFromName("Ice"));
    }
}