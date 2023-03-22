using System.Collections.Concurrent;
using System.Linq;
using Scenery.RoadNetwork.RoadObjects;
using Scenery.RoadNetwork.RoadSignals;
using UnityEngine;

namespace Scenery.RoadNetwork {
    
    /// <summary>
    /// Class for referencing all elements belonging to the OpenDrive Scene as well as the class to access Unity
    /// functions in the Importer.
    /// Functions as the hierarchy root for any road network elements.
    /// </summary>
    public class RoadNetworkHolder : MonoBehaviour {
        
        /// <summary>
        /// The Roads in the scene, referenced by their OpenDrive ID
        /// </summary>
        public ConcurrentDictionary<string, Road> Roads { get; } = new();
        
        /// <summary>
        /// The Junctions in the scene, referenced by their OpenDrive ID
        /// </summary>
        private ConcurrentDictionary<string, Junction> Junctions { get; } = new();

        /// <summary>
        /// The RoadDesign to use for all scene elements
        /// </summary>
        public RoadDesign roadDesign;

        // integers to set the base layer for roads and objects
        private const int RoadLayer = 17;
        private const int ObjectLayer = 19;

        /// <summary>
        /// Shows a simple Terrain that is big enough to fit under all scene elements by setting its size and position
        /// correctly.
        /// </summary>
        /// <param name="terrain">The Terrain to be modified</param>
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

            var lowest = Junctions.Select(junction => junction.Value.LowestPoint).Concat(new[] {float.NegativeInfinity})
                .Max();

            if (float.IsNegativeInfinity(lowest)) lowest = roadDesign.offsetHeight;

            lowest += roadDesign.offsetHeight * 5;

            terrain.transform.position += new Vector3(0, -lowest, 0);
        }

        /// <summary>
        /// Returns the bounds for the entire road network.
        /// </summary>
        /// <returns>The calculated bounds</returns>
        private Bounds GetCompleteBounds() {
            var toReturn = new Bounds();
            foreach (var lane in Roads.Values.SelectMany(road =>
                road.LaneSections.SelectMany(laneSection => laneSection.LaneIdMappings.Values))) {
                toReturn.Encapsulate(lane.GetComponent<MeshFilter>().mesh.bounds);
            }

            return toReturn;
        }

        /// <summary>
        /// Prepares all Roads for Mesh generation and starts the generation procedure.
        /// </summary>
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
            
            foreach (var road in Roads.Values) {
                road.PrepareLaneSectionsAndGeometries();
            }

            foreach (var entry in Roads) {
                entry.Value.StartMeshGeneration();
            }

            foreach (var entry in Junctions) {
                entry.Value.ParentAllRoads();
                entry.Value.DisplaceAllLanesAndRoadMarks();
            }
        }

        /// <summary>
        /// Creates a new Road in the Scene, stores it internally and returns it.
        /// </summary>
        /// <param name="openDriveId">The OpenDrive Id of the road</param>
        /// <param name="junctionId">The OpenDrive Id of the junction the road is on</param>
        /// <returns>The generated Road</returns>
        public Road CreateRoad(string openDriveId, string junctionId) {
            var roadPrefab = Resources.Load<Road>("Prefabs/Objects/RoadNetwork/Road");
            var newRoad = Instantiate(roadPrefab, Vector3.zero, Quaternion.identity, transform);
            newRoad.Id = openDriveId;
            newRoad.gameObject.layer = RoadLayer;
            Roads[openDriveId] = newRoad;

            if (!Junctions.ContainsKey(junctionId)) return newRoad;
            
            Junctions[junctionId].AddRoad(newRoad);
            newRoad.ParentJunction = Junctions[junctionId];
            newRoad.gameObject.isStatic = true;

            return newRoad;
        }

        /// <summary>
        /// Creates a new Lane in the Scene, stores it in the LaneSection and returns it.
        /// </summary>
        /// <param name="parentLaneSection">The LaneSection the lane is on</param>
        /// <returns>The generated Lane</returns>
        public Lane CreateLane(LaneSection parentLaneSection) {
            var lanePrefab = Resources.Load<Lane>("Prefabs/Objects/RoadNetwork/Lane");
            var newLane = Instantiate(lanePrefab, Vector3.zero, Quaternion.identity, parentLaneSection.transform);
            newLane.gameObject.layer = RoadLayer;
            newLane.RoadDesign = roadDesign;
            newLane.gameObject.isStatic = true;

            return newLane;
        }

        /// <summary>
        /// Creates a new LaneSection in the Scene and returns it.
        /// </summary>
        /// <param name="parentRoad">The Road the laneSection is on.</param>
        /// <returns>The generated LaneSection</returns>
        public LaneSection CreateLaneSection(Road parentRoad) {
            var laneSectionPrefab = Resources.Load<LaneSection>("Prefabs/Objects/RoadNetwork/LaneSection");
            var newLaneSection = Instantiate(laneSectionPrefab, Vector3.zero, Quaternion.identity, parentRoad.transform);
            newLaneSection.gameObject.layer = RoadLayer;
            newLaneSection.gameObject.isStatic = true;
            
            return newLaneSection;
        }

        /// <summary>
        /// Creates a Junction in the Scene and returns it.
        /// </summary>
        /// <param name="openDriveId">The OpenDrive Id of the junction</param>
        /// <returns>The generated Junction</returns>
        public Junction CreateJunction(string openDriveId) {
            var junctionPrefab = Resources.Load<Junction>("Prefabs/Objects/RoadNetwork/Junction");
            var newJunction = Instantiate(junctionPrefab, Vector3.zero, Quaternion.identity, transform);
            newJunction.Id = openDriveId;
            newJunction.gameObject.layer = RoadLayer;
            newJunction.RoadDesign = roadDesign;
            newJunction.gameObject.isStatic = true;
            
            Junctions[openDriveId] = newJunction;
            return newJunction;
        }

        /// <summary>
        /// Creates a RoadMark in the Scene and returns it.
        /// </summary>
        /// <param name="parentLane">The parent Lane for the roadmark</param>
        /// <returns>The generated RoadMark</returns>
        public RoadMark CreateRoadMark(Lane parentLane) {
            var roadMarkPrefab = Resources.Load<RoadMark>("Prefabs/Objects/RoadNetwork/RoadMark");
            var newRoadMark = Instantiate(roadMarkPrefab, Vector3.zero, Quaternion.identity, transform);
            newRoadMark.gameObject.layer = RoadLayer;
            newRoadMark.transform.parent = parentLane.transform;
            newRoadMark.RoadDesign = roadDesign;
            newRoadMark.ParentLane = parentLane;
            parentLane.RoadMark = newRoadMark;
            newRoadMark.gameObject.isStatic = true;
            
            return newRoadMark;
        }

        /// <summary>
        /// Creates a RoadObjectRound in the Scene and returns it.
        /// </summary>
        /// <param name="parentRoad">The parent Rod for the object.</param>
        /// <returns>The generated RoadObjectRound</returns>
        public RoadObjectRound CreateRoadObjectRound(Road parentRoad) {
            var newObj = new GameObject {layer = ObjectLayer};
            newObj.transform.position = Vector3.zero;
            newObj.transform.rotation = Quaternion.identity;
            var newRoadObjectRound = newObj.AddComponent<RoadObjectRound>();
            newRoadObjectRound.Parent = parentRoad;
            newRoadObjectRound.RoadDesign = roadDesign;
            parentRoad.RoadObjects.Add(newRoadObjectRound);
            newRoadObjectRound.gameObject.isStatic = true;
            
            return newRoadObjectRound;
        }
        
        /// <summary>
        /// Creates a RoadObjectSquare in the Scene and returns it.
        /// </summary>
        /// <param name="parentRoad">The parent Rod for the object.</param>
        /// <returns>The generated RoadObjectSquare</returns>
        public RoadObjectSquare CreateRoadObjectSquare(Road parentRoad) {
            var newObj = new GameObject {layer = ObjectLayer};
            newObj.transform.position = Vector3.zero;
            newObj.transform.rotation = Quaternion.identity;
            var newRoadObjectSquare = newObj.AddComponent<RoadObjectSquare>();
            newRoadObjectSquare.Parent = parentRoad;
            newRoadObjectSquare.RoadDesign = roadDesign;
            parentRoad.RoadObjects.Add(newRoadObjectSquare);
            newRoadObjectSquare.gameObject.isStatic = true;
            
            return newRoadObjectSquare;
        }

        /// <summary>
        /// Creates a TrafficSign in the scene and returns it.
        /// </summary>
        /// <param name="parentRoad">The parent Road for the object.</param>
        /// <returns>The generated TrafficSign</returns>
        public TrafficSign CreateTrafficSign(Road parentRoad) {
            var newObj = new GameObject {layer = ObjectLayer};
            newObj.transform.position = Vector3.zero;
            newObj.transform.rotation = Quaternion.identity;
            var trafficSign = newObj.AddComponent<TrafficSign>();
            trafficSign.Parent = parentRoad;
            trafficSign.RoadDesign = roadDesign;
            
            parentRoad.TrafficSigns.Add(trafficSign);

            newObj.isStatic = true;
            return trafficSign;
        }
    }
}