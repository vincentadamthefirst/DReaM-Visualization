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
        
    }
}