using System;
using TMPro;
using UI.Main_Menu.Utils;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI.SidePanel {
    
    
    [RequireComponent(typeof(Image))]
    public abstract class SidePanelEntry : MonoBehaviour {
        
        private TMP_Text _titleText;
        private Image _image;

        protected virtual void Awake() {
            _titleText = transform.Find("Title").GetComponent<TMP_Text>();
            _image = GetComponent<Image>();
        }

        public void Initialize(string title, float alpha) {
            var tmpColor = _image.color;
            _image.color = tmpColor.WithAlpha(alpha);
            
            _titleText.SetText(title);
        }
        
        public abstract void TriggerUpdate();

    }
}