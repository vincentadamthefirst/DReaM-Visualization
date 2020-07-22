using System;
using System.Runtime.Serialization;
using UnityEngine;

namespace Scenery.RoadNetwork.RoadGeometries {
    
    /// <summary>
    /// Abstract class representing a Road Geometry of OpenDrive. 
    /// </summary>
    public abstract class RoadGeometry {
        public float SStart { get; }
        protected readonly float x;
        protected readonly float y;
        protected readonly float hdg;
        public float Length { get; }

        protected const float Tolerance = 0.000001f;

        protected RoadGeometry(float sStart, float x, float y, float hdg, float length) {
            SStart = sStart;
            this.x = x;
            this.y = y;
            this.hdg = hdg;
            Length = length;
        }

        /// <summary>
        /// Evaluates this geometry at a given s and t coordinate.
        /// </summary>
        /// <param name="s">The s value</param>
        /// <param name="t">The t value</param>
        /// <returns>The calculated point in the world at the given location along the road</returns>
        public abstract Vector2 Evaluate(float s, float t);

        /// <summary>
        /// Evaluates this geometries heading at a given s coordinate.
        /// </summary>
        /// <param name="s">The s value</param>
        /// <returns>The calculated heading at the given location along the road</returns>
        public abstract float EvaluateHeading(float s);
    }

    /// <summary>
    /// Exception if anything goes wrong when calculating
    /// </summary>
    [Serializable]
    public class RoadGeometryGenerationException : Exception {
        public RoadGeometryGenerationException() { }
        public RoadGeometryGenerationException(string message) : base(message) { }
        public RoadGeometryGenerationException(string message, Exception inner) : base(message, inner) { }

        protected RoadGeometryGenerationException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }
}