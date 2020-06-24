namespace Scenery.RoadNetwork {

    /**
     * All supported lane types
     */
    public enum LaneType {
        None, Sidewalk, Driving, Biking, Restricted
    }

    public enum RoadMarkType {
        None, Broken, Solid, SolidSolid, SolidBroken, BrokenSolid, BrokenBroken
    }

    /**
     * All supported element types
     */
    public enum ElementType {
        Junction, Road
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

    public class RoadEnumStrings {
        
        public static string[] laneDirectionToString = {"Left", "Center", "Right"};
        public static string[] laneTypeToString = {"None", "Sidewalk", "Driving"};

    }
    
}