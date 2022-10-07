using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UI.Main_Menu.Utils;
using UI.POIs.StoppingPoints;
using UnityEngine;
using UnityEngine.UI;

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
            gameObject.SetActive(masterActive && active);
        }

        [CanBeNull] public GameObject gameObject;
    }
    
    public class StoppingPointVisualizer : MonoBehaviour {

        public GameObject stoppingPointMarkerPrefab;
        public Material stoppingPointMarkerMaterial;

        [Header("UI Elements")] 
        public RectTransform container;
        public IntersectionGroup intersectionGroupPrefab;
        public LaneGroup laneGroupPrefab;
        public StoppingPointEntry stoppingPointEntryPrefab;

        public List<IntersectionStoppingPoints> IntersectionStoppingPoints { get; set; } =
            new List<IntersectionStoppingPoints>();

        private bool _currentlyActive;

        public void GenerateObjects() {
            var index = 0;
            var max = 0;
            IntersectionStoppingPoints.ForEach(x => max += x.laneStoppingPoints.Count);
            
            foreach (var intersectionPoints in IntersectionStoppingPoints) {
                var newIntEntry = Instantiate(intersectionGroupPrefab, container);
                newIntEntry.InitializeData(intersectionPoints.IntersectionId);
                
                foreach (var laneStoppingPoint in intersectionPoints.laneStoppingPoints) {
                    laneStoppingPoint.color = Color.HSVToRGB(index / (float) max, .9f, .7f, false);
                    
                    var newLaneEntry = Instantiate(laneGroupPrefab, container);
                    newLaneEntry.InitializeData(laneStoppingPoint.LaneId, laneStoppingPoint.color);
                    newLaneEntry.Parent = newIntEntry;

                    foreach (var stoppingPoint in laneStoppingPoint.stoppingPoints) {
                        var newStoppingPointEntry = Instantiate(stoppingPointEntryPrefab, container);
                        newStoppingPointEntry.InitializeData(stoppingPoint);
                        newStoppingPointEntry.Parent = newLaneEntry;
                        
                        var spObject = Instantiate(stoppingPointMarkerPrefab,
                            new Vector3(stoppingPoint.position.x, .9f, stoppingPoint.position.y), Quaternion.identity);
                        spObject.transform.Rotate(Vector3.forward, 180f);
                        spObject.transform.localScale = Vector3.one * 1.8f; 
                        spObject.transform.parent = transform;
                        stoppingPoint.gameObject = spObject;
                        spObject.SetActive(false);
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
            if (Input.GetKeyUp(KeyCode.P)) {
                if (_currentlyActive)
                    Deactivate();
                else {
                    Activate();
                }
            }
        }
    }
}