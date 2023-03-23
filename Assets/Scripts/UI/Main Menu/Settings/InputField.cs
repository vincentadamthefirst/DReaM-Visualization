using System;
using TMPro;
using UnityEngine.Serialization;

namespace UI.Main_Menu.Settings {
    public class InputField : Setting<string> {
        public TMP_InputField Field { get; private set; }
        private TMP_Text _infoText;
        private TMP_Text _placeholderText;
        
        private string _text;
        private string _defaultValue;
        private string _placeholder;

        public override void SetInfo(params string[] infos) {
            Field = transform.Find("Wrapper").Find("Input").GetComponent<TMP_InputField>();
            _infoText = transform.Find("Text").GetComponent<TMP_Text>();
            _placeholderText = Field.transform.GetChild(0).Find("Placeholder").GetComponent<TMP_Text>();
            
            _infoText.SetText(infos[0]);
            _placeholderText.SetText(infos[1]);
        }

        public override void StoreData() {
            Reference.Value = Field.text;
        }

        public override void LoadData() {
            Field.SetTextWithoutNotify(Reference.Value);
        }
    }
}