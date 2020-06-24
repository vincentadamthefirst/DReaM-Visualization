using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scenery.RoadNetwork {
    public class LaneSection : SceneryElement {
        private List<Lane> _leftLanes;
        private Lane _centerLane;
        private List<Lane> _rightLanes;

        public Dictionary<string, Lane> LaneIdMappings { get; private set; }

        public LaneSection() {
            _leftLanes = new List<Lane>();
            _rightLanes = new List<Lane>();
            LaneIdMappings = new Dictionary<string, Lane>();
        }

        public float S { get; set; }
        
        public float Length { get; set; }
        
        public Road Parent { get; set; }

        public void AddLeftLane(Lane lane) {
            _leftLanes.Add(lane);
            var ordered = _leftLanes.AsEnumerable().OrderBy(l => l.LaneId);
            _leftLanes = ordered.ToList();

            LaneIdMappings[lane.OpenDriveId] = lane;
        }
        
        public void AddRightLane(Lane lane) {
            _rightLanes.Add(lane);
            var ordered = _rightLanes.AsEnumerable().OrderByDescending(l => l.LaneId);
            _rightLanes = ordered.ToList();
            
            LaneIdMappings[lane.OpenDriveId] = lane;
        }

        public Lane CenterLane {
            set => _centerLane = value;
        }
        
        public bool CompletelyOnLineSegment { get; set; }

        public void StartMeshGeneration() {
            for (var i = 0; i < _leftLanes.Count; i++) {
                if (i == 0) _leftLanes[i].Neighbor = _centerLane;
                else _leftLanes[i].Neighbor = _leftLanes[i - 1];
            }
            
            for (var i = 0; i < _rightLanes.Count; i++) {
                if (i == 0) _rightLanes[i].Neighbor = _centerLane;
                else _rightLanes[i].Neighbor = _rightLanes[i - 1];
            }
            
            _leftLanes.ForEach(l => l.GenerateMesh());
            _rightLanes.ForEach(l => l.GenerateMesh());
        }

        public Vector3 EvaluatePoint(float s, float t) {
            return Parent.EvaluatePoint(S + s, t);
        }

        // TODO
        // Referenz auf das zugehörige Road Objekt
        // Werte: start sOffset
        // Methode um Mesh Generierung zu starten
        //     --> berechnet einen boolean, ob diese Lane Section sich komplett in einem Line-Segment befindet
        //     --> kaskadiert Mesh Generierung an alle Lanes
        // Methode um an s & t Coordinate geometry auszuwerten (returnt Ergebnis von der parent road)
    }
}