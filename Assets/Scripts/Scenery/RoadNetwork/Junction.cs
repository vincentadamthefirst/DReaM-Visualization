using System.Collections.Generic;
using System.Linq;
using Scenery.RoadNetwork.RoadGeometries;
using UnityEngine;

namespace Scenery.RoadNetwork {
    public class Junction : SceneryElement {
        public Dictionary<string, Road> Roads { get; }

        public RoadNetworkHolder RoadNetworkHolder { get; set; }
        
        public RoadDesign RoadDesign { get; set; }
        
        public float LowestPoint { get; set; }
        
        public List<Connection> Connections { get; set; }

        public Junction() {
            Roads = new Dictionary<string, Road>();
            Connections = new List<Connection>();
        }

        public void AddRoad(Road road) {
            Roads.Add(road.OpenDriveId, road);
        }

        public void ParentAllRoads() {
            foreach (var road in Roads.Values) {
                road.transform.parent = transform;
            }
        }

        public List<Connection> GetAllConnectionForIncomingRoad(string incomingRoadId) {
            return Connections.Where(c => c.IncomingRoadOdId == incomingRoadId).ToList();
        }

        public void DisplaceAllLanesAndRoadMarks() {
            var allLanes = new List<Lane>();
            foreach (var section in Roads.SelectMany(road => road.Value.LaneSections)) {
                allLanes.AddRange(section.LaneIdMappings.Select(entry => entry.Value));
            }

            var ordered = allLanes.OrderBy(l => l.LaneType )
                .ThenByDescending(l => l.Parent.CompletelyOnLineSegment).ToList();

            for (var i = 0; i < ordered.Count; i++) {
                ordered[i].transform.position -= new Vector3(0, i * RoadDesign.offsetHeight, 0);
                LowestPoint = i * RoadDesign.offsetHeight;
            }

            var allRoadMarks = new List<RoadMark>();
            foreach (var section in Roads.SelectMany(road => road.Value.LaneSections)) {
                allRoadMarks.AddRange(section.LaneIdMappings.Select(entry => entry.Value.RoadMark));
            }

            var ordered2 = allRoadMarks.OrderByDescending(rm => rm.ParentLane.Parent.CompletelyOnLineSegment)
                .ThenBy(rm => rm.ParentLane.LaneType).ToList();

            for (var i = 0; i < ordered2.Count; i++) {
                ordered2[i].transform.position += new Vector3(0, i * RoadDesign.offsetHeight, 0);
            }
        }
    }

    public class Connection {
        public string IncomingRoadOdId { get; set; }
        public string ConnectingRoadOdId { get; set; }
        public ContactPoint ContactPoint { get; set; }

        public List<LaneLink> LaneLinks { get; set; } = new List<LaneLink>();
    }

    public class LaneLink {
        public string From { get; set; }
        public string To { get; set; }
    }
}