using System.Collections.Generic;
using Meta.Numerics.Functions;
using Scenery;
using UnityEditor;
using UnityEngine;
using Utils;

namespace Visualization.OcclusionManagement.DetectionMethods {
    public abstract class OcclusionDetector {
        
        /// <summary>
        /// The overall options for managing occlusions
        /// </summary>
        public OcclusionManagementOptions OcclusionManagementOptions { get; set; }
        
        /// <summary>
        /// The list containing all current targets
        /// </summary>
        public List<VisualizationElement> Targets { get; } = new List<VisualizationElement>();

        /// <summary>
        /// The ExtendedCamera script for accessing the Camera view frustum
        /// </summary>
        public ExtendedCamera ExtendedCamera { get; set; }

        /// <summary>
        /// Triggers the internal occlusion detection logic.
        /// </summary>
        public abstract void Trigger();

        // layer mask for RayCasts
        public LayerMask LayerMask { get; set; }
        
        public Dictionary<Collider, VisualizationElement> ColliderMapping { get; set; }
        
        public Dictionary<VisualizationElement, HashSet<VisualizationElement>> LastHits { get; } =
            new Dictionary<VisualizationElement, HashSet<VisualizationElement>>();

        /// <summary>
        /// Every possible distractor mapped to an integers describing the amount of target objects that this distractor
        /// occludes at the time
        /// </summary>
        public Dictionary<VisualizationElement, int> DistractorCounts { get; } = new Dictionary<VisualizationElement, int>();
        
        /// <summary>
        /// Every possible distractor in the scene
        /// </summary>
        public List<VisualizationElement> OnlyDistractors { get; } = new List<VisualizationElement>();

        public void SetTarget(VisualizationElement element, bool isTarget) {
            if (isTarget) {
                Targets.Add(element);
                element.HandleNonHit();
                if (DistractorCounts.ContainsKey(element)) DistractorCounts[element] = 0;
                return;
            }

            Targets.Remove(element);
        }
        
        /// <summary>
        /// Decreases the occurence for a given element in the Dictionary for distractors. If the value changed from 1
        /// to 0 this method will return true.
        /// </summary>
        /// <param name="element">The element to decrease the occurence of</param>
        /// <returns>Whether the value for this element was 1.</returns>
        protected bool DecreaseDistractorEntry(VisualizationElement element) {
            DistractorCounts[element]--;
            if (DistractorCounts[element] > 0) return false;
            DistractorCounts[element] = 0;
            return true;
        }

        /// <summary>
        /// Increases the amount of targets an object occludes and returns, whether the counter was 0 before. If the
        /// element is not yet present in the dictionary, it will add the element with value 1.
        /// </summary>
        /// <param name="element">The element to increase the occurence of</param>
        /// <returns>Whether the value for this element was 0.</returns>
        protected bool IncreaseDistractorEntry(VisualizationElement element) {
            var toReturn = DistractorCounts[element] == 0;
            DistractorCounts[element]++;
            return toReturn;
        }
    }
}