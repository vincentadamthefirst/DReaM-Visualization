using TMPro;
using UI.Main_Menu.Utils;

namespace UI.SidePanel {
    
    
    public class TextEntry : SidePanelEntry {
        public TMP_Text infoText;

        public void UpdateText(string text) {
            infoText.SetText(text);
        }
    }
}