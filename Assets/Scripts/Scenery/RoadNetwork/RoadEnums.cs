namespace Scenery.RoadNetwork {

    /// <summary>
    /// All supported LaneTypes.
    /// </summary>
    public enum LaneType {
        None, Driving, Biking, Restricted, Sidewalk, Border, Shoulder
    }

    /// <summary>
    /// All RoadMarks, currently only Broken and Solid are supported.
    /// TODO implement support
    /// </summary>
    public enum RoadMarkType {
        None, Broken, Solid, SolidSolid, SolidBroken, BrokenSolid, BrokenBroken
    }

    /// <summary>
    /// All supported ElementTypes for links between roads or lanes.
    /// </summary>
    public enum ElementType {
        None, Junction, Road
    }

    /// <summary>
    /// The different lane directions.
    /// </summary>
    public enum LaneDirection {
        Left, Center, Right
    }

    /// <summary>
    /// The different contact points of roads or lanes.
    /// </summary>
    public enum ContactPoint {
        Start, End
    }
}