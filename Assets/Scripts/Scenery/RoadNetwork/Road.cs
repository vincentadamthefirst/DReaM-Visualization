using System;
using System.Collections.Generic;
using System.Linq;
using Scenery.RoadNetwork.RoadGeometries;
using Scenery.RoadNetwork.RoadObjects;
using UnityEngine;
using UnityEngine.Analytics;

namespace Scenery.RoadNetwork {
    public class Road : SceneryElement {
        private List<RoadGeometry> _roadGeometries;

        public float Length { get; set; }
        
        public bool OnJunction { get; set; }
        
        public string SuccessorOdId { get; set; }
        public Road Successor { get; set; }
        
        public ContactPoint SuccessorContactPoint { get; set; }

        public List<LaneSection> LaneSections { get; private set; }
        
        public List<RoadObject> RoadObjects { get; private set; }

        public Road() {
            _roadGeometries = new List<RoadGeometry>();
            LaneSections = new List<LaneSection>();
            RoadObjects = new List<RoadObject>();
        }

        public void AddRoadGeometry(RoadGeometry geometry) {
            _roadGeometries.Add(geometry);
            var ordered = _roadGeometries.AsEnumerable().OrderBy(r => r.SStart);
            _roadGeometries = ordered.ToList();
        }

        public void AddLaneSection(LaneSection laneSection) {
            LaneSections.Add(laneSection);
            var ordered = LaneSections.AsEnumerable().OrderBy(l => l.S);
            LaneSections = ordered.ToList();
        }

        public void Prepare() {
            for (var i = 0; i < LaneSections.Count; i++) {
                // preparing lane neighbors
                LaneSections[i].Prepare();
                
                // setting the length of the LaneSection
                LaneSections[i].Length = i == LaneSections.Count - 1
                    ? Length - LaneSections[i].S
                    : LaneSections[i + 1].S - LaneSections[i].S;
                
                // setting the Lanes SuccessorContactPoint (Lanes at the end of the Road receive the Roads ContactPoint)
                foreach (var entry in LaneSections[i].LaneIdMappings) {
                    entry.Value.SuccessorContactPoint =
                        i == LaneSections.Count - 1 ? SuccessorContactPoint : ContactPoint.Start;
                }

                // checking if the LaneSection is completely within a LineGeometry
                foreach (var geometry in _roadGeometries) {
                    if (geometry.GetType() != typeof(LineGeometry)) continue;
                    if (!(LaneSections[i].S >= geometry.SStart - 0.0001f) ||
                        !(LaneSections[i].S + LaneSections[i].Length < geometry.SStart + geometry.Length + 0.0001f))
                        continue;
                    
                    LaneSections[i].CompletelyOnLineSegment = true;
                    break;
                }
                
                if (i == LaneSections.Count - 1) continue;
                foreach (var entry in LaneSections[i].LaneIdMappings) {
                    if (entry.Value.Successor != null) continue;
                    if (entry.Value.SuccessorId == "x") continue;
                    if (!LaneSections[i + 1].LaneIdMappings.ContainsKey(entry.Value.SuccessorId)) continue;
                    
                    var successor = LaneSections[i + 1].LaneIdMappings[entry.Value.SuccessorId];
                    entry.Value.Successor = successor;
                }
            }
        }

        public void StartMeshGeneration() {
            LaneSections.ForEach(l => l.StartMeshGeneration());
            
            RoadObjects.ForEach(ro => ro.Show());
        }

        public Vector3 EvaluatePoint(float globalS, float t, float h = 0f) {
            var geometry = _roadGeometries[0];
            for (var i = 0; i < _roadGeometries.Count; i++) {
                if (i == _roadGeometries.Count) geometry = _roadGeometries[i];
                var upper = i == _roadGeometries.Count - 1
                    ? Length
                    : _roadGeometries[i].SStart + _roadGeometries[i].Length;
                if (globalS >= _roadGeometries[i].SStart && globalS <= upper) {
                    geometry = _roadGeometries[i];
                }
            }
            
            var result = geometry.Evaluate(globalS - geometry.SStart, t);
            return new Vector3(result.x, h, result.y);
        }

        public float EvaluateHeading(float globalS) {
            var geometry = _roadGeometries[0];
            for (var i = 0; i < _roadGeometries.Count; i++) {
                if (i == _roadGeometries.Count) geometry = _roadGeometries[i];
                var upper = i == _roadGeometries.Count - 1
                    ? Length
                    : _roadGeometries[i].SStart + _roadGeometries[i].Length;
                if (globalS >= _roadGeometries[i].SStart && globalS <= upper) {
                    geometry = _roadGeometries[i];
                }
            }
            
            return geometry.EvaluateHeading(globalS - geometry.SStart);
        }

        public List<RoadGeometry> RoadGeometries => _roadGeometries;

        // TODO
        // Referenz auf alle Lane Sections
        // Methode um das Mesh Generieren zu starten --> kaskadiert an die lane sections
        // Methode um an einer s & t - Coordinate die Geometry auszuwerten (public)

    }
}