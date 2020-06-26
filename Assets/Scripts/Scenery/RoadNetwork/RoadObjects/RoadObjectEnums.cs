namespace Scenery.RoadNetwork.RoadObjects {
    
    /// <summary>
    /// Supported RoadObjectTypes
    /// </summary>
    public enum RoadObjectType {
        None, Tree, Building, StreetLamp, Pole, CrossWalk, ParkingSpace
    }

    /// <summary>
    /// Orientation of a RoadObject along the track of a road
    /// </summary>
    public enum RoadObjectOrientation {
        None, Positive, Negative
    }
}