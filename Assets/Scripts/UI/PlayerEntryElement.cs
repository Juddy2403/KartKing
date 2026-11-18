using UnityEngine;
using UnityEngine.UIElements;

namespace UI {
    [UxmlElement("Entry")]
    public partial class PlayerEntryElement : VisualElement {
        public int Id { get; private set; }
        public void Initialize(string labelText, Color color, int id) {
            var label = this.Q<Label>("Label");
            label.text        = labelText;
            label.style.color = color;
            Id                = id;
        }
    }
}