using UnityEngine;
using UnityEngine.UI;

namespace UI.Main_Menu.Settings {
    
    [RequireComponent(typeof(Image))]
    public class Ruler : Setting<string> {
        
        private Image _ruler;
        private int _thickness;

        public void SetThickness(int thickness) {
            _thickness = thickness;
        }
        
        public void Start() {
            _ruler = GetComponent<Image>();
            var currentSizeDelta = _ruler.rectTransform.sizeDelta;
            _ruler.rectTransform.sizeDelta = new Vector2(currentSizeDelta.x, _thickness);
        }

        public override void StoreData() { }
        public override void LoadData() { }
        public override void SetInfo(params string[] infos) { }
    }
}