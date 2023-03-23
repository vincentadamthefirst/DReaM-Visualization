using System.Collections.Generic;
using UnityEngine;

namespace Utils.AdditionalMath {
    internal static class ConvexHelper {
        internal static RemovalFlag WhichToRemoveFromBoundary(Vector3 p1, Vector3 p2, Vector3 p3) {
            var cross = CCW(p1, p2, p3);
            switch (cross) {
                // Remove p2
                case < 0:
                    return RemovalFlag.MidPoint;
                // Remove none.
                case > 0:
                    return RemovalFlag.None;
            }

            // Check for being reversed using the dot product off the difference vectors.
            var dotp = (p3.x - p2.x) * (p2.x - p1.x) + (p3.y - p2.y) * (p2.y - p1.y);
            if (Mathf.Approximately(dotp, 0.0f))
                // Remove p2
                return RemovalFlag.MidPoint;
            return dotp < 0 ? RemovalFlag.EndPoint : RemovalFlag.MidPoint;
        }

        private static float CCW(Vector3 p1, Vector3 p2, Vector3 p3) {
            // Compute (p2 - p1) X (p3 - p1)
            var cross1 = (p2.x - p1.x) * (p3.y - p1.y);
            var cross2 = (p2.y - p1.y) * (p3.x - p1.x);
            if (Mathf.Approximately(cross1, cross2))
                return 0;
            return cross1 - cross2;
        }

        internal static void Swap<T>(IList<T> list, int i, int j) {
            if (i == j) return;
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    internal enum RemovalFlag {
        None,
        MidPoint,
        EndPoint
    }
}