using System;
using TMPro;
using UnityEngine;

namespace UI.Main_Menu.Settings {
    public class Heading : Setting<string> {
        private TMP_Text _headingText;
        private string _text;

        public override void SetInfo(params string[] infos) {
            _headingText = transform.Find("Text").GetComponent<TMP_Text>();
            _headingText.SetText(infos[0]);
        }

        public override void StoreData() { }

        public override void LoadData() { }
    }
}