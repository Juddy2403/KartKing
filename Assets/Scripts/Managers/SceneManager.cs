using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Managers {
    [Serializable]
    public struct Scene {
        public string                 name;
        public SceneManager.SceneType type;
    }
    public class SceneManager : MonoBehaviour {
        [SerializeField] private List<Scene> scenes;
        // Not included in the load next rotation
        [SerializeField] private Scene tutorialScene;

        public static SceneManager Instance { get; private set; }

        public  SceneType             CurrentScene => scenes[sceneIdx].type;
        public  UnityEvent<SceneType> OnSceneChange;
        private int                   sceneIdx = 0;

        public enum SceneType {
            menu,
            lobby,
            arena,
            leaderboard,
            tutorial
        }

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        private IEnumerator Start()
        {
            // Making sure that all scripts have time to subscribe before invoking
            yield return null;
            OnSceneChange?.Invoke(scenes[sceneIdx].type);
        }

        private void OnDestroy() {
            OnSceneChange.RemoveAllListeners();
        }

        public void LoadNextScene() {
            sceneIdx = (sceneIdx + 1) % scenes.Count;
            UnityEngine.SceneManagement.SceneManager.LoadScene(scenes[sceneIdx].name);
            OnSceneChange?.Invoke(scenes[sceneIdx].type);
        }

        public void LoadScene(SceneType sceneType) {
            if (sceneType == SceneType.tutorial) {
                UnityEngine.SceneManagement.SceneManager.LoadScene(tutorialScene.name);
                OnSceneChange?.Invoke(tutorialScene.type);
                return;
            }
            for (int i = 0; i < scenes.Count; i++) {
                if (scenes[i].type != sceneType) continue;
                sceneIdx = i;
                UnityEngine.SceneManagement.SceneManager.LoadScene(scenes[i].name);
                OnSceneChange?.Invoke(scenes[i].type);
                break;
            }
        }
    }
}