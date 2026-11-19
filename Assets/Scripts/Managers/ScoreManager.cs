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

        public List<ScoreData>       Scores { get; private set; } = new List<ScoreData>();
        public UnityEvent<ScoreData> onScoreUpdate = new();

        private Crown crown;
        private int   crownHolderIndex = -1;
        private float pickUpTime       = 0f;

        private void Start() {
            foreach (var player in PlayerManager.Instance.Players) {
                Scores.Add(new ScoreData { player = player, score = 0 });
            }

            crown.onPickUp.AddListener(OnCrownPickup);
        }

        private void OnCrownPickup(Kart kart) {
            for (int i = 0; i < Scores.Count; i++) {
                if (Scores[i].player.Index != kart.Slot.Index) continue;
                crownHolderIndex = i;
                break;
            }

            pickUpTime = Time.time;
        }

        private void Update() {
            if (crownHolderIndex == -1) return;

            var timeSincePickup = Time.time - pickUpTime;
            var scoreData       = Scores[crownHolderIndex];
            
            scoreData.score          = Mathf.FloorToInt(scorePerSecond * timeSincePickup);
            Scores[crownHolderIndex] = scoreData;
            
            onScoreUpdate.Invoke(scoreData);
        }

        private void Awake() {
            crown = FindFirstObjectByType<Crown>();
            if (!crown) Debug.LogError("No crown found in scene");
            
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy() {
            onScoreUpdate.RemoveAllListeners();
        }
    }
}