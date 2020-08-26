using System.Collections.Generic;
using System.Linq;
using TMPro;
using UI.Main_Menu_Rework.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Main_Menu_Rework.Elements {
    
    [RequireComponent(typeof(Button))]
    public class CustomUiButton : CustomUiElement {
        
        // UNITY EDITOR values
        [Tooltip("The base color for the images")]
        public ApplicationColor imageColor;
        [Tooltip("The base color for the text")]
        public ApplicationColor textColor;
        [Tooltip("The style to be used for the text")]
        public TextStyle textStyle;

        private List<Image> _images;
        private List<TextMeshProUGUI> _texts;
        
        public override void UpdateUiElement() {
            _images = GetComponentsInChildren<Image>().ToList();
            _texts = GetComponentsInChildren<TextMeshProUGUI>().ToList();
            _images.ForEach(x => x.color = applicationDesign.GetColor(imageColor));
            _texts.ForEach(x => x.color = applicationDesign.GetColor(textColor));
            _texts.ForEach(x => x.font = applicationDesign.GetFont(textStyle));
        }
    }
}