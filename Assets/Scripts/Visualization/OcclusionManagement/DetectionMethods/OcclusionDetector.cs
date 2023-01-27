using System.Collections.Generic;
using System.Linq;
using Scenery;
using UnityEngine;
using Utils;

namespace Visualization.OcclusionManagement.DetectionMethods {
    public abstract class OcclusionDetector {

        /// <summary>
        /// The ExtendedCamera script for accessing the Camera view frustum
        /// </summary>
        public ExtendedCamera ExtendedCamera { get; set; }

        public AgentOcclusionManager AOM { get; set; }

        /// <summary>
        /// The list containing all current targets
        /// </summary>
        protected List<TargetableElement> Targets => AOM.Targets;

        /// <summary>
        /// Triggers the internal occlusion detection logic.
        /// </summary>
        public abstract void Trigger();

        public Dictionary<TargetableElement, HashSet<OccluderElement>> OccludersForTarget { get; } = new();

        public void TargetStatusChanged(object sender, bool isTarget) {
            var element = (TargetableElement) sender;
            if (isTarget) {
                // add the element as a target in the internally stored list
                Targets.Add(element);
                if (!OccludersForTarget.ContainsKey(element)) {
                    // the element is selected as a target for the first time
                    OccludersForTarget[element] = new HashSet<OccluderElement>();
                }
            } else {
                // the element is no longer a target, remove from internal list and decrease target occurence for all
                // occluders of this target
                foreach (var occluderInfo in OccludersForTarget[element].Where(DecreaseOcclusionOccurence)) {
                    // the element no longer occludes anything else, can be made completely visible again
                    occluderInfo.OcclusionEnd();
                }

                Targets.Remove(element);
            }
        }

        /// <summary>
        /// Decreases the occurence for a given element in the Dictionary for occluders. If the value changed from 1
        /// to 0 this method will return true.
        /// </summary>
        /// <param name="element">The occluder to decrease the occurence of</param>
        /// <returns>Whether the value for this element was 1.</returns>
        protected bool DecreaseOcclusionOccurence(OccluderElement element) {
            element.TargetsOccluded--;
            if (element.TargetsOccluded > 0) return false;
            element.TargetsOccluded = 0;
            return true;
        }

        /// <summary>
        /// Increases the amount of targets an object occludes and returns, whether the counter was 0 before. If the
        /// element is not yet present in the dictionary, it will add the element with value 1.
        /// </summary>
        /// <param name="element">The occluder to increase the occurence of</param>
        /// <returns>Whether the value for this element was 0.</returns>
        protected bool IncreaseOcclusionOccurence(OccluderElement element) {
            var toReturn = element.TargetsOccluded == 0;
            element.TargetsOccluded++;
            return toReturn;
        }
    }
}