using System;
using UI.Main_Menu_Rework.Elements;
using UnityEngine;

namespace UI.Main_Menu_Rework.Utils {
    
    
    [RequireComponent(typeof(RectTransform))]
    public class CustomPadding : CustomUiElement {

        [Range(0, 1920)]
        public int sizeX;
        [Range(0, 1080)]
        public int sizeY;

        // this paddings rectTransform
        private RectTransform _rectTransform;

        public override void UpdateUiElement() {
            _rectTransform = GetComponent<RectTransform>();
            _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, sizeX);
            _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sizeY);
        }
    }
}