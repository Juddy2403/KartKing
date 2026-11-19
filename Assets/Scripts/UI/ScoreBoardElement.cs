using System.Collections.Generic;
using Managers;
using UnityEngine.UIElements;

namespace UI {
    [UxmlElement("ScoreBoard")]
    public partial class ScoreBoardElement : VisualElement {
        private VisualTreeAsset entry;

        public void Initialize(VisualTreeAsset entryAsset, List<ScoreData> scores, bool hideLast = false) {
            entry  = entryAsset;
            foreach (var score in scores) {
                AddData(score);
            }

            UpdateList(hideLast);
        }
        
        private void AddData(ScoreData data) {
            ScoreEntryElement scoreEntryElement = entry.Instantiate().Q<ScoreEntryElement>();
            scoreEntryElement.Initialize(data.player.DisplayName, data.player.Color, data.player.Index);
            scoreEntryElement.UpdateScore(data.score);
            Add(scoreEntryElement);
        }

        public void UpdateScore(ScoreData score, bool hideLast = false) {
            foreach (var child in Children()) {
                if (child is not ScoreEntryElement scoreElement) continue;
                if (scoreElement.Id != score.player.Index) continue;
                scoreElement.UpdateScore(score.score);
            }

            UpdateList(hideLast);
        }
        
        private void UpdateList(bool hideLast) {
            Sort((a, b) => 
                ((ScoreEntryElement)b).Score.CompareTo(
                ((ScoreEntryElement)a).Score
            ));

            for (int i = 0; i < childCount; i++) {
                var entry = (ScoreEntryElement)ElementAt(i);
                if (i == 0) entry.AddWinnerClass(); 
                else entry.RemoveWinnerClass();
            }
            
            if (!hideLast) return;
            
            for (int i = 0; i < childCount; i++) {
                ElementAt(i).style.display = i < 3
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
            }
            
        }
    }
}