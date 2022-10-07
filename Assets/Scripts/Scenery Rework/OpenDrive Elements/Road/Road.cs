using System.Collections.Generic;
using Scenery_Rework.OpenDrive_Elements.Lane;
using Scenery_Rework.OpenDrive_Elements.Road.RoadGeometries;

namespace Scenery_Rework.OpenDrive_Elements.Road {
    
    /// <summary>
    /// Class representing an OpenDrive road.
    ///
    /// Reference:
    /// https://www.asam.net/index.php?eID=dumpFile&t=f&f=3495&token=56b15ffd9dfe23ad8f759523c806fc1f1a90a0e8#_roads
    /// Legacy class: Road
    /// 
    /// </summary>
    public class Road : IOpenDriveElement {
        
        /// <summary>
        /// The name of this road.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// The total length of this road in m.
        /// </summary>
        public float Length { get; set; }
        
        /// <summary>
        /// The unique ID of this road.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The unique ID of the junction this road is on. If this road is not on a junction this value is set to
        /// "NONE".
        /// </summary>
        public string JunctionId { get; set; } = "NONE";
        
        /// <summary>
        /// The Junction object this road is on. If this road is not on a junction this value is set to null.
        /// </summary>
        public Junction Junction { get; set; }

        /// <summary>
        /// The lane sections that make up the road.
        /// </summary>
        public List<LaneSection> LaneSections { get; } = new List<LaneSection>();
        
        /// <summary>
        /// The geometry describing the reference line of this road.
        /// </summary>
        public List<RoadGeometry> PlanView { get; } = new List<RoadGeometry>(); 
        
        /// <summary>
        /// The SuperElevations of this road (elevation along s-axis).
        ///
        /// Currently unsupported in rendering!
        /// </summary>
        // TODO Fully implement
        public List<SuperElevation> SuperElevation { get; } = new List<SuperElevation>();
        
        /// <summary>
        /// The ElevationProfiles of this road (elevation along t-axis).
        /// 
        /// Currently unsupported in rendering!
        /// </summary>
        // TODO Fully implement
        public List<ElevationProfile> Elevation { get; } = new List<ElevationProfile>();
        
    }
}