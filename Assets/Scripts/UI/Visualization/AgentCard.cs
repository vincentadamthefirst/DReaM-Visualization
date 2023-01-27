using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Visualization.Agents;

namespace UI {
    public class AgentCard : MonoBehaviour {
        public RectTransform Parent { get; set; }
        public Agent Agent { get; set; }
        
        public event EventHandler CardClicked;
        
        private RectTransform _mainObject;
        private TextMeshProUGUI _text;
        private Image _mainImage;
        private Color _normalColor;
        private Color _highlightedColor;

        public void Awake() {
            _mainObject = GetComponent<RectTransform>();
            _text = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            _mainImage = GetComponent<Image>();
            
            _mainObject.localScale = Vector3.one * 0.7f;
            _mainImage.color = _normalColor;
            
            SetColor(Agent.StaticData.ColorMaterial.color);
            _text.text = Agent.name.Split(new [] {" ["}, StringSplitOptions.None)[0];
        }

        private void SetColor(Color color) {
            _mainImage.color = new Color(color.r, color.g, color.b, 0.9f);
            _highlightedColor = new Color(color.r, color.g, color.b, 0.9f);
            _normalColor = new Color(color.r, color.g, color.b, 0.7f);
        }

        public void TargetStatusChanged(object sender, bool value) {
            _mainObject.localScale = value ? Vector3.one * 0.9f : Vector3.one * 0.7f;
            _mainImage.color = value ? _highlightedColor : _normalColor;
            LayoutRebuilder.ForceRebuildLayoutImmediate(Parent);
        }

        public void Clicked() {
            CardClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}