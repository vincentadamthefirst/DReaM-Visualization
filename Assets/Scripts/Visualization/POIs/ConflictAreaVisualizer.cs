using System.Collections.Generic;
using System.Linq;
using Scenery.RoadNetwork;
using TMPro;
using UI.Main_Menu.Utils;
using UI.POIs.ConflictAreas;
using UnityEngine;
using UnityEngine.UI;

namespace Visualization.POIs {

    public class ConflictArea {
        public float startSa;
        public float endSa;
        public float startSb;
        public float endSb;
        
        public string roadIdA;
        public int laneIdA;
        
        public string roadIdB;
        public int laneIdB;

        public Color color;
        public GameObject obj;
    }

    public class ConflictAreaVisualizer : MonoBehaviour {

        [Header("UI Elements")] 
        public JunctionGroup junctionGroupPrefab;
        public RoadAGroup roadAGroupPrefab;
        public RoadBGroup roadBGroupPrefab;
        public LaneToLaneConflictArea laneToLaneConflictAreaPrefab;
        public RectTransform container;
        public TMP_InputField searchInput;

        public Dictionary<string, List<ConflictArea>> ConflictAreaMapping { get; set; } =
            new Dictionary<string, List<ConflictArea>>();

        public Material conflictAreaMaterial;

        private List<JunctionGroup> _junctionGroups = new List<JunctionGroup>();

        // (current road -> (other road -> (current lane, other lanes)))
        private readonly Dictionary<string, Dictionary<string, Dictionary<int, List<ConflictArea>>>> _mapping =
            new Dictionary<string, Dictionary<string, Dictionary<int, List<ConflictArea>>>>();

        private void BuildConflictArea(Road roadA, Road roadB, ConflictArea area, float colorLerp) {
            var laneA = roadA.LaneSections[0].LaneIdMappings["" + area.laneIdA];
            var laneB = roadB.LaneSections[0].LaneIdMappings["" + area.laneIdB];

            var meshA = new Mesh();
            var meshB = new Mesh();

            RoadHelper.GenerateSimpleMeshForLane(ref meshA, laneA, area.startSa, area.endSa);
            RoadHelper.GenerateSimpleMeshForLane(ref meshB, laneB, area.startSb, area.endSb);

            var newObj = new GameObject {
                transform = {
                    name = $"ConflictArea {roadA.OpenDriveId} & {roadB.OpenDriveId}",
                    parent = transform
                }
            };

            var meshFilter = newObj.AddComponent<MeshFilter>();
            var meshRenderer = newObj.AddComponent<MeshRenderer>();

            var mesh2 = new Mesh();
            var totalVertices = meshA.vertices.ToList();
            totalVertices.AddRange(meshB.vertices);
            mesh2.vertices = totalVertices.Select(x => x + Vector3.up * .2f).ToArray();

            mesh2.subMeshCount = 2;
            mesh2.SetTriangles(meshA.triangles, 0);
            mesh2.SetTriangles(meshB.triangles.Select(x => x + meshA.vertices.Length).ToArray(), 1);
            
            mesh2.Optimize();
            mesh2.RecalculateBounds();
            mesh2.RecalculateNormals();
            
            var col2 = Color.HSVToRGB(colorLerp, .9f, .7f, false);
            area.color = col2;

            var mat2 = new Material(conflictAreaMaterial) {
                color = col2.WithAlpha(.2f)
            };
            meshRenderer.materials = new [] {mat2, mat2};

            meshFilter.mesh = mesh2;
            
            newObj.SetActive(false);
            area.obj = newObj;

            // if (!_mapping.ContainsKey(roadA.OpenDriveId))
            //     _mapping.Add(roadA.OpenDriveId, new Dictionary<string, Dictionary<int, List<ConflictArea>>>());
            // if (!_mapping[roadA.OpenDriveId].ContainsKey(roadB.OpenDriveId))
            //     _mapping[roadA.OpenDriveId].Add(roadB.OpenDriveId, new Dictionary<int, List<ConflictArea>>());
            // if (!_mapping[roadA.OpenDriveId][roadB.OpenDriveId].ContainsKey(area.laneIdA))
            //     _mapping[roadA.OpenDriveId][roadB.OpenDriveId].Add(area.laneIdA, new List<ConflictArea>());
            //
            // _mapping[roadA.OpenDriveId][roadB.OpenDriveId][area.laneIdA].Add(area);
        }

        public void GenerateObjects() {
            var rnh = FindObjectOfType<RoadNetworkHolder>();

            // build the actual objects
            var totalCount = ConflictAreaMapping.Sum(item => item.Value.Count());
            var currentIndex = 0;

            foreach (var junction in ConflictAreaMapping) {
                var newJunctionGroup = Instantiate(junctionGroupPrefab, container);
                newJunctionGroup.InitializeData(junction.Key);

                foreach (var conflictArea in junction.Value) {
                    var roadA = rnh.Roads.First(x => x.Key == conflictArea.roadIdA).Value;
                    var roadB = rnh.Roads.First(x => x.Key == conflictArea.roadIdB).Value;

                    BuildConflictArea(roadA, roadB, conflictArea, currentIndex / (float)totalCount);

                    if (!newJunctionGroup.RoadAGroups.Any(x => x.roadAText.text == roadA.OpenDriveId)) {
                        var newRoadAGroup = Instantiate(roadAGroupPrefab, container);
                        newRoadAGroup.InitializeData(roadA.OpenDriveId);
                        newRoadAGroup.Parent = newJunctionGroup;
                        newJunctionGroup.RoadAGroups.Add(newRoadAGroup);
                    }

                    var roadAGroup = newJunctionGroup.RoadAGroups.First(x => x.roadAText.text == roadA.OpenDriveId);
                    if (!roadAGroup.RoadBGroups.Any(x => x.roadBText.text == roadB.OpenDriveId)) {
                        var newRoadBGroup = Instantiate(roadBGroupPrefab, container);
                        newRoadBGroup.InitializeData(roadB.OpenDriveId);
                        newRoadBGroup.Parent = roadAGroup;
                        roadAGroup.RoadBGroups.Add(newRoadBGroup);
                    }

                    var roadBGroup = roadAGroup.RoadBGroups.First(x => x.roadBText.text == roadB.OpenDriveId);

                    var newLaneToLane = Instantiate(laneToLaneConflictAreaPrefab, container);
                    newLaneToLane.InitializeData(conflictArea);
                    newLaneToLane.Parent = roadBGroup;
                    roadBGroup.ConflictAreas.Add(newLaneToLane);

                    currentIndex++;
                }

                _junctionGroups.Add(newJunctionGroup);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(container);
            
            // scroll up
            container.parent.parent.GetComponent<ScrollRect>().verticalNormalizedPosition = 1f;
        }

        public void ClearSearch() {
            foreach (var jg in _junctionGroups) {
                jg.ResetSearch();
                jg.gameObject.SetActive(true);
            }
        }

        public void StartSearch() {
            ClearSearch();
            foreach (var jg in _junctionGroups) {
                if (!jg.Search(searchInput.text))
                    jg.gameObject.SetActive(false);
            }
        }
    }
}