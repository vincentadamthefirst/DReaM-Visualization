using UnityEngine;
using UnityEngine.UI;

namespace UI.Main_Menu.Settings {
    
    [RequireComponent(typeof(Image))]
    public class Ruler : Setting {
        
        private Image _ruler;
        private int _thickness;

        public void SetData(int thickness) {
            _thickness = thickness;
        }
        
        public void Start() {
            _ruler = GetComponent<Image>();
            var currentSizeDelta = _ruler.rectTransform.sizeDelta;
            _ruler.rectTransform.sizeDelta = new Vector2(currentSizeDelta.x, _thickness);
        }
    }
}