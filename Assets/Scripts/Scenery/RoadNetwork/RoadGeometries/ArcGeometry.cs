using UnityEngine;
using UnityEngine.Rendering.UI;
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
        
        public override Vector2 Evaluate(float s, float t) {
            var radius = 1f / _curvature;
            var circumference = 2f * Mathf.PI / _curvature;

            var fractionRad = (s % circumference) * _curvature;
            var offset = new Vector2(0f, -radius + t);
            offset.RotateRadians(fractionRad);

            offset.y += radius;
            offset.RotateRadians(hdg);

            offset.x += x;
            offset.y += y;

            return offset;
        }

        public override float EvaluateHeading(float s) {
            var circumference = 2f * Mathf.PI / _curvature;

            var fractionRad = (s % circumference) * _curvature;
            return hdg + fractionRad;
        }
    }
}