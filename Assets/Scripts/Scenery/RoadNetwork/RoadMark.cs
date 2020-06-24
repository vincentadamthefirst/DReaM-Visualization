using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scenery.RoadNetwork {
    
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class RoadMark : SceneryElement {
        
        public RoadMarkType RoadMarkType { get; set; }

        public Lane ParentLane { get; set; }
        
        public Color RoadMarkColor { get; set; }
        
        public RoadDesign RoadDesign { get; set; }
        
        public float Width { get; set; }

        public void GenerateMesh() {
            if (RoadMarkType == RoadMarkType.None) return;
            
            if (ParentLane.LaneDirection == LaneDirection.Center) {
                // maybe generate 2 meshes
                if (Math.Abs(ParentLane.GetAParameter()) < Tolerance) {
                    GenerateNormalMesh();
                } else {
                    GenerateDoubleMesh();
                }
            } else {
                GenerateNormalMesh();
            }
        }

        private void GenerateNormalMesh() {
            // TODO
        }

        private void GenerateDoubleMesh() {
            var mesh = new Mesh {subMeshCount = 2};
            var trianglesA = new List<int>();
            var trianglesB = new List<int>();
            var points = new List<Vector3>();
            var uvs = new List<Vector2>();

            if (ParentLane.Parent.CompletelyOnLineSegment && ParentLane.IsConstantWidth()) {
                points.Add(ParentLane.Parent.Parent.EvaluatePoint(0, ParentLane.Multiplier * (ParentLane.EvaluateWidth(0) - Width - Width / 2)));
                points.Add(ParentLane.Parent.Parent.EvaluatePoint(0, ParentLane.Multiplier * (ParentLane.EvaluateWidth(0) - Width / 2)));
                points.Add(ParentLane.Parent.Parent.EvaluatePoint(ParentLane.Parent.Length, ParentLane.Multiplier * (ParentLane.EvaluateWidth(0) - Width - Width / 2)));
                points.Add(ParentLane.Parent.Parent.EvaluatePoint(ParentLane.Parent.Length, ParentLane.Multiplier * (ParentLane.EvaluateWidth(0) - Width / 2)));
                
                points.Add(ParentLane.Parent.Parent.EvaluatePoint(0, ParentLane.Multiplier * (ParentLane.EvaluateWidth(0) + Width / 2)));
                points.Add(ParentLane.Parent.Parent.EvaluatePoint(0, ParentLane.Multiplier * (ParentLane.EvaluateWidth(0) + Width + Width / 2)));
                points.Add(ParentLane.Parent.Parent.EvaluatePoint(ParentLane.Parent.Length, ParentLane.Multiplier * (ParentLane.EvaluateWidth(0) + Width / 2)));
                points.Add(ParentLane.Parent.Parent.EvaluatePoint(ParentLane.Parent.Length, ParentLane.Multiplier * (ParentLane.EvaluateWidth(0) + Width + Width / 2)));

                uvs.AddRange(new[] {
                    new Vector2(0, 0),
                    new Vector2(0, 1),
                    new Vector2(1, 0),
                    new Vector2(1, 1),
                    new Vector2(0, 0),
                    new Vector2(0, 1),
                    new Vector2(1, 0),
                    new Vector2(1, 1),
                });
                
                trianglesA.AddRange(ParentLane.LaneDirection == LaneDirection.Left 
                    ? new [] {0, 2, 3, 0, 3, 1, 4, 6, 7, 4, 7, 5}
                    : new [] {0, 3, 2, 0, 1, 3, 4, 7, 6, 4, 5, 7});
            }
        }
    }
}