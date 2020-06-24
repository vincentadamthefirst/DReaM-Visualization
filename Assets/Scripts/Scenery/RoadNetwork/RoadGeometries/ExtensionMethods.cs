using UnityEngine;

namespace Scenery.RoadNetwork.RoadGeometries {
    public static class ExtensionMethods {
        public static void RotateRadians(this ref Vector2 v, float angle) {
            var cosAngle = Mathf.Cos(angle);
            var sinAngle = Mathf.Sin(angle);
            var tmpX = v.x * cosAngle - v.y * sinAngle;
            v.y = v.x * sinAngle + v.y * cosAngle;
            v.x = tmpX;
        }
    }
}