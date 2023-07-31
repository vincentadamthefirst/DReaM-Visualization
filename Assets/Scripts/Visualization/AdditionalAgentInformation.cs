using System;
using UnityEngine;

namespace Visualization {
    public abstract class AdditionalAgentInformation {
        
        /// <summary>
        /// The glance type of the driver
        /// </summary>
        public string GlanceType { get; set; }
        
        /// <summary>
        /// The current CrossingPhase of the agent
        /// </summary>
        public string CrossingPhase { get; set; }
        
        /// <summary>
        /// The current Scan Area of Interest of the driver
        /// </summary>
        public string ScanAoI { get; set; }
        
        /// <summary>
        /// Other Agents that his agent sees at this point in time, initialized in world coordinates but gets converted
        /// into the agent-local coordinate system (agent location is point 0,0) for displaying
        /// </summary>
        public Tuple<string, Vector2, float>[] OtherAgents { get; set; }

    }
}