﻿using UnityEngine;
using Utils;

namespace Scenery.RoadNetwork.RoadGeometries {
    
    /// <summary>
    /// Class representing a LineGeometry from OpenDrive
    /// </summary>
    public class LineGeometry : RoadGeometry {
        
        public LineGeometry(float sStart, float x, float y, float hdg, float length) : base(sStart, x,
            y, hdg, length) { }

        public static Vector2 GetLine(float s, float t, float x, float y, float hdg) {
            var offset = new Vector2(s, t);
            offset.RotateRadians(hdg);
            offset.x += x;
            offset.y += y;

            return offset;
        }
        
        public override Vector2 Evaluate(float s, float t) {
            return GetLine(s, t, x, y, hdg);
        }

        public override float EvaluateHeading(float s) {
            return hdg;
        }
    }
}