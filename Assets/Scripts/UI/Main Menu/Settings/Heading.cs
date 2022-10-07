using TMPro;

namespace UI.Main_Menu.Settings {
    public class Heading : Setting {
        private TMP_Text _headingText;
        private string _text;

        public void SetData(string text) {
            _text = text;
        }
        
        public void Start() {
            _headingText = transform.GetComponentInChildren<TMP_Text>();
            _headingText.SetText(_text);
        }
    }
}