using System.Collections.Generic;
using UnityEngine;

namespace Scenery.RoadNetwork {
    public class NetworkHolder : MonoBehaviour {
        private List<LaneSection> _laneSections = new List<LaneSection>();
        private Dictionary<string, Road> _roads = new Dictionary<string, Road>();
        private Dictionary<string, Junction> _junctions = new Dictionary<string, Junction>();

        public RoadDesign roadDesign;

        private const int _networkLayer = 17;

        public void CreateMeshes() {
            foreach (var roadEntry in _roads) {
                var successorId = roadEntry.Value.SuccessorOdId;
                if (successorId != "-1" && _roads.ContainsKey(successorId)) {
                    roadEntry.Value.Successor = _roads[successorId];
                    
                    var lastLaneSection = roadEntry.Value.LaneSections[roadEntry.Value.LaneSections.Count - 1];
                    var nextLaneSection = _roads[successorId].LaneSections[0];
                    
                    foreach (var laneEntry in lastLaneSection.LaneIdMappings) {
                        if (nextLaneSection.LaneIdMappings.ContainsKey(laneEntry.Value.SuccessorId)) {
                            laneEntry.Value.Successor = nextLaneSection.LaneIdMappings[laneEntry.Value.SuccessorId];
                        }
                    }
                }

                foreach (var laneSection in roadEntry.Value.LaneSections) {
                    foreach (var lane in laneSection.LaneIdMappings) {
                        lane.Value.RoadDesign = roadDesign;
                    }
                }
            }

            foreach (var road in _roads) {
                road.Value.StartMeshGeneration();
            }
        }

        public Road CreateRoad(string openDriveId) {
            var newRoad = Instantiate(roadDesign.roadPrefab, Vector3.zero, Quaternion.identity, transform)
                .GetComponent<Road>();
            newRoad.OpenDriveId = openDriveId;
            newRoad.gameObject.layer = _networkLayer;
            _roads[openDriveId] = newRoad;
            return newRoad;
        }

        public Lane CreateLane(LaneSection parentLaneSection) {
            var newLane = Instantiate(roadDesign.lanePrefab, Vector3.zero, Quaternion.identity, parentLaneSection.transform)
                .GetComponent<Lane>();
            newLane.gameObject.layer = _networkLayer;
            return newLane;
        }

        public LaneSection CreateLaneSection(Road parentRoad) {
            var newLaneSection =
                Instantiate(roadDesign.laneSectionPrefab, Vector3.zero, Quaternion.identity, parentRoad.transform)
                    .GetComponent<LaneSection>();
            newLaneSection.gameObject.layer = _networkLayer;
            _laneSections.Add(newLaneSection);
            return newLaneSection;
        }
    }
}