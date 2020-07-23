
using System;
using System.Collections.Generic;
using System.Linq;
using Scenery;
using UnityEngine;
using Utils.AdditionalMath;
using Random = UnityEngine.Random;

public class SimpleCameraController : MonoBehaviour {
    private class CameraState {
        public float yaw;
        public float pitch;
        public float roll;
        public float x;
        public float y;
        public float z;

        public void SetFromTransform(Transform t) {
            pitch = t.eulerAngles.x;
            yaw = t.eulerAngles.y;
            roll = t.eulerAngles.z;
            x = t.position.x;
            y = t.position.y;
            z = t.position.z;
        }

        public void Translate(Vector3 translation) {
            Vector3 rotatedTranslation = Quaternion.Euler(pitch, yaw, roll) * translation;

            x += rotatedTranslation.x;
            y += rotatedTranslation.y;
            z += rotatedTranslation.z;
        }

        public void LerpTowards(CameraState target, float positionLerpPct, float rotationLerpPct) {
            yaw = Mathf.Lerp(yaw, target.yaw, rotationLerpPct);
            pitch = Mathf.Lerp(pitch, target.pitch, rotationLerpPct);
            roll = Mathf.Lerp(roll, target.roll, rotationLerpPct);

            x = Mathf.Lerp(x, target.x, positionLerpPct);
            y = Mathf.Lerp(y, target.y, positionLerpPct);
            z = Mathf.Lerp(z, target.z, positionLerpPct);
        }

        public void UpdateTransform(Transform t) {
            t.eulerAngles = new Vector3(pitch, yaw, roll);
            t.position = new Vector3(x, y, z);
        }
    }

    CameraState m_TargetCameraState = new CameraState();
    CameraState m_InterpolatingCameraState = new CameraState();

    [Header("Movement Settings")] [Tooltip("Exponential boost factor on translation, controllable by mouse wheel.")]
    public float boost = 3.5f;

    [Tooltip("Time it takes to interpolate camera position 99% of the way to the target."), Range(0.001f, 1f)]
    public float positionLerpTime = 0.2f;

    [Header("Rotation Settings")]
    [Tooltip("X = Change in mouse position.\nY = Multiplicative factor for camera rotation.")]
    public AnimationCurve mouseSensitivityCurve =
        new AnimationCurve(new Keyframe(0f, 0.5f, 0f, 5f), new Keyframe(1f, 2.5f, 0f, 0f));

    [Tooltip("Time it takes to interpolate camera rotation 99% of the way to the target."), Range(0.001f, 1f)]
    public float rotationLerpTime = 0.01f;

    [Tooltip("Whether or not to invert our Y axis for mouse input to rotation.")]
    public bool invertY = false;

    [Header("Occlusion Detection Parameters")] [Tooltip("Object that will be checked if it is occluded.")]
    public VisualizationObject focusedObject;

    [Tooltip("Precision Value gets changed automatically.")]
    public bool adaptivePrecision = false;

    public int minPrecision = 2;
    public int maxPrecision = 5;
    public float precisionNearDistance = 5f;
    public float precisionFarDistance = 50f;

    void OnEnable() {
        m_TargetCameraState.SetFromTransform(transform);
        m_InterpolatingCameraState.SetFromTransform(transform);
    }

    Vector3 GetInputTranslationDirection() {
        Vector3 direction = new Vector3();
        if (Input.GetKey(KeyCode.W)) {
            direction += Vector3.forward;
        }

        if (Input.GetKey(KeyCode.S)) {
            direction += Vector3.back;
        }

        if (Input.GetKey(KeyCode.A)) {
            direction += Vector3.left;
        }

        if (Input.GetKey(KeyCode.D)) {
            direction += Vector3.right;
        }

        if (Input.GetKey(KeyCode.Q)) {
            direction += Vector3.down;
        }

        if (Input.GetKey(KeyCode.E)) {
            direction += Vector3.up;
        }

        return direction;
    }

    private void Update() {
        // Exit Sample  
        if (Input.GetKey(KeyCode.Escape)) {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        // Hide and lock cursor when right mouse button pressed
        if (Input.GetMouseButtonDown(1)) {
            Cursor.lockState = CursorLockMode.Locked;
        }

        // Unlock and show cursor when right mouse button released
        if (Input.GetMouseButtonUp(1)) {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        // Rotation
        if (Input.GetMouseButton(1)) {
            var mouseMovement = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y") * (invertY ? 1 : -1));

            var mouseSensitivityFactor = mouseSensitivityCurve.Evaluate(mouseMovement.magnitude);

            m_TargetCameraState.yaw += mouseMovement.x * mouseSensitivityFactor;
            m_TargetCameraState.pitch += mouseMovement.y * mouseSensitivityFactor;
        }

        // Translation
        var translation = GetInputTranslationDirection() * Time.deltaTime;

        // Speed up movement when shift key held
        if (Input.GetKey(KeyCode.LeftShift)) {
            translation *= 10.0f;
        }

        // Modify movement by a boost factor (defined in Inspector and modified in play mode through the mouse scroll wheel)
        boost += Input.mouseScrollDelta.y * 0.2f;
        translation *= Mathf.Pow(2.0f, boost);

        m_TargetCameraState.Translate(translation);

        // Framerate-independent interpolation
        // Calculate the lerp amount, such that we get 99% of the way to our target in the specified time
        var positionLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / positionLerpTime) * Time.deltaTime);
        var rotationLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / rotationLerpTime) * Time.deltaTime);
        m_InterpolatingCameraState.LerpTowards(m_TargetCameraState, positionLerpPct, rotationLerpPct);

        m_InterpolatingCameraState.UpdateTransform(transform);
    }

    private List<GameObject> possibleOccludingObjects = new List<GameObject>();

    private Vector3 _lastPosition;
    private Quaternion _lastRotation;
    

    private List<Vector2> GetScreenSpaceListFromMesh(GameObject go, Camera cam) {
        var returnList = new List<Vector2>();

        var meshFilters = go.GetComponentsInChildren<MeshFilter>();
        foreach (var meshFilter in meshFilters) {
            var vertices = meshFilter.mesh.vertices;
            foreach (var vertex in vertices) {
                var wts = cam.WorldToScreenPoint(meshFilter.transform.TransformPoint(vertex));
                wts.y = Screen.height - wts.y;
                returnList.Add(wts);
            }
        }

        return returnList;
    }

    private List<Vector2> ps1 = new List<Vector2>();
    private List<Vector2> ps2 = new List<Vector2>();
    
    void OnGUI() {
        return;
        foreach (var p in ps2) {
            GUI.color = Color.black;
            GUI.Box(new Rect(p.x, p.y, 0.05f, 0.05f), "");
        }
        
        foreach (var p in ps1) {
            GUI.color = Color.red;
            GUI.Box(new Rect(p.x, p.y, 0.05f, 0.05f), "");
        }
    }

    private List<Vector2> GetScreenSpaceListFromRendererBounds(GameObject go, Camera cam) {
        var bounds = GetRendererBounds(go);

        var ext = bounds.extents;
        var screenPositions = new List<Vector2>();
        for (var x = -1; x < 2; x += 2) {
            for (var y = -1; y < 2; y += 2) {
                for (var z = -1; z < 2; z += 2) {
                    var vec = bounds.center + new Vector3(ext.x * x, ext.y * y, ext.z * z);
                    var wts = cam.WorldToScreenPoint(vec);
                    wts.y = Screen.height - wts.y;
                    screenPositions.Add(wts);
                }
            }
        }

        return screenPositions;
    }
    
    private Bounds GetRendererBounds(GameObject go) {
        var renderers = go.GetComponentsInChildren<Renderer>();
        var bounds = renderers[0].bounds;
        for (var i = 1; i < renderers.Length; i++) {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds;
    }

    private void GetScreenSpacePointsFromBounds(Bounds bounds,
        out List<Vector2> screenPoints, out List<Vector3> scenePoints) {
        screenPoints = new List<Vector2>();
        scenePoints = new List<Vector3>();

        var precision = 2f;

        if (adaptivePrecision) {
            var dist = Vector3.Distance(transform.position, focusedObject.transform.position);
            if (dist > precisionFarDistance) precision = minPrecision;
            else if (dist < precisionNearDistance) precision = maxPrecision;
            else {
                var normal = Mathf.InverseLerp(precisionNearDistance, precisionFarDistance, dist);
                var unrounded = Mathf.Lerp((float) minPrecision, (float) maxPrecision, normal);
                var rounded = Mathf.RoundToInt(unrounded);
                precision = rounded;
            }
        }

        var ext = bounds.extents;
        for (var x = -1; x < 2; x += 2) {
            for (var y = -1; y < 2; y += 2) {
                for (var z = -1; z < 2; z += 2) {
                    var vec = bounds.center + new Vector3(ext.x * x, ext.y * y, ext.z * z);
                    var wts = mainCamera.WorldToScreenPoint(vec);
                    wts.y = Screen.height - wts.y;
                    scenePoints.Add(vec);
                    screenPoints.Add(wts);
                }
            }
        }

        for (var i = 0; i < 7; i += 2) {
            HandlePoints(i, i + 1, scenePoints, screenPoints, precision);
        }

        HandlePoints(0, 2, scenePoints, screenPoints, precision);
        HandlePoints(1, 3, scenePoints, screenPoints, precision);
        HandlePoints(4, 6, scenePoints, screenPoints, precision);
        HandlePoints(5, 7, scenePoints, screenPoints, precision);

        for (var i = 0; i < 4; i++) {
            HandlePoints(i, i + 4, scenePoints, screenPoints, precision);
        }
    }

    private void HandlePoints(int vec1, int vec2, IList<Vector3> scenePoints, ICollection<Vector2> screenPoints,
        float precision) {
        var pos1 = scenePoints[vec1];
        var pos2 = scenePoints[vec2];

        var distance = Vector3.Distance(pos1, pos2);
        var direction = (pos2 - pos1).normalized;

        for (var j = 1; j < precision + 1; j++) {
            var vec = pos1 + (float) j * (distance / (precision + 1)) * direction;
            var wts = mainCamera.WorldToScreenPoint(vec);
            wts.y = Screen.height - wts.y;
            scenePoints.Add(vec);
            screenPoints.Add(wts);
        }
    }

    private Camera mainCamera;

    private Plane[] planes;

    private void Start() {
        mainCamera = Camera.main;
        planes = GeometryUtility.CalculateFrustumPlanes(mainCamera);
    }

    private void RayCaster() {
        var bounds = GetRendererBounds(focusedObject.gameObject);

        planes = GeometryUtility.CalculateFrustumPlanes(mainCamera);
        if (!GeometryUtility.TestPlanesAABB(planes, bounds)) return;

        GetScreenSpacePointsFromBounds(bounds, out var screenPoints, out var scenePoints);

        var newHits = new HashSet<RaycastHit>();
        for (var i = 0; i < screenPoints.Count; i++) {
            var cameraPoint =
                mainCamera.ScreenToWorldPoint(new Vector3(screenPoints[i].x, screenPoints[i].y,
                    mainCamera.nearClipPlane));
            var directionVector = scenePoints[i] - cameraPoint;

            var currentHits = Physics.RaycastAll(cameraPoint, directionVector,
                Vector3.Distance(cameraPoint, scenePoints[i]));

            foreach (var hit in currentHits) newHits.Add(hit);
        }

        foreach (var so in newHits.Select(hit => hit.collider.GetComponent<VisualizationObject>())) {
            if (so == null) return;
            if (so == focusedObject) continue;
            so.HandleHit();
        }
    }

    // Check if polygon A is going to collide with polygon B for the given velocity
    private static bool PolygonCollision(Polygon polygonA, Polygon polygonB) {
        var edgeCountA = polygonA.Edges().Count;
        var edgeCountB = polygonB.Edges().Count;

        // Loop through all the edges of both polygons
        for (var edgeIndex = 0; edgeIndex < edgeCountA + edgeCountB; edgeIndex++) {
            var edge = edgeIndex < edgeCountA ? polygonA.Edges()[edgeIndex] : polygonB.Edges()[edgeIndex - edgeCountA];

            // Find the axis perpendicular to the current edge
            var axis = new Vector2(-edge.y, edge.x);
            axis.Normalize();

            // Find the projection of the polygon on the current axis
            float minA = 0;
            float minB = 0;
            float maxA = 0;
            float maxB = 0;
            ProjectPolygon(axis, polygonA, ref minA, ref maxA);
            ProjectPolygon(axis, polygonB, ref minB, ref maxB);

            // Check if the polygon projections are currentlty intersecting
            if (IntervalDistance(minA, maxA, minB, maxB) > 0) return false;
        }

        return true;
    }

    // Calculate the distance between [minA, maxA] and [minB, maxB]
    // The distance will be negative if the intervals overlap
    private static float IntervalDistance(float minA, float maxA, float minB, float maxB) {
        if (minA < minB) {
            return minB - maxA;
        }
        return minA - maxB;
    }

    // Calculate the projection of a polygon on an axis and returns it as a [min, max] interval
    private static void ProjectPolygon(Vector2 axis, Polygon polygon, ref float min, ref float max) {
        // To project a point on an axis use the dot product
        var d = Vector2.Dot(axis, polygon.Points()[0]);
        min = d;
        max = d;
        for (var i = 0; i < polygon.Points().Count; i++) {
            d = Vector2.Dot(polygon.Points()[i], axis);
            if (d < min) {
                min = d;
            } else {
                if (d > max) {
                    max = d;
                }
            }
        }
    }
}