using System.Collections.Generic;
using Managers;
using UnityEngine;

namespace Spawners {
    public class BallSpawner : MonoBehaviour {
        [SerializeField] private BoxCollider spawnBounds;
        [SerializeField] private GameObject  ballPrefab;
        [SerializeField] private GameObject  spawnVFX;
        [SerializeField] private Vector3     spawnOffset;

        [SerializeField] private float spawnBallsAfter = 4f;

        [SerializeField] private int ballsAtMinPlayers = 1;
        [SerializeField] private int ballsAtMaxPlayers = 7;

        [Header("Late game")] 
        
        [SerializeField] private int   lateBallsAtMinPlayers = 2;
        [SerializeField] private int   lateBallsAtMaxPlayers = 9;

        private readonly List<GameObject> balls         = new();
        private          float            timeRemaining = float.PositiveInfinity;

        private void Start() {
            GameManager.Instance.onTimeTick.AddListener(OnTimeTick);
            Invoke(nameof(SpawnBalls), spawnBallsAfter);
        }

        private void OnTimeTick(float seconds) {
            timeRemaining = seconds;
            SpawnBalls();
        }

        private void SpawnBalls() {
            int target = TargetBallCount();
            while (balls.Count < target) {
                if (!SpawnBall()) break;
            }
        }

        private int TargetBallCount() {
            bool lateGame = GameManager.Instance.IsInLateGame;
            float t = Mathf.InverseLerp(PlayerManager.MinPlayersToStart,
                PlayerManager.MaxPlayers, PlayerManager.Instance.PlayerCount);

            int atMin = lateGame ? lateBallsAtMinPlayers : ballsAtMinPlayers;
            int atMax = lateGame ? lateBallsAtMaxPlayers : ballsAtMaxPlayers;

            return Mathf.Max(1, Mathf.RoundToInt(Mathf.Lerp(atMin, atMax, t)));
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