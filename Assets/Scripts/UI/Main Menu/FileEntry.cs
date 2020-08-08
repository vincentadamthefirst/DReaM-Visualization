using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class FileEntry : MonoBehaviour {

        /// <summary>
        /// If this File Entry is currently selected
        /// </summary>
        public bool IsSelected => _toggle.isOn;

        private TextMeshProUGUI _nameText;
        private TextMeshProUGUI _detailsText;
        private TextMeshProUGUI _typeText;

        private Image _toggleImage;

        private Toggle _toggle;
        
        private void Awake() {
            _toggleImage = transform.GetChild(0).GetComponent<Image>();
            _toggle = transform.GetChild(1).GetComponent<Toggle>();
            _nameText = transform.GetChild(2).GetComponent<TextMeshProUGUI>();
            _detailsText = transform.GetChild(3).GetComponent<TextMeshProUGUI>();
            _typeText = transform.GetChild(4).GetComponent<TextMeshProUGUI>();
            
            _toggle.onValueChanged.AddListener(ToggleValueChanged);
        }

        public void SetName(string newName) {
            _nameText.SetText(newName);
        }

        public void SetDetails(string details) {
            _detailsText.SetText(details);
        }

        public void SetType(string type) {
            _typeText.SetText(type);
        }

        public void SetSelected(bool value) {
            _toggle.SetIsOnWithoutNotify(value);
            _toggleImage.color = value ? new Color(1, 1, 1, .1f) : new Color(1, 1, 1, 0);
        }

        private void ToggleValueChanged(bool value) {
            _toggleImage.color = value ? new Color(1, 1, 1, .1f) : new Color(1, 1, 1, 0);
        }
    }
}
