using System.Collections.Generic;
using UnityEngine;
using Visualization.SimulationEvents;

namespace Visualization {

    public class SimulationStep {
        /// <summary>
        /// The next SimulationStep, might be null if this is the last step.
        /// </summary>
        public SimulationStep Next { get; set; }
        
        /// <summary>
        /// The previous SimulationStep, might be null if this is the first step.
        /// </summary>
        public SimulationStep Previous { get; set; }
        
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
        /// The information of this Agents sensors at this step.
        /// </summary>
        public Dictionary<string, SensorInformation> SensorInformation { get; } = new Dictionary<string, SensorInformation>();
        
        /// <summary>
        /// The additional information for an agent at this SimulationStep
        /// </summary>
        public AdditionalAgentInformation AdditionalInformation { get; set; }

        /// <summary>
        /// The id of the OpenDrive Objects that the agent is currently on, can be a road or junction id
        /// </summary>
        public string OnId { get; set; } = "-1";

        /// <summary>
        /// List of all events that happen at this time for this agent.
        /// </summary>
        public List<SimulationEvent> Events { get; } = new List<SimulationEvent>();
        
        /// <summary>
        /// Information that is not further classified and can be displayed in the side panel.
        /// </summary>
        public List<object> UnclassifiedInformation { get; } = new List<object>();
    }

    public class SensorInformation {
        /// <summary>
        /// The relative position of the sensor towards the center of the vehicle
        /// </summary>
        public Vector2 RelativePosition { get; set; }

        /// <summary>
        /// Tells whether the opening angle of this Sensor Changed towards the previous SimulationStep
        /// </summary>
        public bool OpeningChangedTowardsPrevious { get; set; }
        
        /// <summary>
        /// Tells whether the opening angle of this Sensor Changed towards the next SimulationStep
        /// </summary>
        public bool OpeningChangedTowardsNext { get; set; }
        
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