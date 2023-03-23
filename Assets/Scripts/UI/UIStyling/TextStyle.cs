using TMPro;
using UnityEngine;

namespace UI.UIStyling {
    
    [RequireComponent(typeof(TMP_Text))]
    public class TextStyle : UIStyle {
        public FontWeight fontWeight = FontWeight.Regular;
        public ColorStyle textColor = ColorStyle.TextA;
        public TextSize textSize = TextSize.B;
        
        public override UIStyleElementType GetElementType() => UIStyleElementType.Text;
    }
}