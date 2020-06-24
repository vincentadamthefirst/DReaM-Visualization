using UnityEngine;

namespace Scenery.RoadNetwork.RoadGeometries {
    public class Poly3Geometry : RoadGeometry {
        public Poly3Geometry(float sStart, float x, float y, float hdg, float length) : base(sStart, x, y, hdg, length) { }
        public override Vector2 Evaluate(float s, float t) {
            throw new System.NotImplementedException();
        }
    }
}