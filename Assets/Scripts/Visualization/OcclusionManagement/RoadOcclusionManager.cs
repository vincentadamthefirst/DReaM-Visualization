using System.Collections.Generic;
using Scenery;
using UnityEngine;

namespace Visualization.OcclusionManagement {
    
    /// <summary>
    /// This class gets updated by each agent, telling this class what road the agent is on. Based on this information
    /// it will move certain roads to the road_targets layer to display them through other objects.
    /// </summary>
    public class RoadOcclusionManager : MonoBehaviour {
        private readonly HashSet<VisualizationElement> _currentRoadIds = new HashSet<VisualizationElement>();

        /// <summary>
        /// Adds a road Id to the set containing all active roads for the current point in time.
        /// </summary>
        /// <param name="element">The currently active element</param>
        public void AddOnElement(VisualizationElement element) {
            _currentRoadIds.Add(element);
        }
    }
}