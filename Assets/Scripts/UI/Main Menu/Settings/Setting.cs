using UnityEngine;

namespace UI.Main_Menu.Settings {
    public class Setting : MonoBehaviour {
        public RectTransform spacer;

        private int _spacing;

        public void SetSpacing(int width) {
            _spacing = width;
            if (spacer != null)
                spacer.sizeDelta = new Vector2(width, 1);
        }

        public int GetSpacing() {
            return _spacing;
        }
    }
}