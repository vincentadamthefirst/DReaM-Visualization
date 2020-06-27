using System;
using System.Runtime.Serialization;
using UnityEngine;

namespace Scenery.RoadNetwork.RoadGeometries {
    public abstract class RoadGeometry {
        protected readonly float sStart;
        protected readonly float x;
        protected readonly float y;
        protected readonly float hdg;
        protected readonly float length;
        
        protected const float Tolerance = 0.000001f;

        public RoadGeometry(float sStart, float x, float y, float hdg, float length) {
            this.sStart = sStart;
            this.x = x;
            this.y = y;
            this.hdg = hdg;
            this.length = length;
        }

        public abstract Vector2 Evaluate(float s, float t);

        public abstract float EvaluateHeading(float s);

        public float Length => length;

        public float SStart => sStart;
    }

    [Serializable]
    public class RoadGeometryGenerationException : Exception {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public RoadGeometryGenerationException() { }
        public RoadGeometryGenerationException(string message) : base(message) { }
        public RoadGeometryGenerationException(string message, Exception inner) : base(message, inner) { }

        protected RoadGeometryGenerationException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }
}