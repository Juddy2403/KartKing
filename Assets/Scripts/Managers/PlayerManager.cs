using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Managers {
    public class PlayerManager : MonoBehaviour {
        public const int MaxPlayers        = 12;
        public const int MinPlayersToStart = 2;

        public static PlayerManager Instance { get; private set; }

        [SerializeField] private List<Color>  playerColors;
        [SerializeField] private List<string> colorNames;

        private Dictionary<Color, string> colorNamesDict = new Dictionary<Color, string>(MaxPlayers);

        private readonly PlayerSlot[] slots = new PlayerSlot[MaxPlayers];
        public           string GetColorName(Color color) => colorNamesDict[color];
        private          bool acceptingJoins => SceneManager.Instance.CurrentScene == SceneManager.SceneType.lobby;

        public IEnumerable<PlayerSlot> Players     => slots.Where(s => s != null);
        public int                     PlayerCount => Players.Count();
        public bool                    CanStart    => PlayerCount >= MinPlayersToStart;

        public event Action<PlayerSlot> PlayerJoined;
        public event Action<PlayerSlot> PlayerLeft;
        public event Action<PlayerSlot> PlayerStateChanged; // disconnected / reconnected / reclaimed

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            for (int i = 0; i < MaxPlayers; i++) {
                colorNamesDict.Add(playerColors[i], colorNames[i]);
            }

            playerColors.Shuffle();
        }

        private void Start() {
            InputSystem.onDeviceChange += OnDeviceChange;
            SceneManager.Instance.OnSceneChange.AddListener(OnSceneChange);
        }

        private void OnDestroy() {
            InputSystem.onDeviceChange -= OnDeviceChange;
        }

        private void OnSceneChange(SceneManager.SceneType type) {
            if (type != SceneManager.SceneType.menu) return;
            foreach (var slot in Players.ToList()) Leave(slot);
        }

        public void TryJoin(InputDevice device, InputActionAsset inputActions) {
            int index = FirstFreeIndex();
            if (index < 0) return;

            var slot = new PlayerSlot(index, ColorFor(index), inputActions, device);
            slots[index] = slot;
            PlayerJoined?.Invoke(slot);
        }

        public void Leave(PlayerSlot slot) {
            if (slot == null || slots[slot.Index] != slot) return;
            slots[slot.Index] = null;
            slot.Dispose();
            PlayerLeft?.Invoke(slot);
        }

        public void AddBot() {
            int index = FirstFreeIndex();
            if (index < 0) return;

            var slot = new PlayerSlot(index, ColorFor(index), null, null);
            slots[index] = slot;
            PlayerJoined?.Invoke(slot);
        }

        public void RemoveLastBot() {
            PlayerSlot bot = Players.LastOrDefault(s => s.IsBot);
            if (bot != null) Leave(bot);
        }

        private void OnDeviceChange(InputDevice device, InputDeviceChange change) {
            PlayerSlot slot = FindSlot(device);
            if (slot == null) return;

            switch (change) {
                case InputDeviceChange.Disconnected:
                case InputDeviceChange.Removed:
                    if (acceptingJoins) Leave(slot); // lobby: just drop the seat
                    else {
                        slot.SetConnected(false); // match: keep the seat, kart coasts
                        PlayerStateChanged?.Invoke(slot);
                    }

                    break;
                case InputDeviceChange.Reconnected:
                    TryReclaim(device);
                    slot.SetConnected(true);
                    PlayerStateChanged?.Invoke(slot);
                    break;
            }
        }

        private void TryReclaim(InputDevice device) {
            PlayerSlot lost = Players.FirstOrDefault(slot => !slot.IsBot && !slot.IsConnected);
            if (lost == null) return;

            lost.AssignDevice(device);
            PlayerStateChanged?.Invoke(lost);
        }

        #region Helpers

        public PlayerSlot FindSlot(InputDevice device) =>
            slots.FirstOrDefault(s => s != null && s.Device == device);

        private int FirstFreeIndex() {
            for (int i = 0; i < slots.Length; i++) {
                if (slots[i] == null) return i;
            }

            return -1;
        }

        private Color ColorFor(int index) {
            if (playerColors != null && playerColors.Count > 0) {
                return playerColors[index];
            }

            Debug.LogError("Player colors not initialized");
            return Color.white;
        }

        #endregion
    }
}