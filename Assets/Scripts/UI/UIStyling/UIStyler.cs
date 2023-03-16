using System;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

namespace UI.UIStyling {

    public enum ColorStyle {
        None,
        Main,
        Secondary,
        Tertiary,
        TextA,
        TextB
    }

    public enum TextSize {
        None,
        A,
        B,
        C
    }

    public enum UIStyleElementType {
        Graphic,
        Text
    }

    [ExecuteInEditMode]
    public class UIStyler : MonoBehaviour {
        public static UIStyler Instance { get; private set; }
        
        [Header("Fonts")] 
        public TMP_FontAsset regular;
        public TMP_FontAsset medium;
        public TMP_FontAsset bold;
        public TMP_FontAsset light;
        public TMP_FontAsset thin;
        
        [Header("Colors")]
        public Color mainColor;
        public Color secondaryColor;
        public Color tertiaryColor;
        public Color textAColor;
        public Color textBColor;

        [Header("Text Sizes")] 
        public int sizeA = 35;
        public int sizeB = 25;
        public int sizeC = 20;

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(this);
            } else {
                Instance = this;
            }
            UpdateAll();
        }
        
        /**
         * Calls the update functions for all active UIStyle objects in the scene.
         */
        private void UpdateAll() {
            var uiStyles = FindObjectsOfType<UIStyle>();
            foreach (var uiStyle in uiStyles) {
                SingleUpdate(uiStyle);
            }
        }

        /**
         * Updates a single UIStyle.
         */
        public void SingleUpdate(UIStyle uiStyle) {
            switch (uiStyle.GetElementType()) {
                case UIStyleElementType.Graphic:
                    SingleUpdate((ImageStyle) uiStyle);
                    break;
                case UIStyleElementType.Text:
                    SingleUpdate((TextStyle) uiStyle);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SingleUpdate(ImageStyle imageStyle) {
            if (imageStyle.imageColor == ColorStyle.None)
                return;
            imageStyle.GetComponent<Graphic>().color = GetColorByStyle(imageStyle.imageColor);
        }

        private void SingleUpdate(TextStyle textStyle) {
            if (textStyle.textColor != ColorStyle.None)
                textStyle.GetComponent<TMP_Text>().color = GetColorByStyle(textStyle.textColor);
            textStyle.GetComponent<TMP_Text>().font = GetFontAssetByWeight(textStyle.fontWeight);
            if (textStyle.textSize != TextSize.None)
                textStyle.GetComponent<TMP_Text>().fontSize = GetTextSize(textStyle.textSize);
        }

        private TMP_FontAsset GetFontAssetByWeight(FontWeight weight) {
            // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
            return weight switch {
                FontWeight.Thin => thin,
                FontWeight.Regular => regular,
                FontWeight.Medium => medium,
                FontWeight.Bold => bold,
                FontWeight.Light => light,
                _ => throw new ArgumentOutOfRangeException(nameof(weight), weight, "FontWeight not supported.")
            };
        }

        private Color GetColorByStyle(ColorStyle style) {
            return style switch {
                ColorStyle.Main => mainColor,
                ColorStyle.Secondary => secondaryColor,
                ColorStyle.Tertiary => tertiaryColor,
                ColorStyle.TextA => textAColor,
                ColorStyle.TextB => textBColor,
                _ => throw new ArgumentOutOfRangeException(nameof(style), style, null)
            };
        }

        private int GetTextSize(TextSize textSize) {
            return textSize switch {
                TextSize.A => sizeA,
                TextSize.B => sizeB,
                TextSize.C => sizeC,
                _ => throw new ArgumentOutOfRangeException(nameof(textSize), textSize, null)
            };
        }
    }
}