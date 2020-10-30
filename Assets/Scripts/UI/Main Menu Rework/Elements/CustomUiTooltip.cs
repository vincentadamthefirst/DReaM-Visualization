using System;
using UI.Main_Menu_Rework.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace UI.Main_Menu_Rework.Elements {
    
    /// <summary>
    /// Can be added to an UI object to enable Tooltips.
    /// </summary>
    public class CustomUiTooltip : CustomUiElement, IPointerEnterHandler, IPointerExitHandler {

        public string tooltip;

        private Vector2 _lastPosition;
        private int _hoverTime;

        private bool _mouseOver;
        
        private void Update() {
            if (!_mouseOver) {
                centralUiController.HideTooltip();
                _hoverTime = 0;
                return;
            }
            
            var position = Input.mousePosition;
            if (Vector2.Distance(_lastPosition, position) < 10) {
                _hoverTime += (int) (Time.deltaTime * 1000);
            } else {
                _hoverTime = 0;
                centralUiController.HideTooltip();
            }

            if (_hoverTime > centralUiController.timeToTooltip) {
                centralUiController.ShowTooltip(tooltip, position + new Vector3(2, 2, 0));
            }

            _lastPosition = position;
        }

        public override void UpdateUiElement() { }
        
        public void OnPointerEnter(PointerEventData eventData) {
            _mouseOver = true;
        }

        public void OnPointerExit(PointerEventData eventData) {
            _mouseOver = false;
        }
    }
}