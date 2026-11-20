using UnityEngine;
using UnityEngine.Events;

namespace Managers {
    public class GameManager : MonoBehaviour {
        [SerializeField] private float roundTime;
        public                   float RoundTime => roundTime;
        
        [SerializeField] private float lateGameSeconds       = 15f;
        public bool IsInLateGame => currentRoundTime <= lateGameSeconds;

        public static GameManager       Instance;
        private       float             currentRoundTime;
        public        UnityEvent        onRoundEnded = new UnityEvent();
        public        UnityEvent<float> onTimeTick   = new UnityEvent<float>();
        public        bool              shouldTick   = false;

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start() {
            SceneManager.Instance.OnSceneChange.AddListener(OnSceneChange);
        }

        private void OnDestroy() {
            SceneManager.Instance.OnSceneChange?.RemoveListener(OnSceneChange);
            onRoundEnded.RemoveAllListeners();
            onTimeTick.RemoveAllListeners();
        }

        private void OnSceneChange(SceneManager.SceneType type) {
            if (type == SceneManager.SceneType.arena) {
                currentRoundTime = RoundTime;
                shouldTick       = true;
            }
            else {
                shouldTick = false;
            }
        }

        private void Update() {
            if (shouldTick) Tick();
        }

        private void Tick() {
            currentRoundTime -= Time.deltaTime;
            onTimeTick.Invoke(currentRoundTime);
            
            // Letting the timer show 0:00 for a full second
            if (currentRoundTime >= 0) return;
            
            onRoundEnded.Invoke();
            shouldTick = false;
            SceneManager.Instance.LoadNextScene();
        }
    }
}