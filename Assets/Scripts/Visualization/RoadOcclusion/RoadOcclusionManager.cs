using System;
using System.Collections.Generic;
using System.Linq;
using Scenery;
using Scenery.RoadNetwork;
using UnityEngine;
using Utils;
using Visualization.Agents;

namespace Visualization.RoadOcclusion {
    
    /// <summary>
    /// This class gets updated by each agent, telling this class what road the agent is on. Based on this information
    /// it will move certain roads to the road_targets layer to display them through other objects.
    /// </summary>
    public class RoadOcclusionManager : MonoBehaviour {

        private HashSet<SceneryElement> _activeRoadIds = new HashSet<SceneryElement>();
        
        private readonly HashSet<SceneryElement> _currentRoadIds = new HashSet<SceneryElement>();

        private const int RoadBaseLayer = 17;
        private const int RoadTargetLayer = 13;

        /// <summary>
        /// Finds roads and junctions that need to be moved and moves them into the necessary layer.
        /// </summary>
        public void ChangeRoadLayers() {
            var toBaseLayer = new HashSet<SceneryElement>(_activeRoadIds);
            toBaseLayer.ExceptWith(_currentRoadIds);
            
            foreach (var sceneryElement in toBaseLayer) {
                sceneryElement.SetLayer(RoadBaseLayer);
            }
            
            var toTargetLayer = new HashSet<SceneryElement>(_currentRoadIds);
            toTargetLayer.ExceptWith(_activeRoadIds);

            foreach (var sceneryElement in toTargetLayer) {
                sceneryElement.SetLayer(RoadTargetLayer);
            }

            _activeRoadIds = new HashSet<SceneryElement>(_currentRoadIds);
            _currentRoadIds.Clear();
        }

        /// <summary>
        /// Adds a road Id to the set containing all active roads for the current point in time.
        /// </summary>
        /// <param name="elementId">The currently active element</param>
        public void AddOnElement(SceneryElement element) {
            _currentRoadIds.Add(element);
        }
    }
}