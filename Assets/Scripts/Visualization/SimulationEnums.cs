namespace Visualization {

    /// <summary>
    /// All supported agent types
    /// </summary>
    public enum AgentType {
        Vehicle, Pedestrian
    }

    /// <summary>
    /// TODO implement all
    /// </summary>
    public enum CrossingPhase {
        None
    }

    /// <summary>
    /// TODO implement all
    /// </summary>
    public enum GlanceType {
        None, ScanGlance, ObserveGlance
    }

    public enum ScanAoI {
        None, Right, Left
    }

    /// <summary>
    /// The different Indicator States
    /// </summary>
    public enum IndicatorState {
        None, Warn, Left, Right
    }
    
}