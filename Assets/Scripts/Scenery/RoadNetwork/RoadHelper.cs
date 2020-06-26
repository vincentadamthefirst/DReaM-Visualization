using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;

namespace Scenery.RoadNetwork {
    [SuppressMessage("ReSharper", "SwitchStatementHandlesSomeKnownEnumValuesWithDefault")]
    public static class RoadHelper {
        public static void GenerateMeshForLane(ref Mesh mesh, Lane lane) {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (lane.LaneType) {
                case LaneType.Sidewalk:
                    GenerateSidewalkMeshForLane(ref mesh, lane);
                    break;
                default:
                    GenerateDrivingMeshForLane(ref mesh, lane);
                    break;
            }

            mesh.Optimize();
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
        }

        private static void GenerateSidewalkMeshForLane(ref Mesh mesh, Lane lane) {
            if (lane.Parent.Parent.OnJunction && lane.RoadDesign.disableSidewalkOnJunction) return;
            if (lane.Parent.Parent.OnJunction && lane.RoadDesign.disableStraightSidewalkOnJunction &&
                lane.Parent.CompletelyOnLineSegment) return;
            
            mesh.subMeshCount = 3;

            if (lane.Parent.CompletelyOnLineSegment && lane.IsConstantWidth()) {
                GenerateSidewalkMeshStraightForLane(ref mesh, lane);
            } else 
                GenerateSidewalkMeshNormalForLane(ref mesh, lane);
            
            if (lane.RoadDesign.closeMeshToNextMesh) CloseSidewalkMeshForLane(ref mesh, lane);
        }

        private static void GenerateSidewalkMeshStraightForLane(ref Mesh mesh, Lane lane) {
            var rd = lane.RoadDesign; // road design
            var h = rd.sidewalkHeight; // height of sidewalk
            var w = rd.sidewalkCurbWidth; // curb width
            var ls = lane.Parent; // lane section of the current lane
            var m = lane.Multiplier; // side multiplier
            var s = lane.Parent.Length; // distance to cover with mesh
            var ld = lane.LaneDirection; // direction of the lane

            var nw0 = lane.InnerNeighbor.EvaluateWidth(0);
            var lw0 = lane.EvaluateWidth(0);

            var ps = new[] {
                ls.EvaluatePoint(0, m * nw0), ls.EvaluatePoint(0, m * nw0, h), ls.EvaluatePoint(0, m * (nw0 + w), h),
                ls.EvaluatePoint(0, m * (nw0 + w), h), ls.EvaluatePoint(0, m * (lw0 - w), h),
                ls.EvaluatePoint(0, m * (lw0 - w), h), ls.EvaluatePoint(0, m * lw0, h),
                ls.EvaluatePoint(0, m * lw0),
                ls.EvaluatePoint(s, m * nw0), ls.EvaluatePoint(s, m * nw0, h), ls.EvaluatePoint(s, m * (nw0 + w), h),
                ls.EvaluatePoint(s, m * (nw0 + w), h), ls.EvaluatePoint(s, m * (lw0 - w), h),
                ls.EvaluatePoint(s, m * (lw0 - w), h), ls.EvaluatePoint(s, m * lw0, h),
                ls.EvaluatePoint(s, m * lw0)
            }; // points for the mesh

            var ts = new[] {
                new [] {0, 8, 1, 8, 9, 1, 1, 9, 10, 1, 10, 2},
                new [] {3, 11, 12, 3, 12, 4},
                new [] {5, 13, 14, 5, 14, 6, 7, 6, 14, 7, 14, 15},
            }; // triangles for the mesh

            var percentageUvCurb = h / (h + w);
            var uvs = new [] {
                new Vector2(0, 0), new Vector2(0, percentageUvCurb), new Vector2(0, 1), 
                new Vector2(0, 0), new Vector2(0, 1), 
                new Vector2(0, 1), new Vector2(0, percentageUvCurb), new Vector2(0, 0), 
                new Vector2(1, 0), new Vector2(1, percentageUvCurb), new Vector2(1, 1), 
                new Vector2(1, 0), new Vector2(1, 1), 
                new Vector2(1, 1), new Vector2(1, percentageUvCurb), new Vector2(1, 0), 
            }; // uvs for the mesh

            mesh.vertices = ps;
            mesh.uv = uvs;
            mesh.SetTriangles(ld == LaneDirection.Right ? ts[0] : TriangleDirectionChange(ts[0]), 0);
            mesh.SetTriangles(ld == LaneDirection.Right ? ts[1] : TriangleDirectionChange(ts[1]), 1);
            mesh.SetTriangles(ld == LaneDirection.Right ? ts[2] : TriangleDirectionChange(ts[2]), 2);
        }

        private static void GenerateSidewalkMeshNormalForLane(ref Mesh mesh, Lane lane) {
            var ts0 = new List<int>();
            var ts1 = new List<int>();
            var ts2 = new List<int>();
            var ps = new List<Vector3>();
            var uvs = new List<Vector2>();
            var rd = lane.RoadDesign; // road design
            var h = rd.sidewalkHeight; // height of sidewalk
            var w = rd.sidewalkCurbWidth; // curb width
            var ls = lane.Parent; // lane section of the current lane
            var m = lane.Multiplier; // side multiplier
            var s = lane.Parent.Length; // distance to cover with mesh
            var ld = lane.LaneDirection; // direction of the lane
            var p = rd.samplePrecision;

            for (var i = 0f; i < s - s / (2 * p); i += s / p) {
                var w0 = m * lane.InnerNeighbor.EvaluateWidth(i);
                var w1 = m * (lane.InnerNeighbor.EvaluateWidth(i) + w);
                var w2 = m * (lane.EvaluateWidth(0) - w);
                var w3 = m * lane.EvaluateWidth(i);

                ps.AddRange(new[] {
                    ls.EvaluatePoint(i, w0), ls.EvaluatePoint(i, w0, h), ls.EvaluatePoint(i, w1, h),
                    ls.EvaluatePoint(i, w1, h), ls.EvaluatePoint(i, w2, h),
                    ls.EvaluatePoint(i, w2, h), ls.EvaluatePoint(i, w3, h), ls.EvaluatePoint(i, w3),
                });
                
                var percentageUvCurb = h / (h + w);
                uvs.AddRange(new[] {
                    new Vector2(i / s, 0), new Vector2(i / s, percentageUvCurb), new Vector2(i / s, 1), 
                    new Vector2(i / s, 0), new Vector2(i / s, 1), 
                    new Vector2(i / s, 1), new Vector2(i / s, percentageUvCurb), new Vector2(i / s, 0),
                });
            }

            for (var i = 0; i < ps.Count - 8; i += 8) {
                ts0.AddRange(new[]
                    {i + 0, i + 8, i + 1, i + 8, i + 9, i + 1, i + 1, i + 9, i + 10, i + 1, i + 10, i + 2});
                ts1.AddRange(new[] {i + 3, i + 11, i + 12, i + 3, i + 12, i + 4});
                ts2.AddRange(new[]
                    {i + 5, i + 13, i + 14, i + 5, i + 14, i + 6, i + 7, i + 6, i + 14, i + 7, i + 14, i + 15});
            }

            mesh.vertices = ps.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.SetTriangles(ld == LaneDirection.Right ? ts0.ToArray() : TriangleDirectionChange(ts0), 0);
            mesh.SetTriangles(ld == LaneDirection.Right ? ts1.ToArray() : TriangleDirectionChange(ts1), 1);
            mesh.SetTriangles(ld == LaneDirection.Right ? ts2.ToArray() : TriangleDirectionChange(ts2), 2);
        }

        private static void CloseSidewalkMeshForLane(ref Mesh mesh, Lane lane) {
            if (lane.Successor == null) return;

            var ps = mesh.vertices.ToList();
            var ts0 = mesh.GetTriangles(0).ToList();
            var ts1 = mesh.GetTriangles(1).ToList();
            var ts2 = mesh.GetTriangles(2).ToList();
            var uvs = mesh.uv.ToList();
            var h = lane.RoadDesign.sidewalkHeight; // height of sidewalk
            var w = lane.RoadDesign.sidewalkCurbWidth; // curb width
            var ld = lane.LaneDirection; // direction of the lane
            var r = lane.Parent.Parent; // parent road
            var sr = lane.Parent.Parent.Successor; // successor road
            var sl = lane.Successor; // successor lane
            var sln = sl.InnerNeighbor;
            var m = lane.Multiplier; // direction multiplier
            var s = 0f; // distance to evaluate the point of (depends on contact point)
            if (r.SuccessorContactPoint == ContactPoint.End) {
                s = sr.Length;
                m *= -1;
            }

            var i = ps.Count - 8;
            
            ps.AddRange(new[] {
                sr.EvaluatePoint(s, m * sln.EvaluateWidth(s)), sr.EvaluatePoint(s, m * sln.EvaluateWidth(s), h),
                sr.EvaluatePoint(s, m * (sln.EvaluateWidth(s) + w), h),
                sr.EvaluatePoint(s, m * (sln.EvaluateWidth(s) + w), h),
                sr.EvaluatePoint(s, m * (sl.EvaluateWidth(s) - w), h),
                sr.EvaluatePoint(s, m * (sl.EvaluateWidth(s) - w), h),
                sr.EvaluatePoint(s, m * sl.EvaluateWidth(s), h), sr.EvaluatePoint(s, m * sl.EvaluateWidth(s)),
            });

            var percentageUvCurb = h / (h + w);
            uvs.AddRange(new[] {
                new Vector2(1, 0), new Vector2(1, percentageUvCurb), new Vector2(1, 1), 
                new Vector2(1, 0), new Vector2(1, 1),
                new Vector2(1, 1), new Vector2(1, percentageUvCurb), new Vector2(1, 0), 
            });

            var ts0Tmp = new[] {i + 0, i + 8, i + 1, i + 8, i + 9, i + 1, i + 1, i + 9, i + 10, i + 1, i + 10, i + 2};
            var ts1Tmp = new[] {i + 3, i + 11, i + 12, i + 3, i + 12, i + 4};
            var ts2Tmp = new[]
                {i + 5, i + 13, i + 14, i + 5, i + 14, i + 6, i + 7, i + 6, i + 14, i + 7, i + 14, i + 15};

            ts0.AddRange(ld == LaneDirection.Right ? ts0Tmp.ToArray() : TriangleDirectionChange(ts0Tmp));
            ts1.AddRange(ld == LaneDirection.Right ? ts1Tmp.ToArray() : TriangleDirectionChange(ts1Tmp));
            ts2.AddRange(ld == LaneDirection.Right ? ts2Tmp.ToArray() : TriangleDirectionChange(ts2Tmp));

            mesh.vertices = ps.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.SetTriangles(ts0, 0);
            mesh.SetTriangles(ts1, 1);
            mesh.SetTriangles(ts2, 2);
        }

        private static void GenerateDrivingMeshForLane(ref Mesh mesh, Lane lane) {
            if (lane.Parent.CompletelyOnLineSegment && lane.IsConstantWidth())
                GenerateDrivingMeshStraightForLane(ref mesh, lane);
            else 
                GenerateDrivingMeshNormalForLane(ref mesh, lane);
            
            if (lane.RoadDesign.closeMeshToNextMesh) CloseDrivingMeshForLane(ref mesh, lane);
        }
        
        private static void GenerateDrivingMeshStraightForLane(ref Mesh mesh, Lane lane) {
            var ls = lane.Parent; // lane section of the current lane
            var m = lane.Multiplier; // side multiplier
            var s = lane.Parent.Length; // distance to cover with mesh
            var ld = lane.LaneDirection; // direction of the lane

            var nw0 = m * lane.InnerNeighbor.EvaluateWidth(0);
            var lw0 = m * lane.EvaluateWidth(0);

            var ps = new[] {
                ls.EvaluatePoint(0, nw0), ls.EvaluatePoint(0, lw0),
                ls.EvaluatePoint(s, nw0), ls.EvaluatePoint(s, lw0),
            };

            var uvs = new[] {
                Vector2.zero, new Vector2(0, 1), new Vector2(1, 0), new Vector2(1, 1),  
            };

            var ts = new[] {0, 2, 3, 0, 3, 1};

            mesh.vertices = ps;
            mesh.uv = uvs;
            mesh.triangles = ld == LaneDirection.Right ? ts : TriangleDirectionChange(ts);
        }
        
        private static void GenerateDrivingMeshNormalForLane(ref Mesh mesh, Lane lane) {
            var ps = new List<Vector3>();
            var uvs = new List<Vector2>();
            var ts = new List<int>();
            
            var rd = lane.RoadDesign; // road design
            var p = rd.samplePrecision; // precision for sampling
            var ls = lane.Parent; // lane section of the current lane
            var m = lane.Multiplier; // side multiplier
            var s = lane.Parent.Length; // distance to cover with mesh
            var ld = lane.LaneDirection; // direction of the lane
            var mw = lane.GetMaxWidth(p); // max width of the current lane

            var upper = lane.RoadDesign.closeMeshToNextMesh ? s - s / (2 * p) : s;
            for (var i = 0f; i < upper; i += s / p) {
                var w0 = m * lane.InnerNeighbor.EvaluateWidth(i);
                var w1 = m * lane.EvaluateWidth(i);

                ps.AddRange(new[] {
                    ls.EvaluatePoint(i, w0), ls.EvaluatePoint(i, w1)
                });
                
                uvs.AddRange(new[] {
                    new Vector2(i / s, 0), new Vector2(i / s, w1 / mw),
                });
            }
            
            for (var i = 0; i < ps.Count - 2; i += 2) {
                ts.AddRange(new[] {i, i + 2, i + 3, i, i + 3, i + 1});
            }

            mesh.vertices = ps.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.triangles = ld == LaneDirection.Right ? ts.ToArray() : TriangleDirectionChange(ts);
        }

        private static void CloseDrivingMeshForLane(ref Mesh mesh, Lane lane) {
            if (lane.Successor == null) return;

            var ps = mesh.vertices.ToList();
            var ts = mesh.triangles.ToList();
            var uvs = mesh.uv.ToList();
            var ld = lane.LaneDirection; // direction of the lane
            var r = lane.Parent.Parent; // parent road
            var sr = lane.Parent.Parent.Successor; // successor road
            var sl = lane.Successor; // successor lane
            var sln = sl.InnerNeighbor; // neighbor of the successor lane
            var m = lane.Multiplier; // direction multiplier
            var s = 0f; // distance to evaluate the point of (depends on contact point)
            if (r.SuccessorContactPoint == ContactPoint.End) {
                s = sr.Length;
                m *= -1;
            }

            var w0 = sln != null ? sln.EvaluateWidth(s) : 0f;
            var w1 = sl.EvaluateWidth(s);

            var i = ps.Count - 2;

            ps.AddRange(new[]
                {sr.EvaluatePoint(s, m * w0), sr.EvaluatePoint(s, m * w1)});

            uvs.AddRange(new[] {new Vector2(1, 0), new Vector2(1, 1) });

            var tsTmp = new[] {i, i + 2, i + 3, i, i + 3, i + 1};
            ts.AddRange(ld == LaneDirection.Right ? tsTmp : TriangleDirectionChange(tsTmp));

            mesh.vertices = ps.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.triangles = ts.ToArray();
        }
        
        private static int[] TriangleDirectionChange(IReadOnlyList<int> input) {
            var changed = new int[input.Count];
            for (var i = 0; i <= input.Count - 3; i += 3) {
                changed[i] = input[i];
                changed[i + 1] = input[i + 2];
                changed[i + 2] = input[i + 1];
            }

            return changed;
        }

        public static void GenerateMeshForRoadMark(ref Mesh mesh, RoadMark roadMark) {
            if (roadMark.ParentLane.Parent.Parent.OnJunction && roadMark.RoadDesign.disableRoadMarkOnJunction) 
                return;
            if (roadMark.ParentLane.LaneType == LaneType.Sidewalk) return;
            if (roadMark.ParentLane.LaneDirection == LaneDirection.Center &&
                roadMark.ParentLane.Parent.Parent.OnJunction &&
                roadMark.RoadDesign.disableRoadMarkForCenterLaneOnJunction) return;
            
            if (roadMark.ParentLane.Parent.CompletelyOnLineSegment && roadMark.ParentLane.IsConstantWidth())
                GenerateSimpleStraightMeshForRoadMark(ref mesh, roadMark);
            else 
                GenerateSimpleNormalMeshForRoadMark(ref mesh, roadMark);
            
            if (roadMark.RoadDesign.closeMeshToNextMesh) CloseSimpleMeshForRoadMark(ref mesh, roadMark);

            // if (roadMark.ParentLane.LaneDirection == LaneDirection.Center) {
            //     Debug.Log(mesh.vertices.Length + " " + mesh.triangles.Length);
            //     foreach (var vertex in mesh.vertices) {
            //         Debug.Log(vertex);
            //     }
            // }

            mesh.Optimize();
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
        }

        private static void GenerateSimpleStraightMeshForRoadMark(ref Mesh mesh, RoadMark roadMark) {
            var ls = roadMark.ParentLane.Parent; // lane section of the current lane
            var s = roadMark.ParentLane.Parent.Length; // distance to cover with mesh
            var ld = roadMark.ParentLane.LaneDirection; // direction of the lane
            var m = roadMark.ParentLane.Multiplier; // side multiplier
            var l = roadMark.ParentLane;

            var w0 = m * (l.EvaluateWidth(0) - roadMark.Width / 2f);
            var w1 = m * (l.EvaluateWidth(0) + roadMark.Width / 2f);

            if (l.LaneDirection == LaneDirection.Center) {
                if (l.OuterNeighbor == null || l.OuterNeighbor.LaneType == LaneType.Sidewalk) w0 = 0;
                if (l.InnerNeighbor == null || l.InnerNeighbor.LaneType == LaneType.Sidewalk) w1 = 0;
            } else if (l.OuterNeighbor == null || l.OuterNeighbor.LaneType == LaneType.Sidewalk) {
                w1 = m * roadMark.ParentLane.EvaluateWidth(0);
            }

            var ps = new[] {
                ls.EvaluatePoint(0, w0), ls.EvaluatePoint(0, w1),
                ls.EvaluatePoint(s, w0), ls.EvaluatePoint(s, w1),
            };

            var uvs = new[] {
                Vector2.zero, new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1),  
            };

            var ts = new[] {0, 2, 3, 0, 3, 1};

            mesh.vertices = ps;
            mesh.uv = uvs;
            mesh.triangles = ld == LaneDirection.Right || ld == LaneDirection.Center ? ts : TriangleDirectionChange(ts);
        }

        private static void GenerateSimpleNormalMeshForRoadMark(ref Mesh mesh, RoadMark roadMark) {
            var ps = new List<Vector3>();
            var uvs = new List<Vector2>();
            var ts = new List<int>();

            var l = roadMark.ParentLane;
            var rd = roadMark.ParentLane.RoadDesign; // road design
            var p = rd.samplePrecision; // precision for sampling
            var ls = roadMark.ParentLane.Parent; // lane section of the current lane
            var m = roadMark.ParentLane.Multiplier; // side multiplier
            var s = roadMark.ParentLane.Parent.Length; // distance to cover with mesh
            var ld = roadMark.ParentLane.LaneDirection; // direction of the lane

            var upper = roadMark.RoadDesign.closeMeshToNextMesh ? s - s / (2 * p) : s;
            for (var i = 0f; i < upper; i += s / p) {
                var w0 = m * (l.EvaluateWidth(i) - roadMark.Width / 2f);
                var w1 = m * (l.EvaluateWidth(i) + roadMark.Width / 2f);

                if (l.LaneDirection == LaneDirection.Center) {
                    if (l.OuterNeighbor == null || l.OuterNeighbor.LaneType == LaneType.Sidewalk) w1 = 0;
                    if (l.InnerNeighbor == null || l.InnerNeighbor.LaneType == LaneType.Sidewalk) w0 = 0;
                } else if (l.OuterNeighbor == null || l.OuterNeighbor.LaneType == LaneType.Sidewalk) {
                    w1 = m * roadMark.ParentLane.EvaluateWidth(i);
                }

                ps.AddRange(new[] {
                    ls.EvaluatePoint(i, w0), ls.EvaluatePoint(i, w1)
                });
                
                uvs.AddRange(new[] {
                    new Vector2(0, i / s), new Vector2(1, i / s),
                });
            }
            
            for (var i = 0; i < ps.Count - 2; i += 2) {
                ts.AddRange(new[] {i, i + 2, i + 3, i, i + 3, i + 1});
            }

            mesh.vertices = ps.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.triangles = ld == LaneDirection.Right || ld == LaneDirection.Center ? ts.ToArray() : TriangleDirectionChange(ts);
        }

        private static void CloseSimpleMeshForRoadMark(ref Mesh mesh, RoadMark roadMark) {
            if (roadMark.ParentLane.Successor == null) return;

            var ps = mesh.vertices.ToList();
            var ts = mesh.triangles.ToList();
            var uvs = mesh.uv.ToList();
            var ld = roadMark.ParentLane.LaneDirection; // direction of the lane
            var r = roadMark.ParentLane.Parent.Parent; // parent road
            var sr = roadMark.ParentLane.Parent.Parent.Successor; // successor road
            var sl = roadMark.ParentLane.Successor; // successor lane
            var m = roadMark.ParentLane.Multiplier; // direction multiplier
            var s = 0f; // distance to evaluate the point of (depends on contact point)
            if (r.SuccessorContactPoint == ContactPoint.End) {
                s = sr.Length;
                m *= -1;
            }
            
            var w0 = m * (sl.EvaluateWidth(s) - roadMark.Width / 2);
            var w1 = m * (sl.EvaluateWidth(s) + roadMark.Width / 2);
            if (sl.LaneDirection == LaneDirection.Center) {
                if (sl.OuterNeighbor == null || sl.OuterNeighbor.LaneType == LaneType.Sidewalk) w1 = 0;
                if (sl.InnerNeighbor == null || sl.InnerNeighbor.LaneType == LaneType.Sidewalk) w0 = 0;
            } else if (sl.OuterNeighbor == null || sl.OuterNeighbor.LaneType == LaneType.Sidewalk) {
                w1 = m * sl.EvaluateWidth(s);
            }

            var i = ps.Count - 2;

            ps.AddRange(new[]
                {sr.EvaluatePoint(s, w0), sr.EvaluatePoint(s, w1)});
            
            if (roadMark.ParentLane.Parent.Parent.name == "534608_5") {
                Debug.Log("Closing to " + sl.LaneId + " of road " + sr.name + " with " + sr.EvaluatePoint(s, m * w0) + " and " + sr.EvaluatePoint(s, m * w1));
            }

            uvs.AddRange(new[] {new Vector2(0, 1), new Vector2(1, 1) });

            var tsTmp = new[] {i, i + 2, i + 3, i, i + 3, i + 1};
            ts.AddRange(ld == LaneDirection.Right ? tsTmp : TriangleDirectionChange(tsTmp));

            mesh.vertices = ps.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.triangles = ts.ToArray();
        }
    }
}