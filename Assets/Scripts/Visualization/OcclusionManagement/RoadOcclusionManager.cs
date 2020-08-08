using System;
using System.Collections.Generic;
using Evaluation;
using Scenery;
using UnityEngine;

namespace Visualization.OcclusionManagement {
    
    /// <summary>
    /// This class gets updated by each agent, telling this class what road the agent is on. Based on this information
    /// it will move certain roads to the road_targets layer to display them through other objects.
    /// </summary>
    public class RoadOcclusionManager : MonoBehaviour {
        
        public ExecutionMeasurement ExecutionMeasurement { get; set; }

        private HashSet<VisualizationElement> _activeRoadIds = new HashSet<VisualizationElement>();
        
        private readonly HashSet<VisualizationElement> _currentRoadIds = new HashSet<VisualizationElement>();

        private const int RoadBaseLayer = 17;
        private const int RoadTargetLayer = 13;

        /// <summary>
        /// Finds roads and junctions that need to be moved and moves them into the necessary layer.
        /// </summary>
        public void ChangeRoadLayers() {
            ExecutionMeasurement.StartMeasurement();
            
            var toBaseLayer = new HashSet<VisualizationElement>(_activeRoadIds);
            toBaseLayer.ExceptWith(_currentRoadIds);
            
            foreach (var sceneryElement in toBaseLayer) {
                try {
                    sceneryElement.SetLayer(RoadBaseLayer);
                } catch (Exception e) {
                    // ignore roads that can not be found because of errors in the output
                }
            }
            
            var toTargetLayer = new HashSet<VisualizationElement>(_currentRoadIds);
            toTargetLayer.ExceptWith(_activeRoadIds);

            foreach (var sceneryElement in toTargetLayer) {
                try {
                    sceneryElement.SetLayer(RoadTargetLayer);
                } catch (Exception e) {
                    // ignore roads that can not be found because of errors in the output
                }
            }

            _activeRoadIds = new HashSet<VisualizationElement>(_currentRoadIds);
            _currentRoadIds.Clear();
            
            ExecutionMeasurement.EndMeasurement();
        }

        /// <summary>
        /// Adds a road Id to the set containing all active roads for the current point in time.
        /// </summary>
        /// <param name="element">The currently active element</param>
        public void AddOnElement(VisualizationElement element) {
            _currentRoadIds.Add(element);
        }
    }
}