using System;
using System.Diagnostics.CodeAnalysis;
using TMPro;
using UnityEngine;

namespace UI.Main_Menu_Rework.Utils {
    
    [CreateAssetMenu(menuName = "OpenVis/ApplicationDesign")]
    [SuppressMessage("ReSharper", "ConvertSwitchStatementToSwitchExpression")]
    public class ApplicationDesign : ScriptableObject {
        [Header("Colors")]
        public Color textDark;
        public Color textLight;

        public Color mainDark;
        public Color mainLight;

        public Color iconDark;

        public Color highlightA;
        public Color highlightB;
        public Color highlightC;
        public Color highlightD;
        public Color highlightE;

        [Header("Text Styles")]
        public TMP_FontAsset standard;
        public TMP_FontAsset bold;
        public TMP_FontAsset light;
        public TMP_FontAsset italic;

        public Color GetColor(ApplicationColor type) {
            switch (type) {
                case ApplicationColor.MainDark:
                    return mainDark;
                case ApplicationColor.MainLight:
                    return mainLight;
                case ApplicationColor.TextDark:
                    return textDark;
                case ApplicationColor.TextLight:
                    return textLight;
                case ApplicationColor.IconDark:
                    return iconDark;
                case ApplicationColor.HighlightA:
                    return highlightA;
                case ApplicationColor.HighlightB:
                    return highlightB;
                case ApplicationColor.HighlightC:
                    return highlightC;
                case ApplicationColor.HighlightD:
                    return highlightD;
                case ApplicationColor.HighlightE:
                    return highlightE;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public TMP_FontAsset GetFont(TextStyle style) {
            switch (style) {
                case TextStyle.Standard:
                    return standard;
                case TextStyle.Bold:
                    return bold;
                case TextStyle.Light:
                    return light;
                case TextStyle.Italic:
                    return italic;
                default:
                    throw new ArgumentOutOfRangeException(nameof(style), style, null);
            }
        }
    }
    
    public enum ApplicationColor {
        MainDark, MainLight, TextDark, TextLight, IconDark, HighlightA, HighlightB, HighlightC, HighlightD, HighlightE
    }

    public enum TextStyle {
        Standard, Bold, Light, Italic
    }
}
   
