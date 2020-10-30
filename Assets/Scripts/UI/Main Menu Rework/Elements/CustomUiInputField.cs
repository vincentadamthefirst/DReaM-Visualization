using TMPro;
using UI.Main_Menu_Rework.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Main_Menu_Rework.Elements {
    
    [RequireComponent(typeof(TMP_InputField))]
    public class CustomUiInputField : CustomUiElement {

        public ApplicationColor textColor;
        public ApplicationColor backgroundColor;
        public TextStyle textStyle;
        public TextStyle placeholderStyle;
        
        private Image _textArea;
        private TextMeshProUGUI _text;
        private TextMeshProUGUI _placeholder;

        public override void UpdateUiElement() {
            _textArea = GetComponent<Image>();
            _text = transform.GetChild(0).Find("Text").GetComponent<TextMeshProUGUI>();
            _placeholder = transform.GetChild(0).Find("Placeholder").GetComponent<TextMeshProUGUI>();

            _text.color = centralUiController.applicationDesign.GetColor(textColor);
            _text.font = centralUiController.applicationDesign.GetFont(textStyle);
            _placeholder.color = centralUiController.applicationDesign.GetColor(textColor);
            _placeholder.font = centralUiController.applicationDesign.GetFont(placeholderStyle);
            _textArea.color = centralUiController.applicationDesign.GetColor(backgroundColor);
        }
    }
}