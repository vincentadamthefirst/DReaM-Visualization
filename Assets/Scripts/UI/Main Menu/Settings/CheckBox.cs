using TMPro;
using UnityEngine.UI;

namespace UI.Main_Menu.Settings {
    public class CheckBox : Setting {
        public Toggle toggle;
        public TMP_Text infoText;
        
        private string _text;
        private bool _defaultValue;

        public void SetData(string text, bool defaultValue) {
            _text = text;
            _defaultValue = defaultValue;
        }

        public void SetValue(bool value) {
            _defaultValue = value;
            toggle.SetIsOnWithoutNotify(_defaultValue);
        }
        
        public void Start() {
            toggle = transform.GetComponentInChildren<Toggle>();
            infoText = transform.GetComponentInChildren<TMP_Text>();

            toggle.SetIsOnWithoutNotify(_defaultValue);
            infoText.SetText(_text);
        }

        public bool IsOn() {
            return toggle.isOn;
        }
    }
}