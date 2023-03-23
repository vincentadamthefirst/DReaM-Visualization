using System;
using TMPro;
using UnityEngine.UI;

namespace UI.Main_Menu.Settings {
    public class CheckBox : Setting<bool> {
        private Toggle _toggle;
        private TMP_Text _infoText;
        
        private string _text;
        private bool _defaultValue;

        public override void SetInfo(params string[] infos) {
            _toggle = transform.Find("Toggle").GetComponent<Toggle>();
            _infoText = transform.Find("Text").GetComponent<TMP_Text>();
            
            _infoText.SetText(infos[0]);
        }

        public override void StoreData() {
            Reference.Value = _toggle.isOn;
        }

        public override void LoadData() {
            _toggle.SetIsOnWithoutNotify(Reference.Value);
        }
    }
}