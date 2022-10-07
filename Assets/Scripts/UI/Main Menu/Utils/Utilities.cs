using UnityEngine;

namespace UI.Main_Menu.Utils {
    public static class Utilities {

        public static Color WithAlpha(this Color color, float alpha) {
            return new Color(color.r, color.g, color.b, alpha);
        }
        
    }
}