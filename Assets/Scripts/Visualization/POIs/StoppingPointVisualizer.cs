using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using TMPro;
using UI.Main_Menu.Utils;
using UI.POIs.StoppingPoints;
using UnityEngine;
using UnityEngine.UI;
using Visualization.Labels.BasicLabels;

// ReSharper disable Unity.InefficientPropertyAccess

namespace Visualization.POIs {

    public class LaneStoppingPoints {
        public List<StoppingPoint> stoppingPoints;
        public string LaneId;
        public Color color;
    }

    public class IntersectionStoppingPoints {
        public List<LaneStoppingPoints> laneStoppingPoints;
        public string IntersectionId;
    }

    public class StoppingPoint {
        public Vector2 position;
        public string roadId;
        public string laneId;
        public string type;

        public bool active = true;
        public bool masterActive;

        public void Update() {
            stoppingPointMarker!.gameObject.SetActive(masterActive && active);
        }

        [CanBeNull] public StoppingPointMarker stoppingPointMarker;
    }
    
    public class StoppingPointVisualizer : MonoBehaviour {
        
        public Material stoppingPointMarkerMaterial;

        [Header("UI Elements")] 
        public RectTransform container;

        public List<IntersectionStoppingPoints> IntersectionStoppingPoints { get; set; } = new();

        private bool _currentlyActive;

        public void GenerateObjects() {
            var index = 0;
            var max = 0;
            IntersectionStoppingPoints.ForEach(x => max += x.laneStoppingPoints.Count);

            foreach (var intersectionPoints in IntersectionStoppingPoints) {
                var intersectionGroup =
                    Resources.Load<IntersectionGroup>(
                        "Prefabs/UI/Visualization/RuntimeMenu/StoppingPoints/IntersectionGroup");
                var newIntEntry = Instantiate(intersectionGroup, container);
                newIntEntry.InitializeData(intersectionPoints.IntersectionId);
                
                Debug.Log($"IntersectionGroup {intersectionPoints.IntersectionId}");
                
                foreach (var laneStoppingPoint in intersectionPoints.laneStoppingPoints) {
                    laneStoppingPoint.color = Color.HSVToRGB(index / (float) max, .9f, .7f, false);
                    
                    var laneGroup = Resources.Load<LaneGroup>("Prefabs/UI/Visualization/RuntimeMenu/StoppingPoints/LaneGroup");
                    var newLaneEntry = Instantiate(laneGroup, container);
                    newLaneEntry.InitializeData(laneStoppingPoint.LaneId, laneStoppingPoint.color);
                    newLaneEntry.Parent = newIntEntry;

                    foreach (var stoppingPoint in laneStoppingPoint.stoppingPoints) {
                        var stoppingPointEntry = Resources.Load<StoppingPointEntry>("Prefabs/UI/Visualization/RuntimeMenu/StoppingPoints/StoppingPointEntry");
                        var newStoppingPointEntry = Instantiate(stoppingPointEntry, container);
                        newStoppingPointEntry.InitializeData(stoppingPoint);
                        newStoppingPointEntry.Parent = newLaneEntry;
                        
                        var stoppingPointMarker = Resources.Load<StoppingPointMarker>("Prefabs/Objects/StoppingPointMarker");
                        var spObject = Instantiate(stoppingPointMarker,
                            new Vector3(stoppingPoint.position.x, .9f, stoppingPoint.position.y), Quaternion.identity);

                        var textLabelPrefab = Resources.Load<TextLabel>("Prefabs/UI/Visualization/Labels/TextLabel");
                        var textLabelObject = Instantiate(textLabelPrefab, spObject.transform);
                        textLabelObject.transform.localPosition = new Vector3(0, -1.5f, 0);
                        textLabelObject.MainCamera = Camera.main;
                        textLabelObject.GetComponent<TMP_Text>().fontSize = 2;
                        textLabelObject.GetComponent<TMP_Text>().SetText(
                            $"{stoppingPoint.type}<br>" +
                            $"Road: {stoppingPoint.roadId} | Lane: {stoppingPoint.roadId}<br>" +
                            $"Position: {stoppingPoint.position.x}, {stoppingPoint.position.y}");
                        textLabelObject.gameObject.SetActive(false);
                        spObject.InfoLabel = textLabelObject;
                        
                        spObject.transform.Rotate(Vector3.forward, 180f);
                        spObject.transform.localScale = Vector3.one * 1.8f; 
                        spObject.transform.parent = transform;
                        stoppingPoint.stoppingPointMarker = spObject;
                        spObject.gameObject.SetActive(false);
                        var mat = new Material(stoppingPointMarkerMaterial) {
                            color = laneStoppingPoint.color.WithAlpha(.4f)
                        };
                        spObject.GetComponent<MeshRenderer>().material = mat;
                        
                        newLaneEntry.StoppingPointEntries.Add(newStoppingPointEntry);
                    }
                    
                    newIntEntry.LaneGroups.Add(newLaneEntry);
                    index++;
                }
            }

            // scroll up
            container.parent.parent.GetComponent<ScrollRect>().verticalNormalizedPosition = 1f;
            LayoutRebuilder.ForceRebuildLayoutImmediate(container);
        }

        private void Activate() {
            foreach (var stoppingPoint in from intersectionPoints in IntersectionStoppingPoints
                from laneStoppingPoint in intersectionPoints.laneStoppingPoints
                from stoppingPoint in laneStoppingPoint.stoppingPoints
                select stoppingPoint) {
                stoppingPoint.masterActive = true;
                stoppingPoint.Update();
            }

            _currentlyActive = true;
        }

        private void Deactivate() {
            foreach (var stoppingPoint in from intersectionPoints in IntersectionStoppingPoints
                from laneStoppingPoint in intersectionPoints.laneStoppingPoints
                from stoppingPoint in laneStoppingPoint.stoppingPoints
                select stoppingPoint) {
                stoppingPoint.masterActive = false;
                stoppingPoint.Update();
            }

            _currentlyActive = false;
        }

        private void Update() {
            // check for key input to toggle the stopping points
            if (!Input.GetKeyUp(KeyCode.P)) return;
            if (_currentlyActive)
                Deactivate();
            else {
                Activate();
            }
        }
    }
}