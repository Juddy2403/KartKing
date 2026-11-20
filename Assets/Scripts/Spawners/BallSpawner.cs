using System.Collections.Generic;
using Managers;
using UnityEngine;

namespace Spawners {
    public class BallSpawner : MonoBehaviour {
        [SerializeField] private BoxCollider spawnBounds;
        [SerializeField] private GameObject  ballPrefab;
        [SerializeField] private GameObject  spawnVFX;
        [SerializeField] private GameObject  destroyVFX;
        [SerializeField] private Vector3     spawnOffset;
        [Tooltip("Share of the player-based ball count that is present before anyone has held the crown")]
        [SerializeField, Range(0f, 1f)] private float startBallFraction = 0.5f;
        
        [SerializeField] private float spawnBallsAfter = 4f;

        [SerializeField] private int   ballsAtMinPlayers = 1;
        [SerializeField] private int   ballsAtMaxPlayers = 7;
        [Tooltip("At this point no additional balls are spawned")]
        [SerializeField] private float maxCrownHoldTime  = 30f;

        [Header("Late game")] 
        
        [SerializeField] private int   lateBallsAtMinPlayers = 2;
        [SerializeField] private int   lateBallsAtMaxPlayers = 9;

        private readonly List<GameObject> balls         = new();

        private void Start() {
            GameManager.Instance.onTimeTick.AddListener(OnTimeTick);
            Invoke(nameof(SpawnBalls), spawnBallsAfter);
        }

        private void OnTimeTick(float seconds) {
            SpawnBalls();
        }

        private void SpawnBalls() {
            int target = TargetBallCount();
            while (balls.Count < target) {
                if (!SpawnBall()) break;
            }

            while (balls.Count > target) {
                if (!DestroyBall()) break;
            }
        }

        private bool DestroyBall() {
            if (balls.Count <= 0) return false;

            int index = Random.Range(0, balls.Count);

            GameObject ball = balls[index];
            ball.GetComponent<Ball>().Holder?.DropBall();
            balls.RemoveAt(index);
            Instantiate(destroyVFX, ball.transform.position, Quaternion.identity);
            Destroy(ball);
            return true;
        }

        private int TargetBallCount() {
            bool lateGame = GameManager.Instance.IsInLateGame;

            float playerT = Mathf.InverseLerp(
                PlayerManager.MinPlayersToStart,
                PlayerManager.MaxPlayers,
                PlayerManager.Instance.PlayerCount);

            int atMin = lateGame ? lateBallsAtMinPlayers : ballsAtMinPlayers;
            int atMax = lateGame ? lateBallsAtMaxPlayers : ballsAtMaxPlayers;

            // Scale from minimum to maximum based on crown hold time.
            float crownT = Mathf.Clamp01(ScoreManager.Instance.TimeSincePickup / maxCrownHoldTime);
            int playerBasedCount = Mathf.RoundToInt(Mathf.Lerp(atMin, atMax, playerT));
            int startCount       = Mathf.Max(1, Mathf.RoundToInt(playerBasedCount * startBallFraction));

            return Mathf.Max(1, Mathf.RoundToInt(Mathf.Lerp(startCount, playerBasedCount, crownT)));
        }

        private bool SpawnBall() {
            if (!Helpers.TryGetRandomPoint(spawnBounds.bounds, out Vector3 pos)) {
                Debug.LogError($"Could not find a navmesh point inside {spawnBounds}");
                return false; // don't spawn at an uninitialised position
            }

            balls.Add(Instantiate(ballPrefab, pos + spawnOffset, Quaternion.identity));
            Instantiate(spawnVFX, pos + spawnOffset, Quaternion.identity);
            return true;
        }
    }
}