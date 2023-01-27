using TMPro;
using UI.Main_Menu.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace UI.SidePanel {
    
    
    [RequireComponent(typeof(Image))]
    public class SidePanelEntry : MonoBehaviour {
        
        public TMP_Text titleText;
        protected Image image;
        
        public void Setup(string title, float alpha) {
            image = GetComponent<Image>();

            var tmpColor = image.color;
            image.color = tmpColor.WithAlpha(alpha);
            
            titleText.SetText(title);
        }

    }
}