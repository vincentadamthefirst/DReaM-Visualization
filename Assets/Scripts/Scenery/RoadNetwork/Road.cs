using System;
using System.Collections.Generic;
using System.Linq;
using Meta.Numerics.Functions;
using Scenery.RoadNetwork.RoadGeometries;
using Scenery.RoadNetwork.RoadObjects;
using UnityEngine;
using UnityEngine.Analytics;

namespace Scenery.RoadNetwork {
    public class Road : SceneryElement {
        private List<RoadGeometry> _roadGeometries;

        public float Length { get; set; }
        
        public bool OnJunction { get; set; }
        
        public ElementType SuccessorElementType { get; set; }
        
        public string SuccessorOdId { get; set; }
        
        public SceneryElement Successor { get; set; }
        
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

        /// <summary>
        /// Gives each Lane of this road the appropriate successor. Currently only handles right Lanes as left lanes
        /// will be ignored for mesh completion.
        /// </summary>
        public void PrepareLaneSuccessors() {
            if (SuccessorElementType == ElementType.Road) {
                var successor = Successor as Road;
                if (successor == null) return;
                
                for (var i = 0; i < LaneSections.Count; i++) {
                    var ls = LaneSections[i];
                    if (i == LaneSections.Count - 1) { // last LaneSection for road
                        var nls = SuccessorContactPoint == ContactPoint.Start
                            ? successor.LaneSections[0]
                            : successor.LaneSections[successor.LaneSections.Count - 1];

                        foreach (var lane in ls.LaneIdMappings.Values) {
                            if (lane.LaneDirection == LaneDirection.Center) {
                                lane.Successor = nls.CenterLane;
                                continue;
                            }
                            
                            if (lane.SuccessorId != "x") {
                                if (nls.LaneIdMappings.ContainsKey(lane.SuccessorId)) {
                                    lane.Successor = nls.LaneIdMappings[lane.SuccessorId];
                                }
                            }
                        }
                    } else {
                        var nls = LaneSections[i + 1];
                    
                        foreach (var lane in ls.LaneIdMappings.Values) {
                            // each internal right Lane has the ContactPoint Start to the next right Lane
                            lane.SuccessorContactPoint = ContactPoint.Start;

                            if (lane.LaneDirection == LaneDirection.Center) {
                                lane.Successor = nls.CenterLane;
                                continue;
                            }

                            if (lane.SuccessorId != "x") {
                                if (nls.LaneIdMappings.ContainsKey(lane.SuccessorId)) {
                                    lane.Successor = nls.LaneIdMappings[lane.SuccessorId];
                                }
                            }
                        }
                    }
                }
            } else if (SuccessorElementType == ElementType.Junction) {
                var successor = (Successor as Junction);
                if (successor == null) return;

                var lastLaneSection = LaneSections.Last();

                foreach (var conn in successor.GetAllConnectionForIncomingRoad(OpenDriveId)) {
                    var successorRoad = successor.Roads[conn.ConnectingRoadOdId];
                    var nextLaneSection = conn.ContactPoint == ContactPoint.Start
                        ? successorRoad.LaneSections[0]
                        : successorRoad.LaneSections.Last();
                    
                    foreach (var ll in conn.LaneLinks) {
                        if (!lastLaneSection.LaneIdMappings.ContainsKey(ll.From)) continue;
                        if (!nextLaneSection.LaneIdMappings.ContainsKey(ll.To)) continue;

                        var lastLane = lastLaneSection.LaneIdMappings[ll.From];
                        var nextLane = nextLaneSection.LaneIdMappings[ll.To];

                        //if (lastLane.Successor == null) {
                            lastLane.Successor = nextLane;
                            lastLane.SuccessorContactPoint = conn.ContactPoint;
                        //}
                    }
                }

                return;
                // var successor = Successor as Junction;
                // if (successor == null) return;
                //
                // foreach (var connection in successor.Connections) {
                //     if (connection.IncomingRoadOdId != OpenDriveId) continue;
                //     if (!successor.Roads.ContainsKey(connection.ConnectingRoadOdId)) continue;
                //     
                //     var successorRoad = successor.Roads[connection.ConnectingRoadOdId]; // connection road
                //
                //     var cls = LaneSections[LaneSections.Count - 1]; // current LaneSection
                //     var nls = connection.ContactPoint == ContactPoint.Start // next LaneSection
                //         ? successorRoad.LaneSections[0]
                //         : successorRoad.LaneSections[successorRoad.LaneSections.Count - 1];
                //
                //     foreach (var laneLink in connection.LaneLinks) {
                //         if (cls.LaneIdMappings.ContainsKey(laneLink.From) && nls.LaneIdMappings.ContainsKey(laneLink.To)) {
                //             if (cls.LaneIdMappings[laneLink.From].Successor == null) {
                //                 cls.LaneIdMappings[laneLink.From] = nls.LaneIdMappings[laneLink.To];
                //                 cls.LaneIdMappings[laneLink.From].SuccessorContactPoint =
                //                     connection.ContactPoint;
                //             }
                //         }
                //     }
                // }
            }
        }

        public void PrepareLaneSectionsAndGeometries() {
            for (var i = 0; i < LaneSections.Count; i++) {
                // preparing lane neighbors
                LaneSections[i].PrepareNeighbors();

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
            }
            
            if (name == "j1_north") {
                foreach (var ls in LaneSections) {
                    Debug.Log(ls.S + " & " + ls.Length);
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