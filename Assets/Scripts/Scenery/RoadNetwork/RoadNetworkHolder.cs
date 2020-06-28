using System;
using System.Collections.Generic;
using System.Linq;
using Scenery.RoadNetwork.RoadObjects;
using UnityEngine;
using Utils;

namespace Scenery.RoadNetwork {
    public class RoadNetworkHolder : MonoBehaviour {
        public List<LaneSection> LaneSections { get; }
        public Dictionary<string, Road> Roads { get; }
        public Dictionary<string, Junction> Junctions { get; }

        public RoadDesign roadDesign;

        private const int RoadLayer = 17;
        private const int ObjectLayer = 19;
        
        // Properties for materials
        private static readonly int BumpMap = Shader.PropertyToID("_BumpMap");
        private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
        private static readonly int OcclusionMap = Shader.PropertyToID("_OcclusionMap");

        public RoadNetworkHolder() {
            LaneSections = new List<LaneSection>();
            Roads = new Dictionary<string, Road>();
            Junctions = new Dictionary<string, Junction>();
        }

        public void ShowSimpleGround(Terrain terrain) {
            var bounds = GetCompleteBounds();

            var width = bounds.extents.x * 2f;
            var length = bounds.extents.z * 2f;
            var i = (int) (width > length ? width : length);

            i--;
            i |= i >> 1;
            i |= i >> 2;
            i |= i >> 4;
            i |= i >> 8;
            i |= i >> 16;
            i++;

            terrain.terrainData.size = new Vector3(i, 0f, i);
            terrain.terrainData.SetDetailResolution(i, 32);
            terrain.transform.position =
                new Vector3(bounds.center.x - i / 2f, 0, bounds.center.z -  i / 2f);

            var lowest = float.NegativeInfinity;
            foreach (var junction in Junctions) {
                if (junction.Value.LowestPoint > lowest) lowest = junction.Value.LowestPoint;
            }

            if (float.IsNegativeInfinity(lowest)) lowest = roadDesign.offsetHeight;

            lowest += roadDesign.offsetHeight * 5;

            terrain.transform.position += new Vector3(0, -lowest, 0);
        }

        private Bounds GetCompleteBounds() {
            var toReturn = new Bounds();
            foreach (var lane in Roads.Values.SelectMany(road =>
                road.LaneSections.SelectMany(laneSection => laneSection.LaneIdMappings.Values))) {
                toReturn.Encapsulate(lane.GetComponent<MeshFilter>().mesh.bounds);
            }

            return toReturn;
        }

        public void CreateMeshes() {
            foreach (var road in Roads.Values) {
                if (road.SuccessorElementType == ElementType.Road) {
                    if (Roads.ContainsKey(road.SuccessorOdId)) road.Successor = Roads[road.SuccessorOdId];
                } else { // successor is junction
                    if (Junctions.ContainsKey(road.SuccessorOdId)) road.Successor = Junctions[road.SuccessorOdId];
                }
            }

            foreach (var road in Roads.Values) {
                road.PrepareLaneSuccessors();
            }
            
            foreach (var roadsValue in Roads.Values) {
                roadsValue.PrepareLaneSectionsAndGeometries();
            }

            foreach (var entry in Roads) {
                entry.Value.StartMeshGeneration();
            }

            foreach (var entry in Junctions) {
                entry.Value.ParentAllRoads();
                entry.Value.DisplaceAllLanesAndRoadMarks();
            }
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

        public RoadObjectRound CreateRoadObjectRound(Road parentRoad) {
            var newObj = new GameObject {layer = ObjectLayer};
            newObj.transform.position = Vector3.zero;
            newObj.transform.rotation = Quaternion.identity;
            var newRoadObjectRound = newObj.AddComponent<RoadObjectRound>();
            newRoadObjectRound.Parent = parentRoad;
            newRoadObjectRound.RoadDesign = roadDesign;
            parentRoad.RoadObjects.Add(newRoadObjectRound);
            return newRoadObjectRound;
        }
        
        public RoadObjectSquare CreateRoadObjectSquare(Road parentRoad) {
            var newObj = new GameObject {layer = ObjectLayer};
            newObj.transform.position = Vector3.zero;
            newObj.transform.rotation = Quaternion.identity;
            var newRoadObjectSquare = newObj.AddComponent<RoadObjectSquare>();
            newRoadObjectSquare.Parent = parentRoad;
            newRoadObjectSquare.RoadDesign = roadDesign;
            parentRoad.RoadObjects.Add(newRoadObjectSquare);
            return newRoadObjectSquare;
        }
    }
}