using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace UI.MagicUI {
    public class MagicUiButtonSymbol : MagicUiComponent, IPointerEnterHandler, IPointerExitHandler {
        [Header("Button")]
        public Color buttonNormalColor;
        public Color buttonHoverColor;

        [Header("Symbol")]
        public Color symbolNormalColor;
        public Color symbolHoverColor;

        public Sprite symbolA;
        public Sprite symbolB;
        private bool _currentSymbolIsA = true;

        [Header("Animation Parameters")]
        public float fadeDuration = 0.5f;

        private Coroutine _currentSymbolCoroutine;
        private Coroutine _currentImageCoroutine;

        private Color _startColorImage;
        private Color _startColorSymbol;
        
        private Image _fillImage;
        private Image _outlineImage;
        private Image _symbol;

        protected override void OnSkinUI() {
            base.OnSkinUI();
        }

        public void Start() {
            _fillImage = GetComponent<Image>();
            _outlineImage = transform.GetChild(0).GetComponent<Image>();
            _symbol = transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Image>();

            _fillImage.color = buttonNormalColor;
            _symbol.color = symbolNormalColor;
            _outlineImage.color = buttonHoverColor;
        }

        private void OnValidate() {
            _fillImage = GetComponent<Image>();
            _outlineImage = transform.GetChild(0).GetComponent<Image>();
            _symbol = transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Image>();

            _fillImage.color = buttonNormalColor;
            _symbol.color = symbolNormalColor;
            _outlineImage.color = buttonHoverColor;
        }

        public void OnPointerEnter(PointerEventData eventData) {
            EnsureCoroutineStopped(ref _currentImageCoroutine);
            EnsureCoroutineStopped(ref _currentSymbolCoroutine);
            _startColorImage = _fillImage.color;
            _startColorSymbol = _symbol.color;
            _currentImageCoroutine = CreateAnimationRoutine(fadeDuration, ChangeImageColorToHover, null);
            _currentSymbolCoroutine = CreateAnimationRoutine(fadeDuration, ChangeSymbolColorToHover, null);
        }

        public void OnPointerExit(PointerEventData eventData) {
            EnsureCoroutineStopped(ref _currentImageCoroutine);
            EnsureCoroutineStopped(ref _currentSymbolCoroutine);
            _startColorImage = _fillImage.color;
            _startColorSymbol = _symbol.color;
            _currentImageCoroutine = CreateAnimationRoutine(fadeDuration, ChangeImageColorToNormal, null);
            _currentSymbolCoroutine = CreateAnimationRoutine(fadeDuration, ChangeSymbolColorToNormal, null);
        }

        private void ChangeImageColorToNormal(float percent) {
            _fillImage.color = Color.Lerp(_startColorImage, buttonNormalColor, percent);
        }
        
        private void ChangeImageColorToHover(float percent) {
            _fillImage.color = Color.Lerp(_startColorImage, buttonHoverColor, percent);
        }

        private void ChangeSymbolColorToNormal(float percent) {
            _symbol.color = Color.Lerp(_startColorSymbol, symbolNormalColor, percent);
        }
        
        private void ChangeSymbolColorToHover(float percent) {
            _symbol.color = Color.Lerp(_startColorSymbol, symbolHoverColor, percent);
        }

        public void ChangeSymbol() {
            if (_currentSymbolIsA) {
                _symbol.sprite = symbolB;
                _currentSymbolIsA = false;
            } else {
                _symbol.sprite = symbolA;
                _currentSymbolIsA = true;
            }
        }
    }
}