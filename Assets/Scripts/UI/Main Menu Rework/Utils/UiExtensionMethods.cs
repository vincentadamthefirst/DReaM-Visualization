using UnityEngine;

namespace UI.Main_Menu_Rework.Utils {
    public static class UiExtensionMethods {

        public static Color WithAlpha(this Color input, float alpha) {
            return new Color(input.r, input.g, input.b, alpha);
        }

        public static void SetActiveChildren(this Transform transform, bool active) {
            foreach (Transform t in transform) {
                t.gameObject.SetActive(active);
            }
        }
    }
}