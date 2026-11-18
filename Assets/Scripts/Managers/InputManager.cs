using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Managers {
    public class InputManager : MonoBehaviour {
        [SerializeField] private InputActionAsset inputActions;
        private static           InputManager     instance;

        private void Awake() {
            if (instance != null && instance != this) {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start() {
            SceneManager.Instance.OnSceneChange.AddListener(OnSceneChange);
        }

        private void OnDestroy() {
            SceneManager.Instance.OnSceneChange?.RemoveListener(OnSceneChange);
            var map = inputActions.FindActionMap("Lobby", true);
            map.FindAction("Confirm").started -= HandleConfirm;
            map.FindAction("Cancel").started  -= HandleCancel;
        }

        private void OnSceneChange(SceneManager.SceneType type) {
            var map = inputActions.FindActionMap("Lobby", true);
            switch (type) {
                case SceneManager.SceneType.arena:
                    map.Disable();
                    map.FindAction("Confirm").started -= HandleConfirm;
                    map.FindAction("Cancel").started  -= HandleCancel;
                    break;
                case SceneManager.SceneType.lobby:
                    map.Enable();
                    map.FindAction("Confirm").started += HandleConfirm;
                    map.FindAction("Cancel").started  += HandleCancel;
                    break;
                case SceneManager.SceneType.menu:
                    break;
                case SceneManager.SceneType.leaderboard:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private void HandleConfirm(InputAction.CallbackContext obj) {
            var device = obj.control.device;
            if (PlayerManager.Instance.FindSlot(device) != null) {
                if (PlayerManager.Instance.CanStart) SceneManager.Instance.LoadNextScene();
                return;
            }

            PlayerManager.Instance.TryJoin(device, inputActions);
        }

        private void HandleCancel(InputAction.CallbackContext obj) {
            var        device = obj.control.device;
            PlayerSlot slot   = PlayerManager.Instance.FindSlot(device);
            if (slot != null) PlayerManager.Instance.Leave(slot);
        }
    }
}