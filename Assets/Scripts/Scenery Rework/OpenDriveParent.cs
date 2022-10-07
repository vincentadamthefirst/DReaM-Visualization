using System.Collections.Concurrent;
using Scenery_Rework.OpenDrive_Elements;
using Scenery_Rework.OpenDrive_Elements.Road;

namespace Scenery_Rework {
    
    /// <summary>
    /// Class representing the parent Element of OpenDrive. This class holds the references to all OpenDrive Elements
    /// and is the main access point for later parts of the workflow. This class is designed to allow for a thread-safe
    /// import of OpenDrive elements as well as a thread-safe execution of preprocessing code that is unrelated to
    /// Unity.
    ///
    /// Legacy class: RoadNetworkHolder
    /// 
    /// </summary>
    public class OpenDriveParent {
        
        // TODO change from int to actual class
        
        /// <summary>
        /// Stores all the roads of the scene. The key are the OpenDrive IDs.
        /// </summary>
        public ConcurrentDictionary<string, Road> Roads { get; } = new ConcurrentDictionary<string, Road>();

        /// <summary>
        /// Stores all the junctions of the scene. The key are the OpenDrive IDs.
        /// </summary>
        private ConcurrentDictionary<string, Junction> Junctions { get; } = new ConcurrentDictionary<string, Junction>();
        
        
    }
}