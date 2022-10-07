namespace Scenery_Rework.OpenDrive_Elements.Road {
    /// <summary>
    /// Struct representing a link between two roads.
    ///
    /// Reference:
    /// https://www.asam.net/index.php?eID=dumpFile&t=f&f=3495&token=56b15ffd9dfe23ad8f759523c806fc1f1a90a0e8#_road_linkage
    /// Legacy class: none
    /// 
    /// </summary>
    public struct RoadLink {
        /// <summary>
        /// The successor of the road. If there is no successor this value will be null.
        /// </summary>
        public RoadLinkElement Successor { get; set; }
        
        /// <summary>
        /// The predecessor of the road. If there is no predecessor this value will be null.
        /// </summary>
        public RoadLinkElement Predecessor { get; set; }
    }

    /// <summary>
    /// Struct representing the actual link inside of a RoadLink.
    /// 
    /// Reference:
    /// https://www.asam.net/index.php?eID=dumpFile&t=f&f=3495&token=56b15ffd9dfe23ad8f759523c806fc1f1a90a0e8#_road_linkage
    /// Legacy class: none
    /// 
    /// </summary>
    public struct RoadLinkElement {
        /// <summary>
        /// The unique ID of the element this link describes.
        /// </summary>
        public string ElementId { get; set; }
        
        /// <summary>
        /// The contact point of this link.
        /// </summary>
        public ContactPoint ContactPoint { get; set; }

        /// <summary>
        /// The element type of the linked element.
        /// </summary>
        public ElementType ElementType { get; set; }
    }
}