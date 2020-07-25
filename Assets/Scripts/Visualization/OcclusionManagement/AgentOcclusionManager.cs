using System;
using System.Collections.Generic;
using System.Linq;
using Scenery;
using UnityEditor;
using UnityEngine;
using Utils;
using Utils.AdditionalMath;
using Visualization.OcclusionManagement.DetectionMethods;

namespace Visualization.OcclusionManagement {
    public class AgentOcclusionManager : MonoBehaviour {
        public enum DetectionMethod {
            RAYCAST_ALL_BoxCollider, RAYCAST_CONVEXHULL_BoxCollider, POLYGON_BoxCollider, POLYGON_RendererAABBB, POLYGON_Mesh
        }

        [Header("Testing")]
        public Material focusObjectMaterial;

        public bool disable;
    
        [Header("Occlusion Detection")] 
        public List<VisualizationObject> targetObjects = new List<VisualizationObject>();
        public DetectionMethod detectionMethod = DetectionMethod.POLYGON_BoxCollider;
        public bool checkEveryFrame;
        public bool preCheckViewFrustum = true;

        [Header("RayCast Options")] 
        public bool nearClipAsStart = false;
        public bool usePrecision;
        public int fixedPrecision = 2;
        public bool adaptivePrecision;
        public int minPrecision = 2;
        public int maxPrecision = 5;
        public float precisionNearDistance = 5f;
        public float precisionFarDistance = 50f;
        public bool showRayCastEnd = true;

        [Header("Polygon Options")] 
        public bool distanceCheck = true;
        public bool showPolygons = true;

        // position & rotation for this camera in the last frame
        private Vector3 _lastPosition;
        private Quaternion _lastRotation;

        // objects that will be tested for occlusion with the focused object
        private List<VisualizationObject> _possibleOccludingObjects = new List<VisualizationObject>();
    
        // for GUI
        private Dictionary<VisualizationObject, Polygon> _distractorPolygons = new Dictionary<VisualizationObject, Polygon>();
        private List<Polygon> _targetPolygons;
        private List<Vector2> _rayCastPositions;
    
        // the main camera and its properties
        private Camera cam;
        private Plane[] _planes;

        // for raycast
        private Dictionary<VisualizationObject, HashSet<VisualizationObject>> _lastRayCastDict;
        
        // NEW STUFF

        private OcclusionDetector _occlusionDetector;
        
        public OcclusionManagementOptions OcclusionManagementOptions { get; set; }

        private Dictionary<Collider, VisualizationElement> _colliderMapping;

        private VisualizationElement[] _allElements = new VisualizationElement[0];

        private static readonly Type[][] OcclusionDetectors = {
            new [] {typeof(RayCastDetectorNormal), typeof(RayCastDetectorStaggered)}, 
            new [] {typeof(PolygonDetectorNormal), typeof(PolygonDetectorStaggered)}
        };

        private void Start() {
            cam = Camera.main;
            _planes = GeometryUtility.CalculateFrustumPlanes(cam);
            _rayCastPositions = new List<Vector2>();
            _targetPolygons = new List<Polygon>();
            _lastRayCastDict = new Dictionary<VisualizationObject, HashSet<VisualizationObject>>();
            _colliderMapping = new Dictionary<Collider, VisualizationElement>();
        }

        public void Prepare() {
            if (OcclusionManagementOptions.occlusionDetectionMethod == OcclusionDetectionMethod.Shader) {
                
            } else {
                _occlusionDetector = (OcclusionDetector) Activator.CreateInstance(
                    OcclusionDetectors[(int) OcclusionManagementOptions.occlusionHandlingMethod][
                        OcclusionManagementOptions.staggeredCheck ? 1 : 0]);
            }

            // setting the base parameters
            _occlusionDetector.ExtendedCamera = FindObjectOfType<ExtendedCamera>();
            _occlusionDetector.LayerMask = LayerMask.GetMask("agents_base", "scenery_objects", "scenery_signs", "scenery_targets", "agent_targets");
            _occlusionDetector.OcclusionManagementOptions = OcclusionManagementOptions;

            // finding all colliders in scene
            var colliders = FindObjectsOfType<Collider>();
            foreach (var coll in colliders) {
                var tmp = coll.GetComponentInParent<VisualizationElement>();

                if (tmp == null) continue;
                _colliderMapping[coll] = tmp;
            }
            
            _occlusionDetector.ColliderMapping = _colliderMapping;

            // finding all VisualizationElements
            _allElements = FindObjectsOfType<VisualizationElement>();

            foreach (var visualizationElement in _allElements) { // adding all VisualizationElements
                if (!visualizationElement.IsDistractor) continue;

                _occlusionDetector.DistractorCounts[visualizationElement] = 0;
                _occlusionDetector.OnlyDistractors.Add(visualizationElement);
            }
        }

        public void AddTarget(VisualizationElement target) {
            _occlusionDetector.Targets.Add(target);
            _occlusionDetector.LastHits[target] = new HashSet<VisualizationElement>();
        }
        
        private void LateUpdate() {
            _occlusionDetector.Trigger();
        }

        private void Update() {
            return;
            
            if (disable) return;
        
            if (preCheckViewFrustum) _planes = GeometryUtility.CalculateFrustumPlanes(cam);
        
            _targetPolygons.Clear();
            _distractorPolygons.Clear();
            _rayCastPositions.Clear();

            foreach (var target in targetObjects) {
                OcclusionDetection(target);
            }

            var ownTransform = transform;
            _lastPosition = ownTransform.position;
            _lastRotation = ownTransform.rotation;
        
            var targetPos = targetObjects[0].transform.position;
            focusObjectMaterial.SetVector("_ObjectPos", targetPos);
        }

        private void OcclusionDetection(VisualizationObject targetObject) {
            if (!checkEveryFrame && transform.position == _lastPosition && transform.rotation == _lastRotation) {
                return;
            }

            if (preCheckViewFrustum && !IsObjectInViewFrustum(targetObject)) {
                return;
            }

            if (detectionMethod == DetectionMethod.RAYCAST_ALL_BoxCollider ||
                detectionMethod == DetectionMethod.RAYCAST_CONVEXHULL_BoxCollider) {
            
                if (_distractorPolygons.Count != 0) {
                    foreach (var entry in _distractorPolygons) {
                        entry.Key.HandleNonHit();
                    }
                    _distractorPolygons.Clear();
                }

                RayCaster(targetObject);
            }

            if (detectionMethod == DetectionMethod.POLYGON_BoxCollider ||
                detectionMethod == DetectionMethod.POLYGON_RendererAABBB ||
                detectionMethod == DetectionMethod.POLYGON_Mesh) {
                PolygonDetection(targetObject);
            }
        }

        private void RayCaster(VisualizationObject targetObject) {
            var screenPoints = new List<Vector2>();
            var scenePoints = new Dictionary<Vector2, Vector3>();

            switch (detectionMethod) {
                case DetectionMethod.RAYCAST_ALL_BoxCollider:
                    GetScreenAndScenePointsFromBoxCollider(targetObject, ref screenPoints, ref scenePoints);
                    if (usePrecision) ExtentPoints(ref screenPoints, ref scenePoints, targetObject);
                    break;
                case DetectionMethod.RAYCAST_CONVEXHULL_BoxCollider:
                    GetScreenAndScenePointsFromBoxCollider(targetObject, ref screenPoints, ref scenePoints);
                    if (usePrecision) ExtentPoints(ref screenPoints, ref scenePoints, targetObject);
                    screenPoints = ConvexHull.GrahamScanCompute(screenPoints);
                    break;
            }
        
            var newHits = new HashSet<VisualizationObject>();
            var distanceDictionary = new Dictionary<VisualizationObject, float>();
        
            for (var i = 0; i < screenPoints.Count; i++) {
                var cameraPoint = nearClipAsStart
                    ? cam.ScreenToWorldPoint(new Vector3(screenPoints[i].x, screenPoints[i].y, cam.nearClipPlane))
                    : cam.transform.position;
                var scenePoint = scenePoints[screenPoints[i]];
                var directionVector = scenePoint - cameraPoint;
                var distance = Vector3.Distance(cameraPoint, scenePoint);

                // ReSharper disable once Unity.PreferNonAllocApi
                var currentHits = Physics.RaycastAll(cameraPoint, directionVector, distance);

                foreach (var hit in currentHits) {
                    var so = hit.collider.GetComponent<VisualizationObject>();
                    if (so == null || so == targetObject) continue;
                    newHits.Add(so);
                    distanceDictionary[so] = Vector3.Distance(cam.transform.position, so.transform.position);
                }
            }

            var sortedNewHits = newHits.AsEnumerable().OrderByDescending(e => distanceDictionary[e]);
            var alphaCounter = 0.2f;
            foreach (var hit in sortedNewHits) {
                hit.HandleHit(alphaCounter);
            
                if (alphaCounter > 0) {
                    alphaCounter -= 0.05f;
                }
            }

            if (_lastRayCastDict.ContainsKey(targetObject)) {
                _lastRayCastDict[targetObject].ExceptWith(newHits);
                foreach (var hit in _lastRayCastDict[targetObject]) {
                    hit.HandleNonHit();
                }
            }

            _lastRayCastDict[targetObject] = newHits;
            _rayCastPositions.AddRange(screenPoints);
        }

        private void PolygonDetection(VisualizationObject targetObject) {
            IList<Vector2> pointsForFocused;
            switch (detectionMethod) {
                case DetectionMethod.POLYGON_BoxCollider:
                    pointsForFocused = ConvexHull.GrahamScanCompute(GetScreenSpaceListFromBoxCollider(targetObject));
                    break;
                case DetectionMethod.POLYGON_RendererAABBB:
                    pointsForFocused = ConvexHull.GrahamScanCompute(GetScreenSpaceListFromRendererBounds(targetObject));
                    break;
                default:
                    pointsForFocused = ConvexHull.GrahamScanCompute(GetScreenSpaceListFromMesh(targetObject));
                    break;
            }

            var focusedPolygon = new Polygon(pointsForFocused);
            _targetPolygons.Add(focusedPolygon);

            var distanceToFocused = 0f;
            if (distanceCheck)
                distanceToFocused = Vector3.Distance(targetObject.transform.position, cam.transform.position);

            var distractors = new List<VisualizationObject>();
            var distances = new Dictionary<VisualizationObject, float>();

            foreach (var sceneryObject in _possibleOccludingObjects) {
                var distanceToSo = Vector3.Distance(sceneryObject.transform.position, cam.transform.position);
                var inViewFrustum = preCheckViewFrustum && !IsObjectInViewFrustum(sceneryObject);
                var inFrontOfTarget = distanceCheck && distanceToSo > distanceToFocused;

                if (inViewFrustum || inFrontOfTarget) {
                    _distractorPolygons[sceneryObject] = null;
                    sceneryObject.HandleNonHit();
                    continue;
                }

                IList<Vector2> pointsForGo;
                switch (detectionMethod) {
                    case DetectionMethod.POLYGON_BoxCollider:
                        pointsForGo =
                            ConvexHull.GrahamScanCompute(GetScreenSpaceListFromBoxCollider(sceneryObject));
                        break;
                    case DetectionMethod.POLYGON_RendererAABBB:
                        pointsForGo =
                            ConvexHull.GrahamScanCompute(GetScreenSpaceListFromRendererBounds(sceneryObject));
                        break;
                    default:
                        pointsForGo = ConvexHull.GrahamScanCompute(GetScreenSpaceListFromMesh(sceneryObject));
                        break;
                }

                var distractorPolygon = new Polygon(pointsForGo);

                if (distractorPolygon.DoesCollide(focusedPolygon)) {
                    distractors.Add(sceneryObject);
                    distances[sceneryObject] = distanceToSo;
                    _distractorPolygons[sceneryObject] = distractorPolygon;
                } else {
                    sceneryObject.HandleNonHit();
                    _distractorPolygons[sceneryObject] = null;
                }
            }

            var distractorsSorted = distractors.OrderByDescending(e => distances[e]);
            var alphaCounter = 0.2f;
            foreach (var distractor in distractorsSorted) {
                distractor.HandleHit(alphaCounter);

                if (alphaCounter > 0) {
                    alphaCounter -= 0.05f;
                }
            }
        }

        public void AddPossibleOccludingObject(VisualizationObject so) {
            _possibleOccludingObjects.Add(so);
        }

        private bool IsObjectInViewFrustum(VisualizationObject so) {
            return GeometryUtility.TestPlanesAABB(_planes, GetRendererBounds(so));
        }
    
        private static Bounds GetRendererBounds(VisualizationObject so) {
            var renderers = so.GetRenderers();
            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++) {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds;
        }

        private void GetScreenAndScenePointsFromBoxCollider(VisualizationObject so, ref List<Vector2> screenPositions,
            ref Dictionary<Vector2, Vector3> scenePositions) {
        
            var boxCollider = so.GetBoxCollider();
            var transformationMatrix = so.transform.localToWorldMatrix;
            var ext = boxCollider.size;
        
            for (var x = -1; x < 2; x += 2) {
                for (var y = -1; y < 2; y += 2) {
                    for (var z = -1; z < 2; z += 2) {
                        var vec = new Vector3(ext.x * x * .5f, ext.y * y * .5f, ext.z * z * .5f);
                        var scenePosition = transformationMatrix.MultiplyPoint3x4(vec);
                        var wts = WorldToScreenPoint(scenePosition);
                        screenPositions.Add(wts);
                        scenePositions[wts] = scenePosition;
                    }
                }
            }
        }

        private void ExtentPoints(ref List<Vector2> screenPoints, ref Dictionary<Vector2, Vector3> scenePoints, VisualizationObject targetObject) {
            float precision = fixedPrecision;
        
            if (adaptivePrecision) {
                var dist = Vector3.Distance(cam.transform.position, targetObject.transform.position);
                if (dist > precisionFarDistance) precision = minPrecision;
                else if (dist < precisionNearDistance) precision = maxPrecision;
                else {
                    var normal = Mathf.InverseLerp(precisionNearDistance, precisionFarDistance, dist);
                    var unrounded = Mathf.Lerp((float) minPrecision, (float) maxPrecision, normal);
                    var rounded = Mathf.RoundToInt(unrounded);
                    precision = rounded;
                }
            }

            for (var i = 0; i < 7; i += 2) {
                HandlePoints(i, i + 1, screenPoints, scenePoints, precision);
            }

            HandlePoints(0, 2, screenPoints, scenePoints, precision);
            HandlePoints(1, 3, screenPoints, scenePoints, precision);
            HandlePoints(4, 6, screenPoints, scenePoints, precision);
            HandlePoints(5, 7, screenPoints, scenePoints, precision);

            for (var i = 0; i < 4; i++) {
                HandlePoints(i, i + 4, screenPoints, scenePoints, precision);
            }
        }

        private void HandlePoints(int vec1, int vec2, List<Vector2> screenPoints, Dictionary<Vector2, Vector3> scenePoints,
            float precision) {
        
            var pos1 = scenePoints[screenPoints[vec1]];
            var pos2 = scenePoints[screenPoints[vec2]];

            var distance = Vector3.Distance(pos1, pos2);
            var direction = (pos2 - pos1).normalized;

            for (var j = 1; j < precision + 1; j++) {
                var vec = pos1 + (float) j * (distance / (precision + 1)) * direction;
                var wts = WorldToScreenPoint(vec);
                screenPoints.Add(wts);
                scenePoints[wts] = vec;
            }
        }

        private Vector2 WorldToScreenPoint(Vector3 point) {
            var wts = cam.WorldToScreenPoint(point);
            wts.y = Screen.height - wts.y;
            return wts;
        }
    
        private List<Vector2> GetScreenSpaceListFromRendererBounds(VisualizationObject so) {
            var bounds = GetRendererBounds(so);

            var ext = bounds.extents;
            var screenPositions = new List<Vector2>();
            for (var x = -1; x < 2; x += 2) {
                for (var y = -1; y < 2; y += 2) {
                    for (var z = -1; z < 2; z += 2) {
                        var vec = bounds.center + new Vector3(ext.x * x, ext.y * y, ext.z * z);
                        screenPositions.Add(WorldToScreenPoint(vec));
                    }
                }
            }
        
            return screenPositions;
        }

        private List<Vector2> GetScreenSpaceListFromBoxCollider(VisualizationObject so) {
            var returnList = new List<Vector2>();

            var boxCollider = so.GetBoxCollider();
            var transformationMatrix = so.transform.localToWorldMatrix;
            var ext = boxCollider.size;
        
            for (var x = -1; x < 2; x += 2) {
                for (var y = -1; y < 2; y += 2) {
                    for (var z = -1; z < 2; z += 2) {
                        var vec = new Vector3(ext.x * x * .5f, ext.y * y * .5f, ext.z * z * .5f);
                        var scenePosition = transformationMatrix.MultiplyPoint3x4(vec);
                        returnList.Add(WorldToScreenPoint(scenePosition));
                    }
                }
            }

            return returnList;
        }
    
        private List<Vector2> GetScreenSpaceListFromMesh(VisualizationObject so) {
            var returnList = new List<Vector2>();

            var meshFilters = so.GetMeshFilters();
            foreach (var meshFilter in meshFilters) {
                var vertices = meshFilter.mesh.vertices;
                foreach (var vertex in vertices) {
                    returnList.Add(WorldToScreenPoint(meshFilter.transform.TransformPoint(vertex)));
                }
            }

            return returnList;
        }
    }
}