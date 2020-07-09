using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.MagicUI {
    public class MagicUiButton : MagicUiComponent, IPointerEnterHandler, IPointerExitHandler {
        [Header("Button")]
        public Color buttonNormalColor;
        public Color buttonHoverColor;

        [Header("Text")]
        public Color textNormalColor;
        public Color textHoverColor;

        [Header("Animation Parameters")]
        public float fadeDuration = 0.5f;

        private Coroutine _currentTextCoroutine;
        private Coroutine _currentImageCoroutine;

        private Color _startColorImage;
        private Color _startColorText;
        
        private Image _fillImage;
        private Image _outlineImage;
        private TextMeshProUGUI _text;

        protected override void OnSkinUI() {
            base.OnSkinUI();
        }

        public void Start() {
            _fillImage = GetComponent<Image>();
            _outlineImage = transform.GetChild(0).GetComponent<Image>();
            _text = transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();

            _fillImage.color = buttonNormalColor;
            _text.color = textNormalColor;
            _outlineImage.color = buttonHoverColor;
        }

        private void OnValidate() {
            _fillImage = GetComponent<Image>();
            _outlineImage = transform.GetChild(0).GetComponent<Image>();
            _text = transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();

            _fillImage.color = buttonNormalColor;
            _text.color = textNormalColor;
            _outlineImage.color = buttonHoverColor;
        }

        public void OnPointerEnter(PointerEventData eventData) {
            EnsureCoroutineStopped(ref _currentImageCoroutine);
            EnsureCoroutineStopped(ref _currentTextCoroutine);
            _startColorImage = _fillImage.color;
            _startColorText = _text.color;
            _currentImageCoroutine = CreateAnimationRoutine(fadeDuration, ChangeImageColorToHover, null);
            _currentTextCoroutine = CreateAnimationRoutine(fadeDuration, ChangeTextColorToHover, null);
        }

        public void OnPointerExit(PointerEventData eventData) {
            EnsureCoroutineStopped(ref _currentImageCoroutine);
            EnsureCoroutineStopped(ref _currentTextCoroutine);
            _startColorImage = _fillImage.color;
            _startColorText = _text.color;
            _currentImageCoroutine = CreateAnimationRoutine(fadeDuration, ChangeImageColorToNormal, null);
            _currentTextCoroutine = CreateAnimationRoutine(fadeDuration, ChangeTextColorToNormal, null);
        }

        private void ChangeImageColorToNormal(float percent) {
            _fillImage.color = Color.Lerp(_startColorImage, buttonNormalColor, percent);
        }
        
        private void ChangeImageColorToHover(float percent) {
            _fillImage.color = Color.Lerp(_startColorImage, buttonHoverColor, percent);
        }

        private void ChangeTextColorToNormal(float percent) {
            _text.color = Color.Lerp(_startColorText, textNormalColor, percent);
        }
        
        private void ChangeTextColorToHover(float percent) {
            _text.color = Color.Lerp(_startColorText, textHoverColor, percent);
        }
    }
}
