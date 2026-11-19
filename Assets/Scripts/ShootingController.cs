using UnityEngine;
using UnityEngine.Events;

public class ShootingController : MonoBehaviour {
    [SerializeField] private LineRenderer aimIndicator;
    [SerializeField] private float        indicatorLength;
    [SerializeField] private float        shootingForce;
    [SerializeField] private Transform    ballShootingPos;
    [SerializeField] private GameObject   ballPrefab;

    public  UnityEvent onBallShot;
    private IKartInput input;

    private void Start() {
        input   = GetComponent<IKartInput>();
    }

    private void OnEnable() {
        aimIndicator.enabled = true;
    }

    private void OnDisable() {
        aimIndicator.SetPosition(1, Vector3.zero);
        aimIndicator.enabled = false;
    }

    private void Update() {
        Vector2 aim        = input.Aim;
        bool    isShooting = input.Shoot;

        Vector3 aimDirection = new Vector3(aim.x, 0f, aim.y);

        if (Vector3.Magnitude(aimDirection) < 0.1f) {
            aimIndicator.enabled = false;
        }
        else {
            aimIndicator.enabled = true;
            aimIndicator.SetPosition(0, aimIndicator.transform.position);
            aimIndicator.SetPosition(1, aimIndicator.transform.position + aimDirection * indicatorLength);
            
            if (isShooting) ShootBall(aimDirection);
        }
    }

    private void ShootBall(Vector3 aimDirection) {
        GameObject ball = Instantiate(ballPrefab);
        ball.transform.position = ballShootingPos.position;
        ball.GetComponent<Rigidbody>().AddForce(aimDirection * shootingForce, ForceMode.Impulse);
            
        onBallShot?.Invoke();
    }
}