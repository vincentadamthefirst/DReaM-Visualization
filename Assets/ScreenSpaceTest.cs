using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class ScreenSpaceTest : MonoBehaviour {
    public GameObject target;

    [Range(0, 1f)] public float RendererOrMeshBounds;

    private static Rect GetScreenSpaceRectFromRendererBounds(GameObject go, Camera cam) {
        if (go == null || cam == null) {
            return Rect.zero;
        }

        var renderers = go.GetComponentsInChildren<Renderer>();
        var bounds = renderers[0].bounds;
        for (var i = 1; i < renderers.Length; i++) {
            bounds.Encapsulate(renderers[i].bounds);
        }

        var ext = bounds.extents;
        var screenPositions = new List<Vector2>();
        for (var x = -1; x < 2; x += 2) {
            for (var y = -1; y < 2; y += 2) {
                for (var z = -1; z < 2; z += 2) {
                    var vect = bounds.center + new Vector3(ext.x * x, ext.y * y, ext.z * z);
                    var wts = cam.WorldToScreenPoint(vect);
                    wts.y = Screen.height - wts.y;
                    screenPositions.Add(wts);
                }
            }
        }
        
        Debug.Log(screenPositions.Count);

        var rect = new Rect(screenPositions[0].x, screenPositions[0].y, 1, 1);
        Action<Vector2> FindMinMax = vec => {
            if (vec.x < rect.xMin) rect.xMin = vec.x;
            if (vec.x > rect.xMax) rect.xMax = vec.x;
            if (vec.y < rect.yMin) rect.yMin = vec.y;
            if (vec.y > rect.yMax) rect.yMax = vec.y;
        };
        screenPositions.ForEach(FindMinMax);
        return rect;
    }
    
    private static List<Vector2> GetScreenSpaceListFromRendererBounds(GameObject go, Camera cam) {
        if (go == null || cam == null) {
            return new List<Vector2>();
        }

        var renderers = go.GetComponentsInChildren<Renderer>();
        var bounds = renderers[0].bounds;
        for (var i = 1; i < renderers.Length; i++) {
            bounds.Encapsulate(renderers[i].bounds);
        }

        var ext = bounds.extents;
        var screenPositions = new List<Vector2>();
        for (var x = -1; x < 2; x += 2) {
            for (var y = -1; y < 2; y += 2) {
                for (var z = -1; z < 2; z += 2) {
                    var vect = bounds.center + new Vector3(ext.x * x, ext.y * y, ext.z * z);
                    var wts = cam.WorldToScreenPoint(vect);
                    wts.y = Screen.height - wts.y;
                    screenPositions.Add(wts);
                }
            }
        }

        return screenPositions;
    }

    private static Rect GetScreenSpaceRectFromMeshBounds(GameObject go, Camera cam) {
        if (go == null || cam == null) {
            return Rect.zero;
        }

        var mFilters = new List<MeshFilter>(go.GetComponentsInChildren<MeshFilter>());
        mFilters.RemoveAll(filter => filter.sharedMesh == null);
        var bounds = mFilters[0].sharedMesh.bounds;
        bounds.center = go.transform.TransformPoint(bounds.center);
        for (var i = 1; i < mFilters.Count; i++) {
            var b = mFilters[i].sharedMesh.bounds;
            b.center = go.transform.TransformPoint(b.center);
            bounds.Encapsulate(b);
        }

        var ext = bounds.extents;
        ext.Scale(go.transform.localScale);
        var screenPositions = new List<Vector2>();
        for (var x = -1; x < 2; x += 2) {
            for (var y = -1; y < 2; y += 2) {
                for (var z = -1; z < 2; z += 2) {
                    var vect = bounds.center + new Vector3(ext.x * x, ext.y * y, ext.z * z);
                    var wts = cam.WorldToScreenPoint(vect);
                    wts.y = Screen.height - wts.y;
                    screenPositions.Add(wts);
                }
            }
        }

        var rect = new Rect(screenPositions[0].x, screenPositions[0].y, 1, 1);
        Action<Vector2> FindMinMax = vec => {
            if (vec.x < rect.xMin) rect.xMin = vec.x;
            if (vec.x > rect.xMax) rect.xMax = vec.x;
            if (vec.y < rect.yMin) rect.yMin = vec.y;
            if (vec.y > rect.yMax) rect.yMax = vec.y;
        };
        screenPositions.ForEach(FindMinMax);
        return rect;
    }
    
    public static float IsAPointLeftOfVectorOrOnTheLine(Vector2 a, Vector2 b, Vector2 p)
    {
        float determinant = (a.x - p.x) * (b.y - p.y) - (a.y - p.y) * (b.x - p.x);

        return determinant;
    }
    
    private List<Vector2> DetermineConvexHull(IList<Vector2> points) {
        if (points.Count < 3) {
            return null;
        }

        var convexHull = new List<Vector2>();
        var startPoint = points[0];

        for (var i = 1; i < points.Count; i++) {
            var testPoint = points[i];

            if (testPoint.x < startPoint.x ||
                (Mathf.Approximately(testPoint.x, startPoint.x) && testPoint.y < startPoint.x)) {
                startPoint = points[i];
            }
        }
        
        convexHull.Add(startPoint);
        points.Remove(startPoint);

        var currentPoint = convexHull[0];
        var colinearPoints = new List<Vector2>();
        var counter = 0;

        while (true) {
            if (counter == 2) points.Add(convexHull[0]);

            var nextIndex = Random.Range(0, points.Count);
            var nextPoint = points[nextIndex];

            var a = currentPoint;
            var b = nextPoint;

            for (var i = 0; i < points.Count; i++) {
                if (points[i].Equals(nextPoint)) continue;

                var c = points[i];

                var relation = IsAPointLeftOfVectorOrOnTheLine(a, b, c);
                var accuracy = 0.00001f;

                if (relation < accuracy && relation > -accuracy) {
                    colinearPoints.Add(points[i]);
                } else if (relation < 0f) {
                    nextPoint = points[i];
                    b = points[i];
                    colinearPoints.Clear();
                }
            }

            if (colinearPoints.Count > 0) {
                colinearPoints.Add(nextPoint);
                colinearPoints = colinearPoints.OrderBy(n => Vector2.SqrMagnitude(n - currentPoint)).ToList();
                
                convexHull.AddRange(colinearPoints);
                currentPoint = colinearPoints[colinearPoints.Count - 1];

                for (var i = 0; i < colinearPoints.Count; i++) {
                    points.Remove(colinearPoints[i]);
                }
                
                colinearPoints.Clear();
            } else {
                convexHull.Add(nextPoint);
                points.Remove(nextPoint);
                currentPoint = nextPoint;
            }

            if (currentPoint.Equals(convexHull[0])) {
                convexHull.RemoveAt(convexHull.Count - 1);
                break;
            }

            counter += 1;
        }

        return convexHull;
    }

    void OnGUI() {
        // var r = GetScreenSpaceRectFromRendererBounds(target, Camera.main);
        // var r2 = GetScreenSpaceRectFromMeshBounds(target, Camera.main);
        // r.xMin = Mathf.Lerp(r.xMin, r2.xMin, RendererOrMeshBounds);
        // r.xMax = Mathf.Lerp(r.xMax, r2.xMax, RendererOrMeshBounds);
        // r.yMin = Mathf.Lerp(r.yMin, r2.yMin, RendererOrMeshBounds);
        // r.yMax = Mathf.Lerp(r.yMax, r2.yMax, RendererOrMeshBounds);
        //
        // GUI.Box(r, "");

        // var ps = DetermineConvexHull(GetScreenSpaceListFromRendererBounds(target, Camera.main));
        // foreach (var p in ps) {
        //     GUI.Box(new Rect(p.x, p.y, 0.05f, 0.05f), "");
        // }
    }

}
