using Interfaces;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Simple fallible bot driver. It produces the same IKartInput values a human would:
///  - has the crown:        flee from the nearest threat (karts holding a ball count double)
///  - has a ball:           chase the crown holder, aim, charge and shoot
///  - has neither:          go for a free ball
/// It is deliberately imperfect: delayed reactions, wobbly aim, slow aim turning,
/// random distractions and the occasional non-optimal ball choice. 'skill' scales all of it.
/// Assumes: Kart.HasCrown (bool), Kart.HeldBall (Ball or null), Ball.IsFree (bool).
/// </summary>
[DefaultExecutionOrder(-100)] // update before ShootingController so one-frame pulses are seen
[RequireComponent(typeof(Kart))]
public class BotKartInput : MonoBehaviour, IKartInput {
    [Header("Skill (0 = clumsy, 1 = sharp)")] [SerializeField, Range(0f, 1f)]
    private float skill = 0.5f;

    [Header("Decision making")] [SerializeField]
    private float slowThinkInterval = 0.6f; // seconds between decisions at skill 0

    [SerializeField]                private float   fastThinkInterval   = 0.15f; // ... at skill 1
    [SerializeField, Range(0f, 1f)] private float   distractionChance   = 0.12f; // per decision at skill 0
    [SerializeField]                private Vector2 distractionDuration = new Vector2(1f, 2.5f);

    [Header("Movement")] [SerializeField] private float fleeDistance  = 12f;
    [SerializeField]                      private float wanderRadius  = 12f;
    [SerializeField]                      private float waypointReach = 1.5f;

    [Header("Aim and shooting")] [SerializeField]
    private float shootRange = 14f;

    [SerializeField] private float   aimTurnSpeed = 300f; // degrees per second at full skill
    [SerializeField] private float   maxAimError  = 20f; // degrees, at skill 0
    [SerializeField] private float   minAimError  = 2f; // degrees, at skill 1
    [SerializeField] private Vector2 chargeTime   = new Vector2(0.2f, 0.8f); // near / far target

    // ---- IKartInput ----
    public Vector2 Steering       { get; private set; }
    public Vector2 Aim            { get; private set; }
    public bool    ShootStarted   { get; private set; }
    public bool    ShootPerformed { get; private set; }

    private Kart        kart;
    private KartEffects kartEffects;
    private Kart[]      karts = System.Array.Empty<Kart>();
    private Ball[]      balls = System.Array.Empty<Ball>();

    // movement intent
    private bool    hasMoveTarget;
    private Vector3 moveTarget;
    private float   stopDistance;
    private Kart    aimTarget;

    // path following
    private          NavMeshPath path;
    private readonly Vector3[]   cornerBuffer = new Vector3[32];
    private          int         cornerCount;

    // personality / state
    private float   maxStick;
    private float   nextThinkTime;
    private float   aimError;
    private Vector3 aimDir;

    private bool    hasWander;
    private Vector3 wanderTarget;
    private float   distractedUntil;
    private Vector3 distractionTarget;

    private bool  hadBall;
    private bool  charging;
    private float nextShotTime;
    private float releaseTime;

    private void Awake() {
        kart        = GetComponent<Kart>();
        kartEffects = GetComponent<KartEffects>();
        path        = new NavMeshPath();

        maxStick      = Mathf.Lerp(0.65f, 1f, skill) * Random.Range(0.92f, 1f); // dumb bots don't floor it
        aimDir        = Flat(transform.forward).normalized;
        nextThinkTime = Time.time + Random.value * 0.5f; // desync the bots
    }

    private void Update() {
        if (Time.time >= nextThinkTime) Think();

        UpdateSteering();
        UpdateAim();
        UpdateShooting();
    }

    // Shoot flags are one-frame pulses
    private void LateUpdate() {
        ShootStarted   = false;
        ShootPerformed = false;
    }

    // ---------- Decision making (runs a few times per second) ----------

    private void Think() {
        nextThinkTime = Time.time + Mathf.Lerp(slowThinkInterval, fastThinkInterval, skill) * Random.Range(0.8f, 1.2f);

        karts = FindObjectsByType<Kart>(FindObjectsSortMode.None);
        balls = FindObjectsByType<Ball>(FindObjectsSortMode.None);

        hasMoveTarget = false;
        aimTarget     = null;
        stopDistance  = 0f;

        if (kartEffects.HasCrown) Flee();
        else if (kart.HeldBall) Hunt();
        else FetchBall();

        ApplyDistraction();

        if (hasMoveTarget) RecalculatePath();
    }

    private void Flee() {
        Kart threat = NearestThreat();
        if (threat == null) {
            Wander();
            return;
        }

        Vector3 away                        = Flat(transform.position - threat.transform.position);
        if (away.sqrMagnitude < 0.01f) away = Random.insideUnitSphere;
        away = Flat(away).normalized;

        // Not a straight line away: sidestep so the bot doesn't just run into a corner
        away = Quaternion.Euler(0f, Random.Range(-50f, 50f), 0f) * away;

        moveTarget    = transform.position + away * fleeDistance;
        hasMoveTarget = true;
    }

    private void Hunt() {
        Kart holder = CrownHolder();
        if (holder == null) {
            Wander();
            return;
        }

        moveTarget    = holder.transform.position;
        hasMoveTarget = true;
        stopDistance  = shootRange * 0.6f;
        aimTarget     = holder;

        // New aim wobble every decision: the lower the skill, the bigger the miss
        aimError = Random.Range(-1f, 1f) * Mathf.Lerp(maxAimError, minAimError, skill);
    }

    private void FetchBall() {
        Ball  nearest   = null, random = null;
        float best      = float.MaxValue;
        int   freeCount = 0;

        foreach (Ball ball in balls) {
            if (!ball.IsFree) continue;

            float d = Flat(ball.transform.position - transform.position).sqrMagnitude;
            if (d < best) {
                best    = d;
                nearest = ball;
            }

            freeCount++;
            if (Random.Range(0, freeCount) == 0) random = ball; // reservoir sampling
        }

        if (nearest == null) {
            Wander();
            return;
        }

        // Clumsy bots sometimes head for a random ball instead of the closest one
        Ball chosen = Random.value < (1f - skill) * 0.4f ? random : nearest;
        moveTarget    = chosen.transform.position;
        hasMoveTarget = true;
    }

    private void Wander() {
        if (!hasWander || Flat(wanderTarget - transform.position).magnitude < 2f)
            hasWander = TryRandomNearbyPoint(out wanderTarget);

        if (hasWander) {
            moveTarget    = wanderTarget;
            hasMoveTarget = true;
        }
    }

    // Occasionally ignore the plan and amble off somewhere for a moment
    private void ApplyDistraction() {
        if (Time.time >= distractedUntil) {
            if (Random.value >= distractionChance * (1f - skill)) return;
            if (!TryRandomNearbyPoint(out distractionTarget)) return;
            distractedUntil = Time.time + Random.Range(distractionDuration.x, distractionDuration.y);
        }

        moveTarget    = distractionTarget;
        hasMoveTarget = true;
        stopDistance  = 0f;
    }

    // ---------- Per-frame output ----------

    private void UpdateSteering() {
        Vector3 dir = Vector3.zero;

        if (hasMoveTarget) {
            Vector3 pos = transform.position;
            if (Flat(moveTarget - pos).magnitude > stopDistance)
                dir = Flat(NextWaypoint(pos) - pos).normalized;
        }

        Steering = new Vector2(dir.x, dir.z) * maxStick;
    }

    private void UpdateAim() {
        Vector3 desired;

        if (aimTarget != null && kart.HeldBall != null) {
            Vector3 to = Flat(aimTarget.transform.position - transform.position);
            desired = to.sqrMagnitude > 0.0001f
                ? Quaternion.Euler(0f, aimError, 0f) * to.normalized
                : aimDir;
        }
        else {
            desired = Steering.sqrMagnitude > 0.01f ? new Vector3(Steering.x, 0f, Steering.y).normalized : aimDir;
        }

        // Limited turn rate: moving targets get missed
        float maxTurn = aimTurnSpeed * Mathf.Lerp(0.5f, 1f, skill) * Mathf.Deg2Rad * Time.deltaTime;
        aimDir = Vector3.RotateTowards(aimDir, desired, maxTurn, 0f);
        Aim    = new Vector2(aimDir.x, aimDir.z);
    }

    private void UpdateShooting() {
        bool hasBall = kart.HeldBall != null;

        // Just picked a ball up: take a moment before thinking about shooting
        if (hasBall && !hadBall)
            nextShotTime = Time.time + Random.Range(0.3f, 1f) * (1.5f - skill);
        hadBall = hasBall;

        if (charging) {
            if (Time.time >= releaseTime) {
                ShootPerformed = true; // always pair a start with a release
                charging       = false;
                nextShotTime   = Time.time + Random.Range(0.5f, 1.5f);
            }

            return;
        }

        if (!hasBall || aimTarget == null || Time.time < nextShotTime) return;

        Vector3 to       = Flat(aimTarget.transform.position - transform.position);
        float   distance = to.magnitude;
        if (distance > shootRange) return;

        // Shoot once roughly lined up; sloppy bots accept a worse angle
        float tolerance = Mathf.Lerp(maxAimError, minAimError, skill) + 8f;
        if (Vector3.Angle(aimDir, to) > tolerance) return;

        // Farther target = longer charge, with some noise for clumsy bots
        float charge = Mathf.Lerp(chargeTime.x, chargeTime.y, distance / shootRange)
                       + Random.Range(-0.15f, 0.15f) * (1f - skill);

        ShootStarted = true;
        charging     = true;
        releaseTime  = Time.time + Mathf.Max(0.05f, charge);
    }

    // ---------- Navigation ----------

    private void RecalculatePath() {
        cornerCount = 0;
        path.ClearCorners();

        if (NavMesh.SamplePosition(transform.position, out NavMeshHit start, 3f, NavMesh.AllAreas) &&
            NavMesh.SamplePosition(moveTarget, out NavMeshHit end, 6f, NavMesh.AllAreas) &&
            NavMesh.CalculatePath(start.position, end.position, NavMesh.AllAreas, path)) {
            cornerCount = path.GetCornersNonAlloc(cornerBuffer);
        }
    }

    // First path corner that is still worth walking to; falls back to a straight line
    private Vector3 NextWaypoint(Vector3 pos) {
        for (int i = 1; i < cornerCount; i++) {
            if (i == cornerCount - 1 || Flat(cornerBuffer[i] - pos).magnitude > waypointReach)
                return cornerBuffer[i];
        }

        return moveTarget;
    }

    private bool TryRandomNearbyPoint(out Vector3 point) {
        Vector3 candidate = transform.position +
                            Quaternion.Euler(0f, Random.value * 360f, 0f) * Vector3.forward *
                            Random.Range(5f, wanderRadius);

        if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 4f, NavMesh.AllAreas)) {
            point = hit.position;
            return true;
        }

        point = default;
        return false;
    }

    // ---------- World queries ----------

    private Kart CrownHolder() {
        foreach (Kart k in karts) {
            if (k != kart) {
                bool hasCrown = k.GetComponent<KartEffects>().HasCrown;
                if (hasCrown) return k;
            }
        }
        return null;
    }

    // Nearest other kart; karts carrying a ball look twice as close
    private Kart NearestThreat() {
        Kart  best      = null;
        float bestScore = float.MaxValue;

        foreach (Kart k in karts) {
            if (k == kart) continue;

            float score = Flat(k.transform.position - transform.position).magnitude
                          * (k.HeldBall != null ? 0.5f : 1f);
            if (score < bestScore) {
                bestScore = score;
                best      = k;
            }
        }

        return best;
    }

    private static Vector3 Flat(Vector3 v) {
        v.y = 0f;
        return v;
    }
}