using System.Collections.Generic;
using UnityEngine;

namespace Utils {
    public class ColorComparer : IComparer<Color> {
        public int Compare(Color a, Color b) {
            if (a.r < b.r)
                return 1;
            else if (a.r > b.r)
                return -1;
            else {
                if (a.g < b.g)
                    return 1;
                else if (a.g > b.g)
                    return -1;
                else {
                    if (a.b < b.b)
                        return 1;
                    else if (a.b > b.b)
                        return -1;
                }
            }

            return 0;
        }
    }
}