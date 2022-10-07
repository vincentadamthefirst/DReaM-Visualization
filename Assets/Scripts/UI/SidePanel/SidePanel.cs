using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Visualization.Agents;

namespace UI.SidePanel {
    public class SidePanel : MonoBehaviour {

        public TMP_Text agentText;
        public Button openButton;
        public RectTransform content;

        private bool _currentOpenStatus;
        private List<SidePanelEntry> _entries = new List<SidePanelEntry>();

        private LTDescr _currentAnimation;

        private void Start() {
            openButton.onClick.AddListener(OpenClose);
        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.Tab)) {
                OpenClose();
            }
        }

        public void Setup(string[] titles) {
            for (var i = 0; i < titles.Length; i++) {
                var title = titles[i];
                
                var sidePanelEntry = Resources.Load<SidePanelEntry>("Prefabs/UI/SidePanel/TextEntry");
                var newObject = Instantiate(sidePanelEntry, content);
                
                newObject.Setup(title, i % 2 == 0 ? .4f : .2f);
                _entries.Add(newObject);
            }
        }

        public void UpdateTexts(Agent agent, object[] infos) {
            for (var i = 0; i < infos.Length; i++) {
                var element = infos[i];
                switch (element) {
                    case float f:
                        ((TextEntry) _entries[i]).UpdateText(f.ToString("0.000"));
                        break;
                    case string _:
                        ((TextEntry) _entries[i]).UpdateText((string) infos[i]);
                        break;
                }
            }
            
            agentText.SetText($"Agent ID: {agent.Id}");
        }

        private void OpenClose() {
            var moveTime = .3f;
            
            if (_currentAnimation != null) {
                moveTime = _currentAnimation.passed;
                LeanTween.cancel(_currentAnimation.id);
            }

            _currentOpenStatus = !_currentOpenStatus;
            LeanTween.move((RectTransform) transform, new Vector3(_currentOpenStatus ? 0 : -290, -10, 0), moveTime);
        }

    }
}