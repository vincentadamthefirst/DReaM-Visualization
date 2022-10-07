using TMPro;

namespace UI.Main_Menu.Settings {
    public class InputField : Setting {
        public TMP_InputField inputField;
        public TMP_Text infoText;
        public TMP_Text placeholderText;
        
        private string _text;
        private string _defaultValue;
        private string _placeholder;

        public void SetData(string text, string placeholder, string defaultValue) {
            _text = text;
            _placeholder = placeholder;
            _defaultValue = defaultValue;
        }
        
        public void SetValue(string value) {
            _defaultValue = value;
            inputField.SetTextWithoutNotify(_defaultValue);
        }
        
        public void Start() {
            inputField.SetTextWithoutNotify(_defaultValue);
            infoText.SetText(_text);
            placeholderText.SetText(_placeholder);
        }

        public string GetValue() {
            return inputField.text;
        }
    }
}