using UnityEngine;
using UnityEngine.UI;

namespace UI.UIStyling {
    
    [RequireComponent(typeof(Graphic))]
    public class ImageStyle : UIStyle {
        public ColorStyle imageColor = ColorStyle.Main;
        
        public override UIStyleElementType GetElementType() => UIStyleElementType.Graphic;
    }
}