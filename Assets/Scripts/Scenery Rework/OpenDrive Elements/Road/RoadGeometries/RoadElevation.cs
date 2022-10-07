namespace Scenery_Rework.OpenDrive_Elements.Road.RoadGeometries {
    
    
    /// <summary>
    /// Struct representing the elevation profile of the road along the s-axis.
    ///
    /// Reference:
    /// https://www.asam.net/index.php?eID=dumpFile&t=f&f=3495&token=56b15ffd9dfe23ad8f759523c806fc1f1a90a0e8#_methods_of_elevation
    /// Legacy class: none
    /// 
    /// </summary>
    public struct ElevationProfile {
        
        /// <summary>
        /// S-coordinate of start position.
        /// </summary>
        public float S { get; set; }
        
        /// <summary>
        /// Polynom parameters.
        /// </summary>
        public float A { get; set; }
        public float B { get; set; }
        public float C { get; set; }
        public float D { get; set; }
    }

    /// <summary>
    /// Struct representing the elevation profile of the road along the t-axis.
    ///
    /// Reference:
    /// https://www.asam.net/index.php?eID=dumpFile&t=f&f=3495&token=56b15ffd9dfe23ad8f759523c806fc1f1a90a0e8#_methods_of_elevation
    /// Legacy class: none
    /// 
    /// </summary>
    public struct SuperElevation {
        
        /// <summary>
        /// S-coordinate of start position.
        /// </summary>
        public float S { get; set; }
        
        /// <summary>
        /// Polynom parameters.
        /// </summary>
        public float A { get; set; }
        public float B { get; set; }
        public float C { get; set; }
        public float D { get; set; }
    }
}