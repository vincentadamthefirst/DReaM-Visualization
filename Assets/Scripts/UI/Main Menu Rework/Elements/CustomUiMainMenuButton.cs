using System;
using UI.Main_Menu_Rework.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Main_Menu_Rework.Elements {

    public delegate void Notify();
    
    public class CustomUiMainMenuButton : CustomUiElement, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
        
        // EVENTS
        public event Notify ButtonClicked;
        
        // UNITY EDITOR values
        [Tooltip("The base color for icon and marker")]
        public ApplicationColor baseColor;
        [Tooltip("The hover and select color for icon and marker")]
        public ApplicationColor hoverColor;
        [Tooltip("The alpha value for the hovered button")]
        public float hoveredAlpha = .4f;
        [Tooltip("The alpha value for the selected button")]
        public float selectedAlpha = .8f;
        [Tooltip("If the button is active")]
        public bool isSelected;

        // INTERNAL values
        // the current progress (percent)
        private float _currentProgress;
        // images that make up this button
        private Image _marker, _icon;
        // currently active animation
        private Coroutine _currentAnimation;
        // the colors to be used
        private Color _baseColor, _hoverColor;

        private bool _isMouseOver;

        public override void UpdateUiElement() {
            _marker = transform.Find("Marker").GetComponent<Image>();
            _icon = transform.Find("Icon").GetComponent<Image>();

            _baseColor = applicationDesign.GetColor(baseColor);
            _hoverColor = applicationDesign.GetColor(hoverColor);

            SetSelected(isSelected);
        }
        
        public void SetSelected(bool value) {
            isSelected = value;
            EnsureCoroutineStopped(ref _currentAnimation);
            
            if (!isSelected) {
                // no longer selected
                if (_isMouseOver) {
                    // mouse is over the button, change to hover color
                    UpdateImageColorToHover(1);
                } else {
                    // mouse not over the button, change to base color
                    UpdateImageColorToBase(1);
                }
            } else {
                UpdateImageColorToSelected();
            }
        }

        private void UpdateImageColorToSelected() {
            _marker.color = _hoverColor.WithAlpha(selectedAlpha);
            _icon.color = _hoverColor.WithAlpha(selectedAlpha);
        }

        private void UpdateImageColorToHover(float progress) {
            _marker.color = Color.Lerp(new Color(0, 0, 0, 0), _hoverColor.WithAlpha(hoveredAlpha), progress);
            _icon.color = Color.Lerp(_baseColor, _hoverColor.WithAlpha(hoveredAlpha), progress);
            _currentProgress = progress;
        }
        
        private void UpdateImageColorToBase(float progress) {
            _marker.color = Color.Lerp(new Color(0, 0, 0, 0), _hoverColor.WithAlpha(hoveredAlpha), 1 - progress);
            _icon.color = Color.Lerp(_baseColor, _hoverColor.WithAlpha(hoveredAlpha), 1 - progress);
            _currentProgress = 1 - progress;
        }

        public void OnPointerClick(PointerEventData eventData) {
            if (isSelected) return;
            ButtonClicked?.Invoke();
            SetSelected(true);
        }

        public void OnPointerEnter(PointerEventData eventData) {
            _isMouseOver = true;
            if (isSelected) return;
            EnsureCoroutineStopped(ref _currentAnimation);
            _currentAnimation = CreateAnimationRoutine(.5f, 0f, UpdateImageColorToHover, null);
        }

        public void OnPointerExit(PointerEventData eventData) {
            _isMouseOver = false;
            if (isSelected) return;
            EnsureCoroutineStopped(ref _currentAnimation);
            _currentAnimation = CreateAnimationRoutine(.5f, 0f, UpdateImageColorToBase, null);
        }
    }
}