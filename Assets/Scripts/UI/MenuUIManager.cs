using Managers;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI {
    [RequireComponent(typeof(UIDocument))]
    public class MenuUIManager : MonoBehaviour {
        private Button startButton;
        private Button tutorialButton;
        private Button quitButton;
        
        private void Awake() {
            var uiDoc = GetComponent<UIDocument>();
            
            startButton = uiDoc.rootVisualElement.Q<Button>("Start");
            tutorialButton = uiDoc.rootVisualElement.Q<Button>("Tutorial");
            quitButton = uiDoc.rootVisualElement.Q<Button>("Quit");
            
            startButton.Focus();
            startButton.clicked  += OnStartButton;
            quitButton.clicked += OnQuit;
        }

        private void OnDestroy() {
            startButton.clicked -= OnStartButton;
            quitButton.clicked -= OnQuit;
        }

        private void OnQuit() {
            Application.Quit();
        }

        private void OnStartButton() {
            SceneManager.Instance.LoadNextScene();
        }
    }
}