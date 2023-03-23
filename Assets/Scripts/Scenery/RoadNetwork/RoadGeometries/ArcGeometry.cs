using UnityEngine;
using Utils;

namespace Scenery.RoadNetwork.RoadGeometries {
    
    /// <summary>
    /// Class representing a ArcGeometry from OpenDrive
    /// </summary>
    public class ArcGeometry : RoadGeometry {
        
        // the parameter
        private readonly float _curvature;
        
        public ArcGeometry(float sStart, float x, float y, float hdg, float length, float curvature) : base(sStart, x,
            y, hdg, length) {

            _curvature = curvature;
        }

        public static Vector2 GetArc(float s, float t, float x, float y, float hdg, float curvature) {
            var radius = 1f / curvature;
            var circumference = 2f * Mathf.PI / curvature;

            var fractionRad = (s % circumference) * curvature;
            var offset = new Vector2(0f, -radius + t);
            offset.RotateRadians(fractionRad);

            offset.y += radius;
            offset.RotateRadians(hdg);

            offset.x += x;
            offset.y += y;

            return offset;
        }
        
        public override Vector2 Evaluate(float s, float t) {
            return GetArc(s, t, x, y, hdg, _curvature);
        }

        public static float GetArgHeading(float s, float hdg, float curvature) {
            var circumference = 2f * Mathf.PI / curvature;

            var fractionRad = (s % circumference) * curvature;
            return hdg + fractionRad;
        }

        public override float EvaluateHeading(float s) {
            return GetArgHeading(s, hdg, _curvature);
        }
    }
}