using TMPro;

namespace UI.SidePanel {
    
    
    public class TextEntry : SidePanelEntry {
        public TMP_Text infoText;

        public void UpdateText(string text) {
            infoText.SetText(text);
        }
    }
}