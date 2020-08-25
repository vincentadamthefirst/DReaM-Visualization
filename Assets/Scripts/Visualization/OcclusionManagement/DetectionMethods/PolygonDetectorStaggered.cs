using System.Collections.Generic;
using System.Linq;
using Scenery;
using UnityEngine;
using Utils.AdditionalMath;

namespace Visualization.OcclusionManagement.DetectionMethods {
    public class PolygonDetectorStaggered : PolygonDetector {

        private int _lastTarget;

        public override void Trigger() {
            if (Targets.Count == 0) return;
            if (_lastTarget > Targets.Count - 1) _lastTarget = 0;

            var target = Targets[_lastTarget];
            
            if (!target.IsActive || !GeometryUtility.TestPlanesAABB(ExtendedCamera.CurrentFrustumPlanes, 
                    target.AxisAlignedBoundingBox)) {
                // target no longer active or outside of the camera view

                if (!LastHits.ContainsKey(target)) return;

                // removing any occluding objects that this agent had
                foreach (var lastHit in LastHits[target]) {
                    if (DecreaseDistractorEntry(lastHit)) {
                        lastHit.HandleNonHit();
                    }
                }

                LastHits[target].Clear();
                _lastTarget++;
                return;
            }

            DetectionMeasurement.StartMeasurement();
            
            var targetPolygon = ConstructPolygon(target);
            var distanceToTarget = Vector3.Distance(ExtendedCamera.Camera.transform.position,
                target.AxisAlignedBoundingBox.center);

            var newHits = new HashSet<VisualizationElement>();

            foreach (var element in ColliderMapping.Values) {
                var inViewFrustum = OcclusionManagementOptions.preCheckViewFrustum &&
                                    GeometryUtility.TestPlanesAABB(ExtendedCamera.CurrentFrustumPlanes,
                                        element.AxisAlignedBoundingBox);
                var distanceToDistractor = Vector3.Distance(ExtendedCamera.Camera.transform.position,
                    element.AxisAlignedBoundingBox.center);

                if (!inViewFrustum || element.IsTarget() || distanceToDistractor > distanceToTarget) {
                    continue;
                }

                var currentPolygon = ConstructPolygon(element);

                if (currentPolygon.DoesCollide(targetPolygon)) {
                    newHits.Add(element);
                }
            }
            
            DetectionMeasurement.EndMeasurement();
            HandlingMeasurement.StartMeasurement();
            
            var lastHits = new HashSet<VisualizationElement>(LastHits[target]);

            var actualNewHits = new HashSet<VisualizationElement>(newHits);
            actualNewHits.ExceptWith(lastHits);

            foreach (var hitObject in actualNewHits.Where(IncreaseDistractorEntry)) {
                hitObject.HandleHit();
            }

            lastHits.ExceptWith(newHits);

            foreach (var noLongerHitObject in lastHits.Where(DecreaseDistractorEntry)) {
                noLongerHitObject.HandleNonHit();
            }

            LastHits[target] = newHits;
            
            _lastTarget++;
            
            HandlingMeasurement.EndMeasurement();
        }
    }
}