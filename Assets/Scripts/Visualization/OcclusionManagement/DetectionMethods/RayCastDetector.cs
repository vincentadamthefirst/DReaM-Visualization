using System.Collections.Generic;
using System.Linq;
using Scenery;
using UnityEngine;

namespace Visualization.OcclusionManagement.DetectionMethods {
    /**
     * Implementation of a simple staggered raycast detector.
     * At each frame a new target is handled. The workload is therefore spread across multiple frames and has less
     * impact on the application performance.
     */
    public class RayCastDetector : OcclusionDetector {
        private int _lastTarget;

        public override void Trigger() {
            if (Targets.Count == 0) return;
            if (_lastTarget > Targets.Count - 1) _lastTarget = 0;

            CastRay(Targets[_lastTarget]);

            _lastTarget++;
        }

        private void CastRay(TargetableElement element) {
            if (!element.IsTarget || !GeometryUtility.TestPlanesAABB(
                    ExtendedCamera.CurrentFrustumPlanes, element.AABB)) {
                // the element is no longer a target or in the view frustum of the camera
                foreach (var currentOccluder in OccludersForTarget[element].Where(DecreaseOcclusionOccurence)) {
                    currentOccluder.OcclusionEnd();
                }

                // clear the list of occluders for this element
                OccludersForTarget[element].Clear();
                return;
            }

            var endPoints = GetEndPoints(element);
            var startPoints = GetStartPoints(endPoints);
            
            var newHits = new HashSet<OccluderElement>();

            // finding all occluders whose colliders are currently hit
            for (var i = 0; i < startPoints.Length; i++) {
                var directionVector = endPoints[i] - startPoints[i];
                var distance = Vector3.Distance(startPoints[i], endPoints[i]);

                // ReSharper disable once Unity.PreferNonAllocApi
                var currentHits = Physics.RaycastAll(startPoints[i], directionVector, distance);

                foreach (var hit in currentHits) {
                    var hitOccluder = hit.collider?.transform.parent?.GetComponent<OccluderElement>();
                    if (hitOccluder == null) continue;
                    newHits.Add(hitOccluder);
                }
            }

            // the occluders of the previous step
            var lastHits = new HashSet<OccluderElement>(OccludersForTarget[element]);

            // what elements are actual new occluders
            var actualNewHits = new HashSet<OccluderElement>(newHits);
            actualNewHits.ExceptWith(lastHits);
            foreach (var hitObject in actualNewHits.Where(IncreaseOcclusionOccurence)) {
                hitObject.OcclusionStart();
            }

            // what elements are no longer occluders
            lastHits.ExceptWith(newHits);
            foreach (var noLongerHitObject in lastHits.Where(DecreaseOcclusionOccurence)) {
                noLongerHitObject.OcclusionEnd();
            }

            OccludersForTarget[element] = newHits;
        }

        private Vector3[] GetEndPoints(TargetableElement target) {
            var basePoints = target.GetReferencePoints();
            return basePoints;
        }

        private Vector3[] GetStartPoints(Vector3[] endPoints) {
            var toReturn = new Vector3[endPoints.Length];
            for (var i = 0; i < endPoints.Length; i++) {
                var result = ExtendedCamera.Camera.WorldToScreenPoint(endPoints[i]);
                result.y = Screen.height - result.y;
                // TODO really use near clip plane?
                toReturn[i] =
                    ExtendedCamera.Camera.ScreenToWorldPoint(new Vector3(result.x, result.y,
                        ExtendedCamera.Camera.nearClipPlane));
            }
            return toReturn;
        }
    }
}