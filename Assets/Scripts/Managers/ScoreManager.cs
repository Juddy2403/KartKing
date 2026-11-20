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

        [SerializeField] private int   scorePerSecond = 5;
        [Tooltip("The longer the player holds the crown continuously, the more points per second they acquire")]
        [SerializeField] private float   scoreIncreasePerSecond = 0.1f;
        
        [SerializeField] private float lateGameScoreMultiplier = 2f;
        [SerializeField] private int   scoreRounding         = 5;

        public  List<ScoreData>       Scores { get; private set; } = new List<ScoreData>();
        public  UnityEvent<ScoreData> onScoreUpdate = new();
        public  float                 TimeSincePickup { get; private set; } = 0f;
        private Crown                 crown;
        private int                   crownHolderIndex = -1;
        private int                   lastReportedScore;
        private float                 pickUpTime = 0f;

        private void Awake() {
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
            crown = FindFirstObjectByType<Crown>();
            if (!crown) Debug.LogError("No crown found in scene");

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

            TimeSincePickup = Time.time - pickUpTime;
            var  scoreData       = Scores[crownHolderIndex];
            bool lateGame        = GameManager.Instance.IsInLateGame;
            var  scoreMultiplier = lateGame ? lateGameScoreMultiplier : 1f;
            int  newScore        = Mathf.FloorToInt((scorePerSecond + scoreIncreasePerSecond * TimeSincePickup ) * scoreMultiplier * TimeSincePickup);

            if (newScore < lastReportedScore + scoreRounding) return;

            scoreData.score          = newScore;
            Scores[crownHolderIndex] = scoreData;
            lastReportedScore        = newScore;
            onScoreUpdate.Invoke(scoreData);
        }

        private void OnDestroy() {
            onScoreUpdate.RemoveAllListeners();
        }
    }
}