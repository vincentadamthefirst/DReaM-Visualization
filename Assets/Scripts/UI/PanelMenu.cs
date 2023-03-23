using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI {

    [Serializable]
    public struct CardMenuElement {
        public RectTransform content;
        public Button button;
    }

    public abstract class PanelMenu : MonoBehaviour {
        public List<CardMenuElement> cards = new();

        public void Start() {
            for (var i = 0; i < cards.Count; i++) {
                var i1 = i;
                cards[i].button.onClick.AddListener(delegate { ButtonPressed(i1); });
                cards[i].content.gameObject.SetActive(i == 0);
            }
            AfterButtonSetup();
        }

        protected abstract void AfterButtonSetup();

        private void ButtonPressed(int cardIndex) {
            for (var i = 0; i < cards.Count; i++) {
                if (i == cardIndex) {
                    cards[i].content.gameObject.SetActive(true);
                    continue;
                }
                cards[i].content.gameObject.SetActive(false);
            }
        }
    }
}