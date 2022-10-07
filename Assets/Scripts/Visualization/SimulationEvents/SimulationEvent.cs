namespace Visualization.SimulationEvents {
    
    public enum SimulationEventType {
        AEBActive,
        AEBInactive,
        Unsupported
    }

    public class SimulationEvent {
        /// <summary>
        /// the name of the event
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// The timestep this event occured
        /// </summary>
        public int TimeStep { get; set; }
        
        public int AgentId { get; set; }
        public SimulationEventType EventType { get; set; }
    }
}