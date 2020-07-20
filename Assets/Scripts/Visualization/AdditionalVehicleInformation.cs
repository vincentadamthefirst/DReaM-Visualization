using System;
using UnityEngine;

namespace Visualization {
    public class AdditionalVehicleInformation : AdditionalAgentInformation {

        /// <summary>
        /// The gaze points that his agent currently has, might be empty
        /// </summary>
        public Vector2[] GazePoints { get; set; }
        
        /// <summary>
        /// The current indicator light state
        /// </summary>
        public IndicatorState IndicatorState { get; set; }

        /// <summary>
        /// The WheelRotation at this sample
        /// </summary>
        public float WheelRotation { get; set; }
        
        /// <summary>
        /// If the agent is currently braking
        /// </summary>
        public bool Brake { get; set; }
    }
}