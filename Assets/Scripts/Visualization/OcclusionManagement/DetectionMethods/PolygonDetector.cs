using Scenery;
using UnityEngine;
using Utils.AdditionalMath;

namespace Visualization.OcclusionManagement.DetectionMethods {
    public abstract class PolygonDetector : OcclusionDetector {
        
        protected Polygon ConstructPolygon(VisualizationElement element) {
            var scenePoints = element.GetReferencePoints();
            var screenPoints = GetScreenPoints(scenePoints);
            var polygonPoints = ConvexHull.GrahamScanCompute(screenPoints);
            
            return new Polygon(polygonPoints);
        }

        private Vector2[] GetScreenPoints(Vector3[] scenePoints) {
            var toReturn = new Vector2[scenePoints.Length];
            for (var i = 0; i < scenePoints.Length; i++) {
                var result = ExtendedCamera.Camera.WorldToScreenPoint(scenePoints[i]);
                result.y = Screen.height - result.y;
                toReturn[i] = new Vector2(result.x, result.y);
            }
            return toReturn;
        }
    }
}