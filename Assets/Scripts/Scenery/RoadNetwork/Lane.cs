using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scenery.RoadNetwork {
    
    [RequireComponent(typeof(Lane))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class Lane : SceneryElement {
        
        /// <summary>
        /// The OpenDrive id of this lane as integer, used for lane sorting from center out
        /// </summary>
        public int LaneId { get; set; }
        
        /// <summary>
        /// The type of this lane.
        /// </summary>
        public LaneType LaneType { get; set; }
        
        /// <summary>
        /// The next neighbor of this lane (towards the center of the road)
        /// </summary>
        public Lane InnerNeighbor { get; set; }
        
        /// <summary>
        /// The next neighbor of this lane (towards the outside of the road) 
        /// Will not be set for center Lane
        /// </summary>
        public Lane OuterNeighbor { get; set; }
        
        /// <summary>
        /// The side of the road that this lane is on (in road direction)
        /// Will not be set for center Lane
        /// </summary>
        public LaneDirection LaneDirection { get; set; }
        
        /// <summary>
        /// The parent LaneSection that contains this lane
        /// </summary>
        public LaneSection Parent { get; set; }
        
        /// <summary>
        /// The id of the successor Lane-object as string, "x" if there is no successor
        /// </summary>
        public string SuccessorId { get; set; }
        
        /// <summary>
        /// The successor Lane-object to this lane, can be null
        /// </summary>
        public Lane Successor { get; set; }
        
        /// <summary>
        /// Reference to the RoadDesign used for texturing
        /// </summary>
        public RoadDesign RoadDesign { get; set; }
        
        /// <summary>
        /// The RoadMark of this Lane-object (towards the outside of the road)
        /// </summary>
        public RoadMark RoadMark { get; set; }
        
        /// <summary>
        /// Multiplier for t-Offset, get calculated in GenerateMesh()
        /// </summary>
        public int Multiplier { get; set; }

        // parameters for width function
        private float _sOffset, _a, _b, _c, _d;

        // Properties for materials
        private static readonly int BumpMap = Shader.PropertyToID("_BumpMap");
        private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
        

        public void SetWidthParameters(float sOffset, float a, float b, float c, float d) {
            _sOffset = sOffset;
            _a = a;
            _b = b;
            _c = c;
            _d = d;
        }

        public float GetAParameter() {
            return _a;
        }

        public float EvaluateWidth(float initialS) {
            var s = _sOffset + initialS + Parent.S;
            
            if (LaneDirection == LaneDirection.Center) return (_a + _b * s + _c * s * s + _d * s * s * s) / 2f;
            
            if (s > Parent.Length) return InnerNeighbor.EvaluateWidth(Parent.Length) + _a + _b * s + _c * s * s + _d * s * s * s;
            
            if (InnerNeighbor != null) return InnerNeighbor.EvaluateWidth(s) + _a + _b * s + _c * s * s + _d * s * s * s;
            else return _a + _b * s + _c * s * s + _d * s * s * s;
        }

        public bool IsConstantWidth() {
            if (InnerNeighbor == null || LaneDirection == LaneDirection.Center)
                return Math.Abs(_b) < Tolerance && Math.Abs(_c) < Tolerance && Math.Abs(_d) < Tolerance;
            
            return Math.Abs(_b) < Tolerance && Math.Abs(_c) < Tolerance && Math.Abs(_d) < Tolerance && InnerNeighbor.IsConstantWidth();
        }

        public float GetMaxWidth(int stepSize) {
            var max = float.NegativeInfinity;
            for (var i = 0f; i < Parent.Length; i += Parent.Length / stepSize) {
                var tmp = EvaluateWidth(i);
                if (tmp > max) max = tmp;
            }
            return max;
        }
        
        public float GetMaxWidthSelf(int stepSize) {
            var max = float.NegativeInfinity;
            for (var i = 0f; i < Parent.Length; i += Parent.Length / stepSize) {
                var tmp = _a + _b * i + _c * i * i + _d * i * i * i;
                if (tmp > max) max = tmp;
            }
            return max;
        }

        public void GenerateMesh() {
            Multiplier = LaneDirection == LaneDirection.Right || LaneDirection == LaneDirection.Center ? -1 : 1;
            if (IsConstantWidth() && Math.Abs(_a) < Tolerance) return;
            if (LaneDirection == LaneDirection.Center) return;

            var mesh = new Mesh();
            RoadHelper.GenerateMeshForLane(ref mesh, this);

            GetComponent<MeshFilter>().mesh = mesh;
            
            AddMaterials();

            RoadMark.GenerateMesh();
        }

        private void AddMaterials() {
            var meshRenderer = GetComponent<MeshRenderer>();

            if (LaneType == LaneType.Sidewalk) {
                var m1 = new Material(RoadDesign.sidewalkCurb);
                var m2 = new Material(RoadDesign.sidewalk);
                var m3 = new Material(RoadDesign.sidewalkCurb);

                var p = m2.GetTextureScale(BaseMap);
                var v1 = new Vector2(Parent.Length * p.x, p.y);
                var v2 = new Vector2(Parent.Length* p.x, GetMaxWidthSelf(RoadDesign.samplePrecision) * p.y);
                
                m1.SetTextureScale(BumpMap, v1);
                m1.SetTextureScale(BaseMap, v1);
                m2.SetTextureScale(BumpMap, v2);
                m2.SetTextureScale(BaseMap, v2);
                m3.SetTextureScale(BumpMap, v1);
                m3.SetTextureScale(BaseMap, v1);
                meshRenderer.materials = new[] {m1, m2, m3};
            } else {
                var material = new Material(RoadDesign.GetMaterialForLaneType(LaneType));
                var p = material.GetTextureScale(BumpMap);
                var v = new Vector2(Parent.Length * p.x, GetMaxWidthSelf(RoadDesign.samplePrecision) * p.y);
                material.SetTextureScale(BumpMap, v);
                material.SetTextureScale(BaseMap, v);
                meshRenderer.material = material;
            }
        }

        // TODO
        // Kennt Parent LaneSection
        // Kennt nächst linkere Lane (nächste Lane zum Zentrum der Straße)
        // Werte: a, b, c, d für width-Berechnung
        // Methode: public Methode um Width zu berechnen
        // Methode: Generierung des Meshes
        //     --> checkt ob LaneSection komplett im Line Segment ist && eigene Breite sich nicht ändert
        //         --> ja: 2 (4) Punkte generieren: am Start und Ende
        //         --> no: mit Genauigkeits-Integer mehrere Punkte generieren (Start & Endpunkt mit einbegriffen)
        //     --> Breite: Eigene Breite + Breite der nächst linkeren Spur and der selben Stelle
        //                 am weitesten in der Mitte liegende Spur hat Center Lane als linke Spur, Breite der
        //                 Center Lane: 1/2 * Breitenfunktion der Center Lane
        //     --> Punkt Errechnung über Lane Section Methode
    }
}