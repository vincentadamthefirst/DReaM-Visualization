using System;
using System.Collections.Generic;
using System.Linq;
using Scenery;
using Scenery.RoadNetwork.RoadObjects;
using UI.Visualization;
using UnityEngine;
using Utils;
using Visualization.OcclusionManagement.DetectionMethods;

namespace Visualization.OcclusionManagement {
    
    /// <summary>
    /// Manages and triggers the occlusion detector.
    /// </summary>
    public class AgentOcclusionManager : MonoBehaviour {
        /// <summary>
        /// If the occlusion management should be disabled.
        /// </summary>
        public bool Disable { get; set; }

        public Dictionary<Collider, VisualizationElement> AllColliders { get; } = new();

        private OccluderElement[] _allOccluders = Array.Empty<OccluderElement>();
        private TargetableElement[] _allTargetable = Array.Empty<TargetableElement>();

        private ExtendedCamera _extendedCamera;
        private VisualizationMenu _visualizationMenu;
        private RayCastDetector Detector { get; } = new();

        public List<TargetableElement> Targets { get; } = new();


        /// <summary>
        /// Finds all necessary objects in the scene. This method has to be called BEFORE any other method.
        /// </summary>
        public void FindAll() {
            _extendedCamera = FindObjectOfType<ExtendedCamera>();
            _visualizationMenu = FindObjectOfType<VisualizationMenu>();
        }
        
        /// <summary>
        /// Prepares this class by finding all colliders and all possible distractors. Forwards this information to
        /// the TargetController and SettingsControl. Must be called AFTER FindAll().
        /// </summary>
        public void Prepare() {
            // setting up the detector
            Detector.ExtendedCamera = _extendedCamera;
            foreach (var target in Targets) {
                Detector.TargetStatusChanged(target, true);
            }
            Detector.AOM = this;
            
            // finding all colliders in scene
            var colliders = FindObjectsOfType<Collider>();
            foreach (var coll in colliders) {
                var tmp = coll.GetComponentInParent<VisualizationElement>();
                if (tmp == null) continue;
                AllColliders[coll] = tmp;
            }

            // Debug.Log("Colliders Mapped: "); TODO fix agent occlusion
            // foreach (var pair in AllColliders) {
            //     Debug.Log(pair.Value.name); 
            // }

            // finding all possible targets & occluders
            _allTargetable = FindObjectsOfType<TargetableElement>().ToArray();
            foreach (var targetable in _allTargetable) {
                targetable.TargetStatusChanged += Detector.TargetStatusChanged;
            }
            _allOccluders =  FindObjectsOfType<OccluderElement>().ToArray();
            
            // adding the objects to the settings
            foreach (var visualizationElement in _allOccluders) {
                if (visualizationElement.GetType().IsSubclassOf(typeof(RoadObject))) {
                    _visualizationMenu.Elements.Add(visualizationElement);
                }
            }

            // enabling the occlusion management 
            Disable = false;
        }

        /// <summary>
        /// After the position of all objects was updated the occlusion detection will be performed.
        /// </summary>
        private void LateUpdate() {
            if (Disable) return; // disable this process
            Detector.Trigger();
        }
    }
}