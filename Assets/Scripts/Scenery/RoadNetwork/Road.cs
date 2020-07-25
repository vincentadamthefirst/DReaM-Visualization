using System.Collections.Generic;
using System.Linq;
using Scenery.RoadNetwork.RoadGeometries;
using Scenery.RoadNetwork.RoadObjects;
using UnityEngine;

namespace Scenery.RoadNetwork {
    
    /// <summary>
    /// Class representing an OpenDrive road
    /// </summary>
    public class Road : VisualizationElement {
        
        /// <summary>
        /// List of all RoadGeometries for this Road
        /// </summary>
        public List<RoadGeometry> RoadGeometries { get; private set; } = new List<RoadGeometry>();
        
        /// <summary>
        /// Length of this Road
        /// </summary>
        public float Length { get; set; }
        
        /// <summary>
        /// If this Road is on a Junction
        /// </summary>
        public bool OnJunction { get; set; }
        
        /// <summary>
        /// This Roads parent Junction, might be null
        /// </summary>
        public Junction ParentJunction { get; set; }
        
        /// <summary>
        /// The type of this Roads successor (if it has one)
        /// </summary>
        public ElementType SuccessorElementType { get; set; }
        
        /// <summary>
        /// The id of this Roads successor (if it has one)
        /// </summary>
        public string SuccessorOdId { get; set; }
        
        /// <summary>
        /// This Roads successor, might be null
        /// </summary>
        public VisualizationElement Successor { get; set; }
        
        /// <summary>
        /// The Contact Point of this Roads successor (if is has one)
        /// </summary>
        public ContactPoint SuccessorContactPoint { get; set; }

        /// <summary>
        /// All LaneSections on this Road
        /// </summary>
        public List<LaneSection> LaneSections { get; private set; } = new List<LaneSection>();
        
        /// <summary>
        /// All RoadObjects along this Road
        /// </summary>
        public List<RoadObject> RoadObjects { get; }  = new List<RoadObject>();
        
        public override bool IsDistractor => false;

        /// <summary>
        /// Adds a new RoadGeometry to this Road and sorts the list based on s value.
        /// </summary>
        /// <param name="geometry">The RoadGeometry to be added</param>
        public void AddRoadGeometry(RoadGeometry geometry) {
            RoadGeometries.Add(geometry);
            var ordered = RoadGeometries.AsEnumerable().OrderBy(r => r.SStart);
            RoadGeometries = ordered.ToList();
        }

        /// <summary>
        /// Adds a new LaneSection to this Road and sorts the list based on s value.
        /// </summary>
        /// <param name="laneSection">The LaneSection to be added</param>
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

                        // TODO maybe add? Changes lane that is considered the important successor)
                        //if (lastLane.Successor == null) {
                            lastLane.Successor = nextLane;
                            lastLane.SuccessorContactPoint = conn.ContactPoint;
                        //}
                    }
                }
            }
        }

        /// <summary>
        /// Sets the layer for all Lanes that belong to this Road, does NOT set the layer of any RoadObjects.
        /// </summary>
        /// <param name="layer">The new layer</param>
        public override void SetLayer(int layer) {
            gameObject.layer = layer;
            foreach (var laneSection in LaneSections) {
                laneSection.LeftLanes.ForEach(l => l.gameObject.layer = layer);
                laneSection.CenterLane.gameObject.layer = layer;
                laneSection.RightLanes.ForEach(l => l.gameObject.layer = layer);
            }
        }

        /// <summary>
        /// Scenery will not be handled on occlusion, ignore
        /// </summary>
        public override void HandleHit() {
            // Ignore
        }

        /// <summary>
        /// Scenery will not be handled on occlusion, ignore
        /// </summary>
        public override void HandleNonHit() {
            // Ignore
        }

        protected override Vector3[] GetReferencePointsRenderer() {
            return new Vector3[0]; // assume scenery is never target, Ignore
        }

        protected override Vector3[] GetReferencePointsCustom() {
            return new Vector3[0]; // assume scenery is never target, Ignore
        }

        /// <summary>
        /// Prepares all LaneSections and Geometries of this Road by setting internal parameters.
        /// </summary>
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
                foreach (var geometry in RoadGeometries) {
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

        /// <summary>
        /// Starts the Mesh generation procedure in all LaneSections.
        /// </summary>
        public void StartMeshGeneration() {
            LaneSections.ForEach(l => l.StartMeshGeneration());
            
            RoadObjects.ForEach(ro => ro.Show());

            var tmp = new List<RoadObject>();

            foreach (var ro in RoadObjects) {
                if (ro.MaybeDelete()) tmp.Add(ro);
            }

            RoadObjects.RemoveAll(ro => tmp.Contains(ro));
        }

        /// <summary>
        /// Evaluates a point along the reference line of this Road.
        /// </summary>
        /// <param name="globalS">The road-global s coordinate</param>
        /// <param name="t">The t coordinate</param>
        /// <param name="h">The height of the point, will be defaulted to 0</param>
        /// <returns>The resulting point</returns>
        public Vector3 EvaluatePoint(float globalS, float t, float h = 0f) {
            var geometry = RoadGeometries[0];
            for (var i = 0; i < RoadGeometries.Count; i++) {
                if (i == RoadGeometries.Count) geometry = RoadGeometries[i];
                var upper = i == RoadGeometries.Count - 1
                    ? Length
                    : RoadGeometries[i].SStart + RoadGeometries[i].Length;
                if (globalS >= RoadGeometries[i].SStart && globalS <= upper) {
                    geometry = RoadGeometries[i];
                }
            }
            
            var result = geometry.Evaluate(globalS - geometry.SStart, t);
            return new Vector3(result.x, h, result.y);
        }

        /// <summary>
        /// Evaluates the heading along the reference line of this Road.
        /// </summary>
        /// <param name="globalS">The road-global s coordinate</param>
        /// <returns>The resulting heading</returns>
        public float EvaluateHeading(float globalS) {
            var geometry = RoadGeometries[0];
            for (var i = 0; i < RoadGeometries.Count; i++) {
                if (i == RoadGeometries.Count) geometry = RoadGeometries[i];
                var upper = i == RoadGeometries.Count - 1
                    ? Length
                    : RoadGeometries[i].SStart + RoadGeometries[i].Length;
                if (globalS >= RoadGeometries[i].SStart && globalS <= upper) {
                    geometry = RoadGeometries[i];
                }
            }
            
            return geometry.EvaluateHeading(globalS - geometry.SStart);
        }
    }
}