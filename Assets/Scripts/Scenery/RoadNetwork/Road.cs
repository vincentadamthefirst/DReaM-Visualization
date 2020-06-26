using System;
using System.Collections.Generic;
using System.Linq;
using Scenery.RoadNetwork.RoadGeometries;
using Scenery.RoadNetwork.RoadObjects;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

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
            var ordererd = _roadGeometries.AsEnumerable().OrderBy(r => r.SStart);
            _roadGeometries = ordererd.ToList();
        }

        public void AddLaneSection(LaneSection laneSection) {
            LaneSections.Add(laneSection);
            var ordererd = LaneSections.AsEnumerable().OrderBy(l => l.S);
            LaneSections = ordererd.ToList();
        }

        public void StartMeshGeneration() {
            for (var j = 0; j < LaneSections.Count; j++) {
                var nextS = j == LaneSections.Count - 1 ? Length : LaneSections[j + 1].S;
                LaneSections[j].Length = nextS - LaneSections[j].S;
            }

            foreach (var laneSection in LaneSections) {
                foreach (var geometry in _roadGeometries) {
                    if (laneSection.S >= geometry.SStart - 0.001f &&
                        laneSection.S + laneSection.Length < geometry.SStart + geometry.Length + 0.001f) {
                        if (geometry.GetType() == typeof(LineGeometry)) {
                            laneSection.CompletelyOnLineSegment = true;
                        }
                    }
                }
            }
            
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

        public List<RoadGeometry> RoadGeometries => _roadGeometries;

        // TODO
        // Referenz auf alle Lane Sections
        // Methode um das Mesh Generieren zu starten --> kaskadiert an die lane sections
        // Methode um an einer s & t - Coordinate die Geometry auszuwerten (public)

    }
}