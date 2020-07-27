using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Visualization.OcclusionManagement;

namespace UI {
    public class AgentCard : MonoBehaviour {

        public TargetController TargetController { get; set; }
        
        public RectTransform Parent { get; set; }
        
        private RectTransform _mainObject;

        private TextMeshProUGUI _text;

        private Image _mainImage;

        private Color _normalColor;
        private Color _highlightedColor;

        public void Awake() {
            _mainObject = GetComponent<RectTransform>();
            _text = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            _mainImage = GetComponent<Image>();
        }

        public void SetText(string text) {
            _text.text = text;
        }

        public void SetColor(Color color) {
            _mainImage.color = new Color(color.r, color.g, color.b, 0.9f);
            _highlightedColor = new Color(color.r, color.g, color.b, 0.9f);
            _normalColor = new Color(color.r, color.g, color.b, 0.7f);
        }

        public void SetIsTarget(bool value) {
            _mainObject.localScale = value ? Vector3.one : Vector3.one * 0.8f;
            _mainImage.color = value ? _highlightedColor : _normalColor;
            LayoutRebuilder.ForceRebuildLayoutImmediate(Parent);
        }

        public void Clicked() {
            TargetController.HandleCardClick(this);
        }
    }
}