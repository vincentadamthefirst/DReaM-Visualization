using TMPro;
using UI.Main_Menu_Rework.Utils;
using UnityEngine;

namespace UI.Main_Menu_Rework.Elements {
    
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class CustomUiText : CustomUiElement {

        [Tooltip("The type of the text element")]
        public ApplicationColor color;
        [Tooltip("The text style of the text")]
        public TextStyle textStyle;
        
        // the TMP element
        private TextMeshProUGUI _text;

        public override void UpdateUiElement() {
            _text = GetComponent<TextMeshProUGUI>();
            _text.color = centralUiController.applicationDesign.GetColor(color);
            _text.font = centralUiController.applicationDesign.GetFont(textStyle);
        }
    }
}