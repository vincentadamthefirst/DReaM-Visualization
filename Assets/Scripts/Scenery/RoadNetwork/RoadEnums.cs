namespace Scenery.RoadNetwork {

    /**
     * All supported lane types
     */
    public enum LaneType {
        None, Driving, Biking, Restricted, Sidewalk, Border, Shoulder
    }

    public enum RoadMarkType {
        None, Broken, Solid, SolidSolid, SolidBroken, BrokenSolid, BrokenBroken
    }

    /**
     * All supported element types (for links)
     */
    public enum ElementType {
        None, Junction, Road
    }

    /// <summary>
    /// All supported geometry types
    /// TODO maybe remove
    /// </summary>
    public enum GeometryType {
        Line, Arc, Spiral, Poly3, ParamPoly3
    }

    /// <summary>
    /// The different lane directions.
    /// </summary>
    public enum LaneDirection {
        Left, Center, Right
    }

    public enum ContactPoint {
        Start, End
    }

    public class RoadEnumStrings {
        
        public static string[] laneDirectionToString = {"Left", "Center", "Right"};

        public static string[] laneTypeToString =
            {"None", "Driving", "Biking", "Restricted", "Sidewalk", "Border", "Shoulder"};

    }
    
}