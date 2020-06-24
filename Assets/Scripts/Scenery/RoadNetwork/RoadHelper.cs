using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Scenery.RoadNetwork {
    public static class RoadHelper {
        
        public static void AddLinePoints(ref List<Vector3> points, ref List<int> triangles, Road road, float sStart,
            float sEnd, float tSmall, float tLarge, bool reverseTriangles) {
            var i = points.Count;

            points.AddRange(new[] {
                road.EvaluatePoint(sStart, tSmall),
                road.EvaluatePoint(sStart, tLarge),
                road.EvaluatePoint(sEnd, tSmall),
                road.EvaluatePoint(sEnd, tLarge)
            });

            triangles.AddRange(reverseTriangles
                ? new[] {i, i + 2, i + 3, i, i + 3, i + 1}
                : new[] {i, i + 3, i + 2, i, i + 1, i + 3}
            );
        }

        public static void AddLinePoints(ref List<Vector3> points, ref List<int> triangles, ref List<Vector2> uvs,
            Road road, float sStart, float sEnd, float tSmall, float tLarge, bool reverseTriangles) {
            
            AddLinePoints(ref points, ref triangles, road, sStart, sEnd, tSmall, tLarge, reverseTriangles);
            
            uvs.AddRange(new [] {
                new Vector2(0, 0), 
                new Vector2(0, 1), 
                new Vector2(1, 0), 
                new Vector2(1, 1), 
            });
        }
        
        public static void AddLinePoints(ref List<Vector3> points, ref List<int> triangles, ref List<Vector2> uvs,
            Road road, float tSmall, float tLarge, bool reverseTriangles) {
            
            AddLinePoints(ref points, ref triangles, road, 0, road.Length, tSmall, tLarge, reverseTriangles);
            
            uvs.AddRange(new [] {
                new Vector2(0, 0), 
                new Vector2(0, 1), 
                new Vector2(1, 0), 
                new Vector2(1, 1), 
            });
        }
    }
}