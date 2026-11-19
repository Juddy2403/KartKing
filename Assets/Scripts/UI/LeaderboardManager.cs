using Managers;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI {
    [RequireComponent(typeof(UIDocument))]
    public class LeaderboardManager : MonoBehaviour {
        [SerializeField] private VisualTreeAsset scoreEntry;

        private UIDocument        hud;
        private ScoreBoardElement scoreBoardElement;

        private void Awake() {
            hud               = GetComponent<UIDocument>();
            scoreBoardElement = hud.rootVisualElement.Q<ScoreBoardElement>();
        }

        private void Start() {
            scoreBoardElement.Initialize(scoreEntry, ScoreManager.Instance.Scores);
        }

    }
}