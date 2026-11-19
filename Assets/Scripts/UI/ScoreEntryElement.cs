using UnityEngine;
using UnityEngine.UIElements;

namespace UI {
    [UxmlElement("ScoreEntry")]
    public partial class ScoreEntryElement : VisualElement {
        private Label scoreLabel;
        public  int   Id    { get; private set; }
        public  int   Score { get; private set; }

        public void Initialize(string labelText, Color color, int id) {
            var label = this.Q<Label>("Name");
            scoreLabel             = this.Q<Label>("Score");
            label.text             = labelText;
            label.style.color      = color;
            scoreLabel.style.color = color;
            Id                     = id;
        }

        public void UpdateScore(int score) {
            Score = score;
            scoreLabel.text = score.ToString();
        }

        public void AddWinnerClass() {
            AddToClassList("winner");
        }
        
        public void RemoveWinnerClass() {
            RemoveFromClassList("winner");
        }
    }
}