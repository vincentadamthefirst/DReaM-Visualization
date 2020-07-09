using UnityEngine;

namespace Visualization.SimulationEvents {
    public class SimulationEvent {
        /// <summary>
        /// the name of the event
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// The timestep this event occured
        /// </summary>
        public int TimeStep { get; set; }
    }
}