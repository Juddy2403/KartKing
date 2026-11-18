using UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Managers {
    [RequireComponent(typeof(UIDocument))]
    public class LobbyUIManager : MonoBehaviour {
        [SerializeField] private VisualTreeAsset entry;
        private                  UIDocument    uiDoc;
        private                  ListView      playerList;

        private void Awake() {
            uiDoc    = GetComponent<UIDocument>();
            playerList = uiDoc.rootVisualElement.Q<ListView>("PlayerList");
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
            VisualElement      playerEntry        = entry.Instantiate();
            PlayerEntryElement playerEntryElement = playerEntry.Q<PlayerEntryElement>();
            playerEntryElement.Initialize(player.DisplayName, player.Color, player.Index);
            playerList.hierarchy.Add(playerEntryElement);
        }
        
        private void OnPlayerLeft(PlayerSlot player) {
            foreach (var child in playerList.Children()) {
                PlayerEntryElement playerEntryElement = (PlayerEntryElement)child;
                if (playerEntryElement.Id != player.Index) continue;
                child.RemoveFromHierarchy();
                break;
            }
        }
    }
}