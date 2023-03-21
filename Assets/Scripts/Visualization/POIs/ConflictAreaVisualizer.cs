using System.Collections.Generic;
using System.Linq;
using Scenery.RoadNetwork;
using TMPro;
using UI.Main_Menu.Utils;
using UI.POIs.ConflictAreas;
using UnityEngine;
using UnityEngine.UI;
using Visualization.Labels.BasicLabels;

namespace Visualization.POIs {

    public class ConflictAreaInfo {
        public float startSa;
        public float endSa;
        public float startSb;
        public float endSb;

        public string roadIdA;
        public int laneIdA;

        public string roadIdB;
        public int laneIdB;

        public Color color;
        public ConflictArea conflictArea;
    }

    public class ConflictAreaVisualizer : MonoBehaviour {

        [Header("UI Elements")] public RectTransform container;
        public TMP_InputField searchInput;

        public Dictionary<string, List<ConflictAreaInfo>> ConflictAreaMapping { get; set; } = new();

        public Material conflictAreaMaterial;

        private readonly List<IntersectionGroup> _junctionGroups = new();
        private readonly List<ConflictArea> _conflictAreas = new();

        private void BuildConflictArea(Road roadA, Road roadB, ConflictAreaInfo areaInfo, float colorLerp) {
            var laneA = roadA.LaneSections[0].LaneIdMappings["" + areaInfo.laneIdA];
            var laneB = roadB.LaneSections[0].LaneIdMappings["" + areaInfo.laneIdB];

            var meshA = new Mesh();
            var meshB = new Mesh();

            RoadHelper.GenerateSimpleMeshForLane(ref meshA, laneA, areaInfo.startSa, areaInfo.endSa);
            RoadHelper.GenerateSimpleMeshForLane(ref meshB, laneB, areaInfo.startSb, areaInfo.endSb);

            var conflictAreaPrefab = Resources.Load<ConflictArea>("Prefabs/Objects/ConflictArea");
            var conflictArea = Instantiate(conflictAreaPrefab, transform);
            conflictArea.name = $"ConflictArea {roadA.Id} & {roadB.Id}";

            var mesh = new Mesh();
            var totalVertices = meshA.vertices.ToList();
            totalVertices.AddRange(meshB.vertices);
            mesh.vertices = totalVertices.Select(x => x + Vector3.up * .2f).ToArray();

            mesh.subMeshCount = 2;
            mesh.SetTriangles(meshA.triangles, 0);
            mesh.SetTriangles(meshB.triangles.Select(x => x + meshA.vertices.Length).ToArray(), 1);

            mesh.Optimize();
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            var color = Color.HSVToRGB(colorLerp, .9f, .7f, false);
            areaInfo.color = color;
            
            var mat2 = new Material(conflictAreaMaterial) {
                color = color.WithAlpha(.2f)
            };
            var materials = new[] { mat2, mat2 };

            conflictArea.SetMeshData(mesh, materials);

            var centroid = new Vector2 {
                x = mesh.vertices.Sum(vertex => vertex.x) / mesh.vertices.Length,
                y = mesh.vertices.Sum(vertex => vertex.z) / mesh.vertices.Length
            };

            var textLabelPrefab = Resources.Load<TextLabel>("Prefabs/UI/Visualization/Labels/TextLabel");
            var textLabel = Instantiate(textLabelPrefab, conflictArea.transform);
            textLabel.name = "Label";
            textLabel.transform.localPosition = new Vector3(centroid.x, 1.5f, centroid.y);
            textLabel.MainCamera = Camera.main;
            textLabel.GetComponent<TMP_Text>().fontSize = 3.5f;
            textLabel.GetComponent<TMP_Text>().SetText(
                $"Road {areaInfo.roadIdA}, Lane {areaInfo.laneIdA} <br>" +
                $"Road {areaInfo.roadIdB}, Lane {areaInfo.laneIdB}");
            textLabel.gameObject.SetActive(false);
            conflictArea.InfoLabel = textLabel;

            conflictArea.MeshObject.SetActive(false);
            areaInfo.conflictArea = conflictArea;
        }

        public void GenerateObjects() {
            var rnh = FindObjectOfType<RoadNetworkHolder>();

            // build the actual objects
            var totalCount = ConflictAreaMapping.Sum(item => item.Value.Count());
            var currentIndex = 0;

            foreach (var junction in ConflictAreaMapping) {
                var junctionGroupPrefab =
                    Resources.Load<IntersectionGroup>(
                        "Prefabs/UI/Visualization/RuntimeMenu/ConflictAreas/IntersectionGroup");
                var newJunctionGroup = Instantiate(junctionGroupPrefab, container);
                newJunctionGroup.InitializeData(junction.Key);

                foreach (var conflictArea in junction.Value) {
                    var roadA = rnh.Roads.First(x => x.Key == conflictArea.roadIdA).Value;
                    var roadB = rnh.Roads.First(x => x.Key == conflictArea.roadIdB).Value;

                    BuildConflictArea(roadA, roadB, conflictArea, currentIndex / (float)totalCount);

                    if (newJunctionGroup.RoadAGroups.All(x => x.roadAText.text != roadA.Id)) {
                        var roadAGroupPrefab =
                            Resources.Load<RoadAGroup>("Prefabs/UI/Visualization/RuntimeMenu/ConflictAreas/RoadAGroup");
                        var newRoadAGroup = Instantiate(roadAGroupPrefab, container);
                        newRoadAGroup.InitializeData(roadA.Id);
                        newRoadAGroup.Parent = newJunctionGroup;
                        newJunctionGroup.RoadAGroups.Add(newRoadAGroup);
                    }

                    var roadAGroup = newJunctionGroup.RoadAGroups.First(x => x.roadAText.text == roadA.Id);
                    if (roadAGroup.RoadBGroups.All(x => x.roadBText.text != roadB.Id)) {
                        var roadBGroupPrefab =
                            Resources.Load<RoadBGroup>("Prefabs/UI/Visualization/RuntimeMenu/ConflictAreas/RoadBGroup");
                        var newRoadBGroup = Instantiate(roadBGroupPrefab, container);
                        newRoadBGroup.InitializeData(roadB.Id);
                        newRoadBGroup.Parent = roadAGroup;
                        roadAGroup.RoadBGroups.Add(newRoadBGroup);
                    }

                    var roadBGroup = roadAGroup.RoadBGroups.First(x => x.roadBText.text == roadB.Id);

                    var laneToLaneConflictArea = Resources.Load<LaneToLaneConflictArea>(
                        "Prefabs/UI/Visualization/RuntimeMenu/ConflictAreas/LaneToLaneConflictArea");
                    var newLaneToLane = Instantiate(laneToLaneConflictArea, container);
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
            foreach (var jg in _junctionGroups.Where(jg => !jg.Search(searchInput.text))) {
                jg.gameObject.SetActive(false);
            }
        }
    }
}