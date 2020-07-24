using UnityEngine;

namespace Scenery.RoadNetwork.RoadObjects {
    
    /// <summary>
    /// Abstract class representing an object along a road in OpenDrive.
    /// </summary>
    public abstract class RoadObject : VisualizationElement {
        
        /// <summary>
        /// The type of object
        /// </summary>
        public RoadObjectType RoadObjectType { get; set; }
        
        /// <summary>
        /// The objects subType
        /// </summary>
        public string SubType { get; set; }
        
        /// <summary>
        /// The orientation of the object (left or right side of the road)
        /// </summary>
        public RoadObjectOrientation Orientation { get; set; }
        
        /// <summary>
        /// The s coordinate of the object
        /// </summary>
        public float S { get; set; }
        
        /// <summary>
        /// The t coordinate of the object
        /// </summary>
        public float T { get; set; }
        
        /// <summary>
        /// The z Offset of the Object (from z = 0)
        /// </summary>
        public float ZOffset { get; set; }
        
        /// <summary>
        /// The heading of the object (based on the heading of the road at the given s value of the object)
        /// </summary>
        public float Heading { get; set; }

        /// <summary>
        /// The height of the object, might no be accurately represented for prefab objects
        /// </summary>
        public float Height { get; set; }
        
        /// <summary>
        /// The Road this object belongs to
        /// </summary>
        public Road Parent { get; set; }
        
        /// <summary>
        /// RepeatParameters for this object, might be null
        /// </summary>
        public RepeatParameters RepeatParameters { get; set; }
        
        /// <summary>
        /// The overall RoadDesign for the visualization of the road network
        /// </summary>
        public RoadDesign RoadDesign { get; set; }

        /// <summary>
        /// Repeats the object if necessary and generates its Mesh.
        /// </summary>
        public abstract void Show();

        /// <summary>
        /// Method to delete this object but only if markedForDelete is internally set to true.
        /// </summary>
        /// <returns>If the object was deleted.</returns>
        public abstract bool MaybeDelete();

        // internal bool, an object with a repeat will be deleted after all its "children" have been generated
        protected bool markedForDelete;

        protected override Vector3[] GetReferencePointsRenderer() {
            return new Vector3[0]; // assume objects along the road are never targets
        }

        protected override Vector3[] GetReferencePointsCustom() {
            return new Vector3[0]; // assume objects along the road are never targets
        }
    }
}