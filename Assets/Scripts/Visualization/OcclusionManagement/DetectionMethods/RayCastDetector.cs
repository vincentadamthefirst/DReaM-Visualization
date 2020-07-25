﻿using System;
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
            if (!target.IsActive || !GeometryUtility.TestPlanesAABB(
                    ExtendedCamera.CurrentFrustumPlanes, target.AxisAlignedBoundingBox)) {
                
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

                // ReSharper disable once Unity.PreferNonAllocApi
                var currentHits = Physics.RaycastAll(startPoints[i], directionVector, distance);

                Debug.DrawLine(startPoints[i], endPoints[i]);

                foreach (var hit in currentHits) {
                    var hitObject = ColliderMapping[hit.collider];
                    if (hitObject == null || hitObject.IsTarget()) continue;
                    
                    newHits.Add(hitObject);
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
                        OcclusionManagementOptions.nearClipPlaneAsStart ? ExtendedCamera.Camera.nearClipPlane : 0))
                    : new Vector3(result.x, result.y, 0);
            }
            return toReturn;
        }
    }
}