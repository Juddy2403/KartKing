using UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Managers {
    [RequireComponent(typeof(UIDocument))]
    public class LobbyUIManager : MonoBehaviour {
        [SerializeField] private VisualTreeAsset     entry;
        private                  UIDocument          uiDoc;
        private                  VisualElement       playerList;

        private void Awake() {
            uiDoc      = GetComponent<UIDocument>();
            playerList = uiDoc.rootVisualElement.Q<VisualElement>("PlayerList");
        }

        private void Start() {
            PlayerManager.Instance.PlayerJoined += OnPlayerJoined;
            PlayerManager.Instance.PlayerLeft += OnPlayerLeft;
        }

        private void OnDestroy() {
            PlayerManager.Instance.PlayerJoined -= OnPlayerJoined;
            PlayerManager.Instance.PlayerLeft -= OnPlayerLeft;
        }

        private void OnPlayerJoined(PlayerSlot player) {
            PlayerEntryElement playerEntryElement = entry.Instantiate().Q<PlayerEntryElement>();
            playerEntryElement.Initialize(player.DisplayName, player.Color, player.Index);
            playerList.Add(playerEntryElement);
        }
        
        private void OnPlayerLeft(PlayerSlot player) {
            foreach (var child in playerList.Children()) {
                if (child is not PlayerEntryElement playerEntry) continue;
                if (playerEntry.Id != player.Index) continue;
                playerEntry.RemoveFromHierarchy();
                break;
            }
        }
    }
}