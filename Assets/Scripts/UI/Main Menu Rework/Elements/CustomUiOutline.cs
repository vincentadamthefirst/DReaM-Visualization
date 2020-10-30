using UI.Main_Menu_Rework.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Main_Menu_Rework.Elements {
    
    [RequireComponent(typeof(Outline))]
    public class CustomUiOutline : CustomUiElement {

        private Outline _outline;
        
        [Tooltip("The color to be used")]
        public ApplicationColor color;

        [Tooltip("The opacity for the image")]
        [Range(0, 1)]
        public float opacity = 1f;
        
        public override void UpdateUiElement() {
            _outline = GetComponent<Outline>();

            _outline.effectColor = centralUiController.applicationDesign.GetColor(color).WithAlpha(opacity);
        }
    }
}