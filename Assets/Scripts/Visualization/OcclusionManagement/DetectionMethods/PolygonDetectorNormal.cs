using System.Collections.Generic;
using System.Linq;
using Scenery;
using UnityEngine;
using Utils.AdditionalMath;

namespace Visualization.OcclusionManagement.DetectionMethods {
    public class PolygonDetectorNormal : PolygonDetector {
        public override void Trigger() {
            if (Targets.Count == 0) return;

            var currentPolygons = new Dictionary<Polygon, VisualizationElement>();

            foreach (var element in ColliderMapping.Values) {
                var inViewFrustum = OcclusionManagementOptions.preCheckViewFrustum &&
                                    GeometryUtility.TestPlanesAABB(ExtendedCamera.CurrentFrustumPlanes,
                                        element.AxisAlignedBoundingBox);

                if (!inViewFrustum || element.IsTarget()) {
                    continue;
                }
                
                currentPolygons.Add(ConstructPolygon(element), element);
            }

            foreach (var target in Targets) {
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
                    continue;
                }

                var targetPolygon = ConstructPolygon(target);
                var distanceToTarget = Vector3.Distance(ExtendedCamera.Camera.transform.position,
                    target.AxisAlignedBoundingBox.center);

                var newHits = new HashSet<VisualizationElement>();
                
                foreach (var entry in currentPolygons) {
                    var distanceToDistractor = Vector3.Distance(ExtendedCamera.Camera.transform.position,
                        entry.Value.AxisAlignedBoundingBox.center);

                    if (distanceToDistractor > distanceToTarget) continue; // distractor is behind target
                    
                    if (entry.Key.DoesCollide(targetPolygon)) {
                        newHits.Add(entry.Value);
                    }
                }

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
            }
        }
    }
}