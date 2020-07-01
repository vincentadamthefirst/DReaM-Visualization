﻿using System.Collections.Generic;
using UnityEngine;

namespace Visualization {
    public class SimulationStep {
        /// <summary>
        /// The next SimulationStep, might be null if this is the last step.
        /// </summary>
        public SimulationStep Next { get; set; }
        
        /// <summary>
        /// The time (in ms) of this SimulationStep.
        /// </summary>
        public int Time { get; set; }
        
        /// <summary>
        /// The position of the Agent at this step.
        /// </summary>
        public Vector2 Position { get; set; }
        
        /// <summary>
        /// The rotation of the Agent at this step
        /// </summary>
        public float Rotation { get; set; }
        
        /// <summary>
        /// The velocity of the agent at this step
        /// </summary>
        public float Velocity { get; set; }
        
        /// <summary>
        /// The acceleration of the agent at this step
        /// </summary>
        public float Acceleration { get; set; }
        
        /// <summary>
        /// The information of this Agents sensors at this step.
        /// </summary>
        public Dictionary<string, SensorInformation> SensorInformation { get; set; }
        
        /// <summary>
        /// The additional information for an agent at this SimulationStep
        /// </summary>
        public AdditionalAgentInformation AdditionalInformation { get; set; }
    }

    public abstract class SensorInformation {
        /// <summary>
        /// The relative position of the sensor towards the center of the vehicle
        /// </summary>
        public Vector2 RelativePosition { get; set; }
        
        /// <summary>
        /// The name of this sensor
        /// </summary>
        public string SensorName { get; set; }

        /// <summary>
        /// Tells whether the data of this Sensor Changed towards the previous SimulationStep
        /// </summary>
        public bool ChangedTowardsPrevious { get; set; }
        
        /// <summary>
        /// Tells whether the data of this Sensor Changed towards the next SimulationStep
        /// </summary>
        public bool ChangedTowardsNext { get; set; }
    }

    public class BasicConeSensorInformation : SensorInformation {
        /// <summary>
        /// Global heading the sensor looks at.
        /// </summary>
        public float Heading { get; set; }
        
        /// <summary>
        /// The Opening angle of the complete sensor
        /// </summary>
        public float OpeningAngle { get; set; }
        
        /// <summary>
        /// The distance that the sensor cone covers
        /// </summary>
        public float Distance { get; set; }
    }
}