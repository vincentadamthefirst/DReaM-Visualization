using System.Collections.Generic;
using UnityEngine;

namespace Scenery.RoadNetwork {
    public class RoadNetworkHolder : MonoBehaviour {
        public List<LaneSection> LaneSections { get; }
        public Dictionary<string, Road> Roads { get; }
        public Dictionary<string, Junction> Junctions { get; }

        public RoadDesign roadDesign;

        private const int RoadLayer = 17;

        public RoadNetworkHolder() {
            LaneSections = new List<LaneSection>();
            Roads = new Dictionary<string, Road>();
            Junctions = new Dictionary<string, Junction>();
        }

        public void CreateMeshes() {
            foreach (var roadEntry in Roads) {
                var successorId = roadEntry.Value.SuccessorOdId;
                if (successorId != "-1" && Roads.ContainsKey(successorId)) {
                    roadEntry.Value.Successor = Roads[successorId];

                    var lastLaneSections = roadEntry.Value.LaneSections;
                    var nextLaneSections = Roads[successorId].LaneSections;

                    var lastLaneSection = lastLaneSections[lastLaneSections.Count - 1];
                    var nextLaneSection = Roads[successorId].SuccessorContactPoint == ContactPoint.End
                        ? nextLaneSections[nextLaneSections.Count - 1]
                        : nextLaneSections[0];

                    foreach (var laneEntry in lastLaneSection.LaneIdMappings) {
                        if (!nextLaneSection.LaneIdMappings.ContainsKey(laneEntry.Value.SuccessorId)) continue;
                        laneEntry.Value.Successor = nextLaneSection.LaneIdMappings[laneEntry.Value.SuccessorId];
                    }
                }
            }

            foreach (var entry in Roads) {
                entry.Value.StartMeshGeneration();
            }

            foreach (var entry in Junctions) {
                entry.Value.ParentAllRoads();
                entry.Value.DisplaceAllLanesAndRoadMarks();
            }
            
            // float displacement = 0;
            // for (var i = 0; i < transform.childCount; i++) {
            //     transform.GetChild(i).position += new Vector3(0, displacement, 0);
            //     displacement -= 0.00001f;
            // }
        }

        public Road CreateRoad(string openDriveId, string junctionId) {
            var newRoad = Instantiate(roadDesign.roadPrefab, Vector3.zero, Quaternion.identity, transform)
                .GetComponent<Road>();
            newRoad.OpenDriveId = openDriveId;
            newRoad.gameObject.layer = RoadLayer;
            Roads[openDriveId] = newRoad;

            if (Junctions.ContainsKey(junctionId)) {
                Junctions[junctionId].AddRoad(newRoad);
            }
            
            return newRoad;
        }

        public Lane CreateLane(LaneSection parentLaneSection) {
            var newLane = Instantiate(roadDesign.lanePrefab, Vector3.zero, Quaternion.identity, parentLaneSection.transform)
                .GetComponent<Lane>();
            newLane.gameObject.layer = RoadLayer;
            newLane.RoadDesign = roadDesign;
            return newLane;
        }

        public LaneSection CreateLaneSection(Road parentRoad) {
            var newLaneSection =
                Instantiate(roadDesign.laneSectionPrefab, Vector3.zero, Quaternion.identity, parentRoad.transform)
                    .GetComponent<LaneSection>();
            newLaneSection.gameObject.layer = RoadLayer;
            LaneSections.Add(newLaneSection);
            return newLaneSection;
        }

        public Junction CreateJunction(string openDriveId) {
            var newJunction = Instantiate(roadDesign.junctionPrefab, Vector3.zero, Quaternion.identity, transform)
                .GetComponent<Junction>();
            newJunction.OpenDriveId = openDriveId;
            newJunction.gameObject.layer = RoadLayer;
            newJunction.RoadDesign = roadDesign;
            Junctions[openDriveId] = newJunction;
            return newJunction;
        }

        public RoadMark CreateRoadMark(Lane parentLane) {
            var newRoadMark = Instantiate(roadDesign.roadMarkPrefab, Vector3.zero, Quaternion.identity, transform)
                .GetComponent<RoadMark>();
            newRoadMark.gameObject.layer = RoadLayer;
            newRoadMark.transform.parent = parentLane.transform;
            newRoadMark.RoadDesign = roadDesign;
            newRoadMark.ParentLane = parentLane;
            parentLane.RoadMark = newRoadMark;
            return newRoadMark;
        }
    }
}