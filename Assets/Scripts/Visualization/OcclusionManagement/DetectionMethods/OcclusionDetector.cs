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

        public Dictionary<VisualizationElement, int> Distractors { get; } = new Dictionary<VisualizationElement, int>();

        public void SetTarget(VisualizationElement element, bool isTarget) {
            if (isTarget) {
                Targets.Add(element);
                element.HandleNonHit();
                if (Distractors.ContainsKey(element)) Distractors[element] = 0;
                return;
            }

            Targets.Remove(element);
        }
    }
}