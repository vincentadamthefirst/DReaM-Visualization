using System;
using System.Collections;
using UI.Main_Menu_Rework.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Main_Menu_Rework.Elements {
    
    [RequireComponent(typeof(Image))]
    public class CustomUiImage : CustomUiElement {

        [Tooltip("The color to be used")]
        public ApplicationColor color;

        [Tooltip("The opacity for the image")]
        [Range(0, 1)]
        public float opacity = 1f;

        // the Image component to be colored
        private Image _background;

        public override void UpdateUiElement() {
            _background = GetComponent<Image>();
            _background.color = applicationDesign.GetColor(color).WithAlpha(opacity);
        }
    }
}