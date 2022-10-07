namespace Scenery_Rework.OpenDrive_Elements {
    
    /// <summary>
    /// Possible contact points for road links. The value None = 0 is not present in the OpenDrive specification and
    /// was added to represent a missing or malformed value.
    ///
    /// Reference:
    /// https://www.asam.net/index.php?eID=dumpFile&t=f&f=3495&token=56b15ffd9dfe23ad8f759523c806fc1f1a90a0e8#_road_linkage
    ///
    /// </summary>
    public enum ContactPoint {
        None = 0, Start, End
    } 

    /// <summary>
    /// Possible element type for road links. The value None = 0 is not present in the OpenDrive specification and
    /// was added to represent a missing or malformed value.
    /// 
    /// Reference:
    /// https://www.asam.net/index.php?eID=dumpFile&t=f&f=3495&token=56b15ffd9dfe23ad8f759523c806fc1f1a90a0e8#_road_linkage
    /// 
    /// </summary>
    public enum ElementType { 
        None = 0, Road, Junction
    }
}