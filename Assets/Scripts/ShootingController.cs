using UnityEngine;

public class ShootingController : MonoBehaviour {
    [SerializeField] private LineRenderer aimIndicator;
    [SerializeField] private float        indicatorLength;
    [SerializeField] private float        shootingForce;
    [SerializeField] private GameObject   ballMesh;
    [SerializeField] private GameObject   ballPrefab;

    private IKartInput input;

    private void Start() {
        input = GetComponent<IKartInput>();
        enabled = false;
    }

    private void OnEnable() {
        ballMesh.SetActive(true);
        aimIndicator.enabled = true;
    }

    private void OnDisable() {
        aimIndicator.SetPosition(1, Vector3.zero);
        aimIndicator.enabled = false;
        ballMesh?.SetActive(false);
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
            if (!isShooting) return;
            GameObject ball = Instantiate(ballPrefab);
            ball.transform.position = ballMesh.transform.position;
            ball.GetComponent<Rigidbody>().AddForce(aimDirection * shootingForce, ForceMode.Impulse);
            enabled = false;
        }
       
    }
}