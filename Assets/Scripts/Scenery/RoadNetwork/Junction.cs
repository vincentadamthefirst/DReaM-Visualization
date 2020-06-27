using System.Collections.Generic;
using System.Linq;
using Scenery.RoadNetwork.RoadGeometries;
using UnityEngine;

namespace Scenery.RoadNetwork {
    public class Junction : SceneryElement {
        private List<Road> _roadsOnJunction;

        public RoadNetworkHolder RoadNetworkHolder { get; set; }
        
        public RoadDesign RoadDesign { get; set; }
        
        public float LowestPoint { get; set; }

        public Junction() {
            _roadsOnJunction = new List<Road>();
        }

        public void AddRoad(Road road) {
            _roadsOnJunction.Add(road);
        }

        public void ParentAllRoads() {
            foreach (var road in _roadsOnJunction) {
                road.transform.parent = transform;
            }
        }

        public void DisplaceAllLanesAndRoadMarks() {
            var allLanes = new List<Lane>();
            foreach (var section in _roadsOnJunction.SelectMany(road => road.LaneSections)) {
                allLanes.AddRange(section.LaneIdMappings.Select(entry => entry.Value));
            }

            var ordered = allLanes.OrderByDescending(l => l.Parent.CompletelyOnLineSegment)
                .ThenBy(l => l.Parent.Parent.OpenDriveId).ToList();

            for (var i = 0; i < ordered.Count; i++) {
                ordered[i].transform.position -= new Vector3(0, i * RoadDesign.offsetHeight, 0);
                LowestPoint = i * RoadDesign.offsetHeight;
            }

            var allRoadMarks = new List<RoadMark>();
            foreach (var section in _roadsOnJunction.SelectMany(road => road.LaneSections)) {
                allRoadMarks.AddRange(section.LaneIdMappings.Select(entry => entry.Value.RoadMark));
            }

            var ordered2 = allRoadMarks.OrderByDescending(rm => rm.ParentLane.Parent.CompletelyOnLineSegment)
                .ThenBy(rm => rm.ParentLane.Parent.Parent.OpenDriveId).ToList();

            for (var i = 0; i < ordered2.Count; i++) {
                ordered2[i].transform.position += new Vector3(0, i * RoadDesign.offsetHeight, 0);
            }
        }
    }
}