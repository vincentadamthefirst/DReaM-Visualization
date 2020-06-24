using System.Collections.Generic;
using UnityEngine;

namespace Utils.AdditionalMath {
    internal class ConvexHelper {
        internal static RemovalFlag WhichToRemoveFromBoundary(Vector3 p1, Vector3 p2, Vector3 p3) {
            var cross = CCW(p1, p2, p3);
            if (cross < 0)
                // Remove p2
                return RemovalFlag.MidPoint;
            if (cross > 0)
                // Remove none.
                return RemovalFlag.None;
            // Check for being reversed using the dot product off the difference vectors.
            var dotp = (p3.x - p2.x) * (p2.x - p1.x) + (p3.y - p2.y) * (p2.y - p1.y);
            if (Mathf.Approximately(dotp, 0.0f))
                // Remove p2
                return RemovalFlag.MidPoint;
            if (dotp < 0)
                // Remove p3
                return RemovalFlag.EndPoint;
            else
                // Remove p2
                return RemovalFlag.MidPoint;
        }
        
        private static float CCW(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            // Compute (p2 - p1) X (p3 - p1)
            var cross1 = (p2.x - p1.x) * (p3.y - p1.y);
            var cross2 = (p2.y - p1.y) * (p3.x - p1.x);
            if (Mathf.Approximately(cross1, cross2))
                return 0;
            return cross1 - cross2;
        }
        
        internal static void Swap<T>(IList<T> list, int i, int j)
        {
            if (i == j) return;
            var temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
    
    internal enum RemovalFlag {
        None,
        MidPoint,
        EndPoint
    };
}