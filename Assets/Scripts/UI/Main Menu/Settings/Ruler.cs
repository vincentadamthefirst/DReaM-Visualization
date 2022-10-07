using UnityEngine;
using UnityEngine.UI;

namespace UI.Main_Menu.Settings {
    
    [RequireComponent(typeof(Image))]
    public class Ruler : Setting {
        
        private Image _ruler;
        private int _thickness;
        private Color _color;

        public void SetData(int thickness, Color color) {
            _thickness = thickness;
            _color = color;
        }
        
        public void Start() {
            _ruler = GetComponent<Image>();
            var currentSizeDelta = _ruler.rectTransform.sizeDelta;
            _ruler.rectTransform.sizeDelta = new Vector2(currentSizeDelta.x, _thickness);
            _ruler.color = _color;
        }
    }
}