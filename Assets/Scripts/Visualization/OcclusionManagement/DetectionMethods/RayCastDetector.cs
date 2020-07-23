using System;
using System.Collections.Generic;
using System.Linq;
using Scenery;
using UnityEditor;
using UnityEngine;
using Random = System.Random;

namespace Visualization.OcclusionManagement.DetectionMethods {
    public abstract class RayCastDetector : OcclusionDetector {

        private readonly Random _random = new Random(Environment.TickCount);

        protected void CastRay(VisualizationElement target) {
            if (!GeometryUtility.TestPlanesAABB(ExtendedCamera.CurrentFrustumPlanes,
                new Bounds(target.WorldAnchor, Vector3.zero))) {
                
                if (!LastHits.ContainsKey(target)) return;
                
                foreach (var lastHit in LastHits[target]) {
                    if (DecreaseDistractorEntry(lastHit)) {
                        lastHit.HandleNonHit();
                    }
                }
                
                LastHits[target].Clear();
                return;
            }

            var endPoints = GetEndPoints(target);
            var startPoints = GetStartPoints(endPoints);
            
            var newHits = new HashSet<VisualizationElement>();
            
            for (var i = 0; i < startPoints.Length; i++) {
                var directionVector = endPoints[i] - startPoints[i];
                var distance = Vector3.Distance(startPoints[i], endPoints[i]);

                // var currentHits = OcclusionManagementOptions.nearClipPlaneAsStart
                //     ? Physics.RaycastAll(ExtendedCamera.Camera.ScreenPointToRay(startPoints[i]), distance, LayerMask)
                //     : Physics.RaycastAll(ExtendedCamera.Camera.transform.position, endPoints[i], distance, LayerMask);

                var currentHits = Physics.RaycastAll(startPoints[i], directionVector, distance);

                //Debug.DrawLine(startPoints[i], endPoints[i]);

                foreach (var hit in currentHits) {
                    var hitObject = hit.collider.GetComponent<VisualizationElement>(); //ColliderMapping[hit.collider];
                    if (hitObject == null || hitObject.IsTarget()) return;
                    //Debug.Log(hit.collider.name);
                    
                    newHits.Add(hitObject);
                }
            }
            
            //Debug.Log("Found " + newHits.Count + " objects");

            var lastHits = LastHits[target];

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

        /// <summary>
        /// Decreases the occurence for a given element in the Dictionary for distractors. If the value changed from 1
        /// to 0 this method will return true.
        /// </summary>
        /// <param name="element">The element to decrease the occurence of</param>
        /// <returns>Whether the value for this element was 1.</returns>
        private bool DecreaseDistractorEntry(VisualizationElement element) {
            if (Distractors[element] == 1) {
                Distractors[element] = 0;
                return true;
            }

            Distractors[element] = Distractors[element] > 1 ? Distractors[element]-- : 0;
            return false;
        }

        /// <summary>
        /// Increases the amount of targets an object occludes and returns, whether the counter was 0 before. If the
        /// element is not yet present in the dictionary, it will add the element with value 1.
        /// </summary>
        /// <param name="element">The element to increase the occurence of</param>
        /// <returns>Whether the value for this element was 0.</returns>
        private bool IncreaseDistractorEntry(VisualizationElement element) {
            if (!Distractors.ContainsKey(element)) {
                Distractors.Add(element, 1);
                return true;
            }
            
            var toReturn = Distractors[element] == 0;
            Distractors[element]++;
            return toReturn;
        }

        private Vector3[] SelectRandom(IEnumerable<Vector3> input) {
            return input.ToList().OrderBy(x => _random.Next()).Take(OcclusionManagementOptions.randomPointAmount)
                .ToArray();
        }

        private Vector3[] GetEndPoints(VisualizationElement target) {
            var basePoints = target.GetReferencePoints();
            return OcclusionManagementOptions.sampleRandomPoints ? SelectRandom(basePoints) : basePoints;
        }

        private Vector3[] GetStartPoints(Vector3[] endPoints) {
            var toReturn = new Vector3[endPoints.Length];
            for (var i = 0; i < endPoints.Length; i++) {
                var result = ExtendedCamera.Camera.WorldToScreenPoint(endPoints[i]);
                result.y = Screen.height - result.y;
                toReturn[i] = OcclusionManagementOptions.nearClipPlaneAsStart
                    ? ExtendedCamera.Camera.ScreenToWorldPoint(new Vector3(result.x, result.y,
                        ExtendedCamera.Camera.nearClipPlane))
                    : new Vector3(result.x, result.y, 0);
            }
            return toReturn;
        }
    }
}