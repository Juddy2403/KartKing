using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Managers {
    [RequireComponent(typeof(UIDocument))]
    public class HUDManager : MonoBehaviour {
        private UIDocument hud;
        private Label      timerText;

        private void Awake() {
            hud  = GetComponent<UIDocument>();
            timerText = hud.rootVisualElement.Q<Label>("TimerText");
            TimeSpan timeSpan = TimeSpan.FromSeconds(GameManager.Instance.RoundTime);
            timerText.text = timeSpan.ToString(@"mm\:ss");
        }

        private void Start() {
            GameManager.Instance.onTimeTick.AddListener(OnTimeTick);
        }
        private void OnDestroy() {
            GameManager.Instance.onTimeTick.RemoveListener(OnTimeTick);
        }

        private void OnTimeTick(float time) {
            TimeSpan timeSpan = TimeSpan.FromSeconds(time);
            timerText.text = timeSpan.ToString(@"mm\:ss");
        }
    }
}