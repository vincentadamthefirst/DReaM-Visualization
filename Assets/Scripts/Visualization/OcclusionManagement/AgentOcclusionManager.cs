using System;
using System.Collections.Generic;
using System.Linq;
using Evaluation;
using Scenery;
using Scenery.RoadNetwork.RoadObjects;
using UI;
using UnityEngine;
using Utils;
using Visualization.Agents;
using Visualization.OcclusionManagement.DetectionMethods;

namespace Visualization.OcclusionManagement {
    
    /// <summary>
    /// Class managing the occlusion of Agents in the scene by RoadObjects.
    /// </summary>
    public class AgentOcclusionManager : MonoBehaviour {
        
        /// <summary>
        /// The class measuring the handling time for this manager.
        /// </summary>
        public ExecutionMeasurement DetectionMeasurement { get; set; }
        
        /// <summary>
        /// The class measuring the detection time for this manager.
        /// </summary>
        public ExecutionMeasurement HandlingMeasurement { get; set; }
        
        /// <summary>
        /// If the occlusion management should be disabled.
        /// </summary>
        public bool Disable { get; set; }

        /// <summary>
        /// The settings to be used for occlusion management.
        /// </summary>
        public OcclusionManagementOptions OcclusionManagementOptions { get; set; }
        
        // the currently used OcclusionDetector
        private OcclusionDetector _occlusionDetector;

        // mapping a collider in the scene to its visualization element
        private readonly Dictionary<Collider, VisualizationElement> _colliderMapping =
            new Dictionary<Collider, VisualizationElement>();

        // all elements in the scene
        private VisualizationElement[] _allElements = new VisualizationElement[0];

        // the scenes main camera extension
        private ExtendedCamera _extendedCamera;

        // the target selection controller
        private TargetController _targetController;

        // the settings panel controller
        private SettingsController _settingsController;

        // the types of the occlusion detectors to be initialized
        private static readonly Type[][] OcclusionDetectors = {
            new [] {typeof(RayCastDetectorNormal), typeof(RayCastDetectorStaggered)}, 
            new [] {typeof(PolygonDetectorNormal), typeof(PolygonDetectorStaggered)}
        };

        /// <summary>
        /// Finds all necessary objects in the scene. This method has to be called BEFORE any other method.
        /// </summary>
        public void FindAll() {
            _extendedCamera = FindObjectOfType<ExtendedCamera>();
            _targetController = FindObjectOfType<TargetController>();
            _settingsController = FindObjectOfType<SettingsController>();
        }

        /// <summary>
        /// Method to be called when the settings have been changed in a major way. Builds a new occlusion detector.
        /// </summary>
        public void MajorUpdate() {
            if (_occlusionDetector != null) {
                // there already was a detector, resetting all elements to non hit
                foreach (var visualizationElement in _allElements) {
                    visualizationElement.HandleNonHit();
                }
            }
            
            if (OcclusionManagementOptions.occlusionDetectionMethod == OcclusionDetectionMethod.Shader) {
                //GraphicsSettings.renderPipelineAsset = aotPipelineAsset;
                //GraphicsSettings.defaultRenderPipeline = aotPipelineAsset;
                
                _extendedCamera.cameraData.SetRenderer(0);
                
                // disabling the occlusion management in code
                Disable = true;
            } else {
                //GraphicsSettings.renderPipelineAsset = normalPipelineAsset;
                //GraphicsSettings.defaultRenderPipeline = normalPipelineAsset;
                
                _extendedCamera.cameraData.SetRenderer(1);

                _occlusionDetector = (OcclusionDetector) Activator.CreateInstance(
                    OcclusionDetectors[(int) OcclusionManagementOptions.occlusionHandlingMethod][
                        OcclusionManagementOptions.staggeredCheck ? 1 : 0]);
                
                // setting the base parameters
                _occlusionDetector.ExtendedCamera = _extendedCamera;
                _occlusionDetector.OcclusionManagementOptions = OcclusionManagementOptions;
            
                _occlusionDetector.ColliderMapping = _colliderMapping;

                // forwarding the measurement objects to the new occlusion detector
                _occlusionDetector.DetectionMeasurement = DetectionMeasurement;
                _occlusionDetector.HandlingMeasurement = HandlingMeasurement;

                foreach (var visualizationElement in _allElements) {
                    // adding all VisualizationElements
                    if (!visualizationElement.IsDistractor) continue;
                    _occlusionDetector.DistractorCounts[visualizationElement] = 0;
                }

                foreach (var target in _targetController.Targets) {
                    _occlusionDetector.SetTarget(target, true);
                }
                
                // enabling the occlusion management 
                Disable = false;
            }
        }

        /// <summary>
        /// Method called when there is a minor update to the settings. Only Changes the method of handling occlusion.
        /// </summary>
        public void MinorUpdate() {
            if (OcclusionManagementOptions.occlusionDetectionMethod == OcclusionDetectionMethod.Shader) return;
            
            var toChange = new List<VisualizationElement>();
            foreach (var entry in _occlusionDetector.DistractorCounts) {
                entry.Key.HandleNonHit();
                entry.Key.SetupOccludedMaterials();
                toChange.Add(entry.Key);
            }
            
            toChange.ForEach(x => _occlusionDetector.DistractorCounts[x] = 0);
        }

        /// <summary>
        /// Prepares this class by finding all colliders and all possible distractors. Forwards this information to
        /// the TargetController and SettingsControl. Must be called AFTER FindAll().
        /// </summary>
        public void Prepare() {
            // finding all colliders in scene
            var colliders = FindObjectsOfType<Collider>();
            foreach (var coll in colliders) {
                var tmp = coll.GetComponentInParent<VisualizationElement>();

                if (tmp == null) continue;
                _colliderMapping[coll] = tmp;
            }
            
            // setting the collider mapping for the target controller
            _targetController.ColliderMapping = _colliderMapping;
            
            // finding all VisualizationElements
            _allElements = FindObjectsOfType<VisualizationElement>().Where(e => e.transform.childCount > 0).ToArray();
            
            // adding the objects to the settings
            foreach (var visualizationElement in _allElements) {
                if (visualizationElement.GetType().IsSubclassOf(typeof(RoadObject))) {
                    _settingsController.Elements.Add(visualizationElement);
                }
            }
            
            _settingsController.Rebuild();
            
            MajorUpdate();
        }

        /// <summary>
        /// Sets the target status of a specific Agent in the current class for detecting occlusions.
        /// </summary>
        /// <param name="element">The Agent of which to change the target status</param>
        /// <param name="isTarget">The new target status</param>
        public void SetTarget(Agent element, bool isTarget) {
            if (OcclusionManagementOptions.occlusionDetectionMethod == OcclusionDetectionMethod.Shader) return;
            
            _occlusionDetector.SetTarget(element, isTarget);
        }

        /// <summary>
        /// Sets all Agents in the scene as a target.
        /// </summary>
        public void SetAllTargets() {
            if (OcclusionManagementOptions.occlusionDetectionMethod == OcclusionDetectionMethod.Shader) {
                foreach (var agent in FindObjectsOfType<Agent>()) {
                    agent.SetIsTarget(true);
                }
                return;
            }

            foreach (var agent in FindObjectsOfType<Agent>()) {
                agent.SetIsTarget(true);
                _occlusionDetector.SetTarget(agent, true);
            }
        }

        /// <summary>
        /// After the position of all objects was updated the occlusion detection will be performed.
        /// </summary>
        private void LateUpdate() {
            if (Disable) return; // disable this process
            _occlusionDetector.Trigger();
        }
    }
}