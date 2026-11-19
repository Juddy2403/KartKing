using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Kart))]
public class KartEffects : MonoBehaviour {
    [SerializeField] private float      crownSpeedMultiplier = 1.5f;
    [SerializeField] private float      mudSpeedMultiplier = 0.4f;
    [SerializeField] private float      iceGripMultiplier = 0.4f;
    [Tooltip("Immunity duration right after picking up the crown")]
    [SerializeField] private float      immunityTime = 1.5f;
    [SerializeField] private GameObject shieldObject;
    public                   bool       HoldsBall       { get; private set; } = false;
    public                   bool       HasCrown        { get; private set; } = false;
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
        HasCrown = false;
        SpeedMultiplier = 1f;
    }

    private void OnCapturedCrown() {
        HasCrown        = true;
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
    
    private bool CheckTerrainFor(string areaName) {
        if (!NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas)) return false;
        int area = NavMesh.GetAreaFromName(areaName);
        return (hit.mask & (1 << area)) != 0;

    }
}