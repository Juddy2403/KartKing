using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Managers {
    public struct ScoreData {
        public PlayerSlot player;
        public int        score;
    }

    public class ScoreManager : MonoBehaviour {
        public static ScoreManager Instance;

        [SerializeField] private int scorePerSecond;
        [SerializeField] private int scoreMultiplier = 5;

        public List<ScoreData>       Scores { get; private set; } = new List<ScoreData>();
        public UnityEvent<ScoreData> onScoreUpdate = new();

        private Crown crown;
        private int   crownHolderIndex = -1;
        private int   lastReportedScore;
        private float pickUpTime = 0f;

        private void Awake() {
            crown = FindFirstObjectByType<Crown>();
            if (!crown) Debug.LogError("No crown found in scene");

            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            foreach (var player in PlayerManager.Instance.Players) {
                Scores.Add(new ScoreData { player = player, score = 0 });
            }
        }

        private void Start() {
            SceneManager.Instance.OnSceneChange.AddListener(OnSceneChange);
            crown.onPickUp.AddListener(OnCrownPickup);
        }
        
        private void OnSceneChange(SceneManager.SceneType type) {
            if (type is SceneManager.SceneType.menu or SceneManager.SceneType.arena) {
                Instance = null;
                Destroy(gameObject);
            }
        }

        private void OnCrownPickup(Kart kart) {
            for (int i = 0; i < Scores.Count; i++) {
                if (Scores[i].player.Index != kart.Slot.Index) continue;
                crownHolderIndex = i;
                break;
            }

            lastReportedScore = 0;
            pickUpTime        = Time.time;
        }

        private void Update() {
            if (crownHolderIndex == -1) return;

            var timeSincePickup = Time.time - pickUpTime;
            var scoreData       = Scores[crownHolderIndex];

            int newScore = Mathf.FloorToInt(scorePerSecond * timeSincePickup);

            if (newScore < lastReportedScore + scoreMultiplier)
                return;

            scoreData.score          = newScore;
            Scores[crownHolderIndex] = scoreData;

            lastReportedScore = newScore;

            onScoreUpdate.Invoke(scoreData);
        }

        private void OnDestroy() {
            onScoreUpdate.RemoveAllListeners();
        }

    }
}