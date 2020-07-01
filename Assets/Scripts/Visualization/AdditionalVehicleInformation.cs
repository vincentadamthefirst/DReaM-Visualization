using System;
using UnityEngine;

namespace Visualization {
    public class AdditionalVehicleInformation : AdditionalAgentInformation {
        
        /// <summary>
        /// The glance type of the driver
        /// </summary>
        public GlanceType GlanceType { get; set; }
        
        /// <summary>
        /// The gaze points that his agent currently has, might be empty
        /// </summary>
        public Vector2[] GazePoints { get; set; }
        
        /// <summary>
        /// The current indicator light state
        /// </summary>
        public IndicatorState IndicatorState { get; set; }
        
        /// <summary>
        /// Other Agents that his agent sees at this point in time
        /// </summary>
        public Tuple<Vector2, float, float>[] OtherAgents { get; set; }
        
        /// <summary>
        /// The WheelRotation at this sample
        /// </summary>
        public float WheelRotation { get; set; }
    }
}