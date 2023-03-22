using TMPro;
using Utils;

namespace UI.SidePanel {
    
    
    public class TextSidePanelEntry : SidePanelEntry {
        private TMP_Text _infoText;
        
        public Reference<string> Reference { get; set; }

        protected override void Awake() {
            base.Awake();
            _infoText = transform.Find("Info").GetComponent<TMP_Text>();
        }

        public override void TriggerUpdate() {
            _infoText.SetText(Reference.Value);
        }
    }
}