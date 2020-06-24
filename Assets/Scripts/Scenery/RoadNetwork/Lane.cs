using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace Scenery.RoadNetwork {
    
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
        public Lane Neighbor { get; set; }
        
        /// <summary>
        /// The side of the road that this lane is on (in road direction)
        /// </summary>
        public LaneDirection LaneDirection { get; set; }
        
        /// <summary>
        /// The parent LaneSection that contains this lane
        /// </summary>
        public LaneSection Parent { get; set; }
        
        /// <summary>
        /// The id of the successor Lane-object as string, "-1" if there is no successor
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
        
        // parameters for width function
        private float _sOffset, _a, _b, _c, _d;
        // multiplier, get calculated with LaneDirection
        public int Multiplier { get; set; }

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
            
            if (s > Parent.Length) return Neighbor.EvaluateWidth(Parent.Length) + _a + _b * s + _c * s * s + _d * s * s * s;
            
            return Neighbor.EvaluateWidth(s) + _a + _b * s + _c * s * s + _d * s * s * s;
        }

        public bool IsConstantWidth() {
            if (Neighbor == null)
                return Math.Abs(_b) < Tolerance && Math.Abs(_c) < Tolerance && Math.Abs(_d) < Tolerance;
            else
                return Math.Abs(_b) < Tolerance && Math.Abs(_c) < Tolerance && Math.Abs(_d) < Tolerance && Neighbor.IsConstantWidth();
        }

        public float GetMaxWidth(int stepSize) {
            var max = float.NegativeInfinity;
            for (var i = 0f; i < Parent.Length; i += Parent.Length / stepSize) {
                var tmp = EvaluateWidth(i);
                if (tmp > max) max = tmp;
            }
            return max;
        }

        public void GenerateMesh() {
            if (LaneDirection == LaneDirection.Center) return;
            
            Multiplier = LaneDirection == LaneDirection.Right ? -1 : 1;

            Mesh mesh = null;

            if (IsConstantWidth() && Parent.CompletelyOnLineSegment) {
                if (LaneType == LaneType.Sidewalk && Parent.Parent.OnJunction) return;
                
                mesh = GenerateLinePointsFlat();
            } else {
                switch (LaneType) {
                    case LaneType.Sidewalk:
                        mesh = GenerateNormalPointsFlat();
                        break;
                    case LaneType.Driving:
                    case LaneType.None:
                        mesh = GenerateNormalPointsFlat();
                        break;
                }
            }
            
            mesh.Optimize();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            
            GetComponent<MeshFilter>().mesh = mesh;
            if (LaneType == LaneType.Sidewalk) {
                var materials = new List<Material>();
                for (var i = 0; i < 3; i++) {
                    var m = new Material(RoadDesign.roadBase);
                    var tilingVector = new Vector2(Parent.Length, 1);
                    m.SetTextureScale("_BumpMap", tilingVector);
                    m.SetTextureScale("_BaseMap", tilingVector);
                    materials.Add(m);
                }

                GetComponent<MeshRenderer>().materials = materials.ToArray();
            } else {
                var m = new Material(RoadDesign.roadBase);
                var tilingVector = new Vector2(Parent.Length, 1);
                m.SetTextureScale("_BumpMap", tilingVector);
                m.SetTextureScale("_BaseMap", tilingVector);

                GetComponent<MeshRenderer>().material = m;
            }
        }

        private Mesh GenerateLinePointsFlat() {
            var mesh = new Mesh();
            var points = new List<Vector3>();
            var triangles = new List<int>();
            var uvs = new List<Vector2>();

            RoadHelper.AddLinePoints(ref points, ref triangles, ref uvs, Parent.Parent, Parent.S, Parent.S + Parent.Length,
                Multiplier * Neighbor.EvaluateWidth(Parent.S), Multiplier * EvaluateWidth(Parent.S), 
                LaneDirection == LaneDirection.Right);

            mesh.vertices = points.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();
            return mesh;
        }

        private Mesh GenerateNormalPointsFlat() {
            var mesh = new Mesh();
            var points = new List<Vector3>();
            var triangles = new List<int>();
            var uvs = new List<Vector2>();

            var maxWidth = GetMaxWidth(RoadDesign.samplePrecision);
            var multiplier = LaneDirection == LaneDirection.Right ? -1 : 1;

            for (var i = 0f; i < Parent.Length - Parent.Length / RoadDesign.samplePrecision; i += Parent.Length / RoadDesign.samplePrecision) {
                var breaker = false;
                if (i >= Parent.Length) {
                    i = Parent.Length;
                    breaker = true;
                }

                points.Add(Parent.EvaluatePoint(i, multiplier * Neighbor.EvaluateWidth(i)));
                points.Add(Parent.EvaluatePoint(i, multiplier * EvaluateWidth(i)));
                
                uvs.Add(new Vector2(i / Parent.Length, 0));
                uvs.Add(new Vector2(i / Parent.Length, EvaluateWidth(i) / maxWidth));

                if (breaker) break;
            }

            if (Successor == null) return null;
            float sValue;
            if (LaneDirection == Successor.LaneDirection) {
                sValue = 0f;
            } else {
                sValue = Successor.Parent.Length;
                Multiplier *= -1;
            }

            points.Add(Parent.Parent.Successor.EvaluatePoint(sValue, Multiplier * Successor.Neighbor.EvaluateWidth(sValue)));
            points.Add(Parent.Parent.Successor.EvaluatePoint(sValue, Multiplier * Successor.EvaluateWidth(sValue)));
            
            uvs.AddRange(new [] {new Vector2(1, 0), new Vector2(1, 1), });

            for (var i = 0; i < points.Count - 2; i += 2) {
                triangles.AddRange(LaneDirection == LaneDirection.Right
                    ? new[] {i, i + 2, i + 3, i, i + 3, i + 1}
                    : new[] {i, i + 3, i + 2, i, i + 1, i + 3});
            }
            
            mesh.vertices = points.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();
            return mesh;
        }

        private Mesh GenerateLinePointsSidewalk() {
            var mesh = new Mesh();
            var points = new List<Vector3>();
            var trianglesA = new List<int>(); // left curb
            var trianglesB = new List<int>(); // main section
            var trianglesC = new List<int>(); // right curb
            
            var triangles = new List<int>();

            points.Add(Parent.EvaluatePoint(0, Multiplier * Neighbor.EvaluateWidth(0)));
            points.Add(Parent.EvaluatePoint(0, Multiplier * EvaluateWidth(0)));
            points.Add(Parent.EvaluatePoint(Parent.Length, Multiplier * Neighbor.EvaluateWidth(Parent.Length)));
            points.Add(Parent.EvaluatePoint(Parent.Length, Multiplier * EvaluateWidth(Parent.Length)));

            triangles.AddRange(
                LaneDirection == LaneDirection.Right ? new[] {0, 2, 3, 0, 3, 1} : new[] {0, 3, 2, 0, 1, 3});
            
            mesh.vertices = points.ToArray();
            mesh.triangles = triangles.ToArray();

            return mesh;
        }

        private Mesh GenerateNormalPointsSidewalk() {
            var mesh = new Mesh();
            var points = new List<Vector3>();
            var trianglesA = new List<int>(); // left curb
            var trianglesB = new List<int>(); // main section
            var trianglesC = new List<int>(); // right curb
            var uvs = new List<Vector2>();

            for (var i = 0f; i < Parent.Length - Parent.Length / RoadDesign.samplePrecision; i += Parent.Length / RoadDesign.samplePrecision) {
                var breaker = false;
                if (i >= Parent.Length) {
                    i = Parent.Length;
                    breaker = true;
                }

                var smallWidth = Multiplier * Neighbor.EvaluateWidth(i);
                var widthLeftCurb = Multiplier * (Neighbor.EvaluateWidth(i) + RoadDesign.sidewalkCurbWidth);
                var widthRightCurb = Multiplier * (EvaluateWidth(i) - RoadDesign.sidewalkCurbWidth);
                var bigWidth = Multiplier * EvaluateWidth(i);

                var leftCurbPointA = Parent.EvaluatePoint(i, smallWidth);
                var leftCurbPointB = Parent.EvaluatePoint(i, widthLeftCurb);
                var rightCurbPointB = Parent.EvaluatePoint(i, widthRightCurb);
                var rightCurbPointA = Parent.EvaluatePoint(i, bigWidth);
                
                // left curb
                points.Add(leftCurbPointA);
                points.Add(new Vector3(leftCurbPointA.x, RoadDesign.sidewalkHeight, leftCurbPointA.z));
                points.Add(new Vector3(leftCurbPointB.x, RoadDesign.sidewalkHeight, leftCurbPointB.z));
                
                // center piece
                points.Add(new Vector3(leftCurbPointB.x, RoadDesign.sidewalkHeight, leftCurbPointB.z));
                points.Add(new Vector3(rightCurbPointB.x, RoadDesign.sidewalkHeight, rightCurbPointB.z));

                // right curb
                points.Add(new Vector3(rightCurbPointB.x, RoadDesign.sidewalkHeight, rightCurbPointB.z));
                points.Add(new Vector3(rightCurbPointA.x, RoadDesign.sidewalkHeight, rightCurbPointA.z));
                points.Add(rightCurbPointA);
                
                uvs.AddRange(new [] {
                    new Vector2(i / Parent.Length, 0), 
                    new Vector2(i / Parent.Length, RoadDesign.sidewalkHeight / (RoadDesign.sidewalkHeight + RoadDesign.sidewalkCurbWidth)), 
                    new Vector2(i / Parent.Length, 1), 
                    new Vector2(i / Parent.Length, 0),
                    new Vector2(i / Parent.Length, 1),
                    new Vector2(i / Parent.Length, 1),
                    new Vector2(i / Parent.Length, RoadDesign.sidewalkHeight / (RoadDesign.sidewalkHeight + RoadDesign.sidewalkCurbWidth)),
                    new Vector2(i / Parent.Length, 0),
                });
                
                if (breaker) break;
            }
            
            for (var i = 0; i < points.Count - 8; i += 8) {
                trianglesA.AddRange(LaneDirection == LaneDirection.Right 
                    ? new[] {i + 8, i + 9, i + 1, i + 8, i + 1, i, i + 1, i + 9, i + 10, i + 1, i + 10, i + 2} 
                    : new[] {i + 8, i + 1, i + 9, i + 8, i, i + 1, i + 1, i + 10, i + 9, i + 1, i + 2, i + 10});
                
                trianglesB.AddRange(LaneDirection == LaneDirection.Right
                    ? new[] {i + 3, i + 11, i + 12, i + 3, i + 12, i + 4}
                    : new[] {i + 3, i + 12, i + 11, i + 3, i + 4, i + 12});
                
                trianglesC.AddRange(LaneDirection == LaneDirection.Right
                    ? new[] {i + 5, i + 13, i + 14, i + 5, i + 14, i + 6, i + 6, i + 14, i + 15, i + 6, i + 15, i + 7}
                    : new[] {i + 5, i + 14, i + 13, i + 5, i + 6, i + 14, i + 6, i + 15, i + 14, i + 6, i + 7, i + 15});
            }

            mesh.subMeshCount = 3;
            mesh.vertices = points.ToArray();
            mesh.SetTriangles(trianglesA.ToArray(), 0);
            mesh.SetTriangles(trianglesB.ToArray(), 1);
            mesh.SetTriangles(trianglesC.ToArray(), 2);

            return mesh;
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