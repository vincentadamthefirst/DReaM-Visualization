using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Utils.AdditionalMath {
    public static class ConvexHull {

        public static Vector2[] GrahamScanCompute(Vector2[] initialPoints) {
            return GrahamScanCompute(initialPoints.ToList()).ToArray();
        }
        
        public static List<Vector2> GrahamScanCompute(List<Vector2> initialPoints) {
            if (initialPoints.Count < 2) 
                return initialPoints;

            var iMin = Enumerable.Range(0, initialPoints.Count).Aggregate((jMin, jCur) => {
                if (initialPoints[jCur].y < initialPoints[jMin].y)
                    return jCur;
                if (initialPoints[jCur].y > initialPoints[jMin].y)
                    return jMin;
                if (initialPoints[jCur].x < initialPoints[jMin].x)
                    return jCur;
                return jMin;
            });

            var sortQuery = Enumerable.Range(0, initialPoints.Count)
                .Where((i) => (i != iMin))
                .Select((i) =>
                    new KeyValuePair<double, Vector2>(
                        Math.Atan2(initialPoints[i].y - initialPoints[iMin].y,
                            initialPoints[i].x - initialPoints[iMin].x), initialPoints[i]))
                .OrderBy((pair) => pair.Key)
                .Select((pair) => pair.Value);
            var points = new List<Vector2>(initialPoints.Count) {initialPoints[iMin]};
            points.AddRange(sortQuery);

            var M = 0;
            for (int i = 1, N = points.Count; i < N ; i++) {
                var keepNewPoint = true;
                if (M == 0) {
                    keepNewPoint = !(Mathf.Approximately(points[0].x, points[i].x) &&
                                     Mathf.Approximately(points[0].y, points[i].y));
                } else {
                    while (true) {
                        var flag = ConvexHelper.WhichToRemoveFromBoundary(points[M - 1], points[M], points[i]);
                        if (flag == RemovalFlag.None) break;
                        else if (flag == RemovalFlag.MidPoint) {
                            if (M > 0)
                                M--;
                            if (M == 0) 
                                break;
                        } else if (flag == RemovalFlag.EndPoint) {
                            keepNewPoint = false;
                            break;
                        }
                    }
                }

                if (!keepNewPoint) continue;
                M++;
                ConvexHelper.Swap(points, M, i);
            }
            
            points.RemoveRange(M + 1, points.Count - M - 1);
            return points;
        }
    }
}