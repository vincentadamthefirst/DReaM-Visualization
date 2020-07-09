using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Scenery.RoadNetwork {
    
    [RequireComponent(typeof(Lane))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class Lane : SceneryElement {
        
        /// <summary>
        /// The OpenDrive id of this lane as string
        /// </summary>
        public string LaneId { get; set; }
        
        /// <summary>
        /// The OpenDrive id of this lane as integer, used for lane sorting from center out
        /// </summary>
        public int LaneIdInt { get; set; }
        
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
        
        /// <summary>
        /// Height of this Lane at the inner edge
        /// </summary>
        public float InnerHeight { get; set; }
        
        /// <summary>
        /// Height of this Lane at the outer edge
        /// </summary>
        public float OuterHeight { get; set; }
        
        /// <summary>
        /// Contact point type for the successor Lane-object. Get calculated internally, does not completely come from
        /// OpenDrive. Lanes on last LaneSection for Road always share Road ContactPoint. For every other Lane
        /// (internal Lane): ContactPoint.Start
        /// </summary>
        public ContactPoint SuccessorContactPoint { get; set; }
        
        /// <summary>
        /// The widths of this Lane segment, ordered by sOffset (low to high)
        /// </summary>
        private List<LaneWidth> Widths { get; set; } = new List<LaneWidth>();

        public void AddWidthEntry(float sOffset, float a, float b, float c, float d) {
            var newEntry = new LaneWidth(sOffset, a, b, c, d);
            Widths.Add(newEntry);
            var ordered = Widths.OrderBy(w => w.SOffset);
            Widths = ordered.ToList();
        }

        private float EvaluateWidthSelf(float s) {
            if (LaneDirection == LaneDirection.Center) return 0f;

            for (var i = 0; i < Widths.Count - 1; i++) {
                if ( s >= Widths[i].SOffset && s < Widths[i + 1].SOffset) {
                    return Widths[i].Evaluate(s);
                }
            }

            return Widths[Widths.Count - 1].Evaluate(s);
        }

        public float EvaluateWidth(float s) {
            if (LaneDirection == LaneDirection.Center) return 0f;

            return EvaluateWidthSelf(s) + InnerNeighbor.EvaluateWidth(s);
        }

        public bool IsConstantWidth() {
            if (LaneDirection == LaneDirection.Center) return true;
            
            var resultSelf = true;
            foreach (var widthEntry in Widths) {
                resultSelf = resultSelf && Math.Abs(widthEntry.B) < Tolerance && Math.Abs(widthEntry.C) < Tolerance &&
                         Math.Abs(widthEntry.D) < Tolerance;
            }

            if (InnerNeighbor == null) return resultSelf;
            return resultSelf && InnerNeighbor.IsConstantWidth();
        }

        public float GetMaxWidthSelf(int stepSize) {
            var maxWidth = float.NegativeInfinity;
            for (var i = 0f; i < Parent.Length; i += Parent.Length / stepSize) {
                var tmp = EvaluateWidthSelf(i);
                if (tmp > maxWidth) maxWidth = tmp;
            }

            return maxWidth;
        }

        public void GenerateMesh() {
            Multiplier = LaneDirection == LaneDirection.Right || LaneDirection == LaneDirection.Center ? -1 : 1;
            if (LaneDirection == LaneDirection.Center) return;
            if (LaneType == LaneType.None) return;

            var mesh = new Mesh();
            RoadHelper.GenerateMeshForLane(ref mesh, this);

            GetComponent<MeshFilter>().mesh = mesh;
            
            AddMaterials();

            RoadMark.GenerateMesh();
        }

        private void AddMaterials() {
            var meshRenderer = GetComponent<MeshRenderer>();

            if (LaneType == LaneType.Sidewalk) {
                var lm1 = RoadDesign.GetLaneMaterial(LaneType, "curb");
                var lm2 = RoadDesign.GetLaneMaterial(LaneType, "top");
                
                var m1 = new Material(lm1.material); // curb
                var m2 = new Material(lm2.material); // top
                var m3 = new Material(lm1.material); // curb

                var p = m2.GetTextureScale(BaseMap);
                var v1 = new Vector2(p.x, Parent.Length * p.y);
                var v2 = new Vector2(GetMaxWidthSelf(RoadDesign.samplePrecision) * p.x, Parent.Length * p.y);
                
                m1.SetTextureScale(BumpMap, v1); // curb
                m1.SetTextureScale(BaseMap, v1);
                m1.SetTextureScale(OcclusionMap, v1);
                m2.SetTextureScale(BumpMap, v2); // top
                m2.SetTextureScale(BaseMap, v2);
                m2.SetTextureScale(OcclusionMap, v2);
                m3.SetTextureScale(BumpMap, v1); // curb
                m3.SetTextureScale(BaseMap, v1);
                m3.SetTextureScale(OcclusionMap, v1);
                
                m1.SetColor(BaseColor, lm1.color);
                m2.SetColor(BaseColor, lm2.color);
                m3.SetColor(BaseColor, lm1.color);
                
                meshRenderer.materials = new[] {m1, m2, m3};
            } else {
                var lm = RoadDesign.GetLaneMaterial(LaneType);
                var material = new Material(lm.material);
                var p = material.GetTextureScale(BaseMap);
                var v = new Vector2( GetMaxWidthSelf(RoadDesign.samplePrecision) * p.x, Parent.Length * p.y);
                material.SetTextureScale(BumpMap, v);
                material.SetTextureScale(BaseMap, v);
                material.SetTextureScale(OcclusionMap, v);
                material.SetColor(BaseColor, lm.color);
                meshRenderer.material = material;
            }
        }
    }

    public class LaneWidth {
        public float SOffset { get; }
        private float A { get; }
        public float B { get; }
        public float C { get; }
        public float D { get; }

        public LaneWidth(float sOffset, float a, float b, float c, float d) {
            SOffset = sOffset;
            A = a;
            B = b;
            C = c;
            D = d;
        }
        
        public float Evaluate(float s) {
            return A + B * s + C * s * s + D * s * s * s;
        }
    }
}