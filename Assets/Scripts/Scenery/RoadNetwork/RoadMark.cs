using UnityEngine;

namespace Scenery.RoadNetwork {
    
    /// <summary>
    /// Class representing an OpenDrive RoadMark
    /// </summary>
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class RoadMark : VisualizationElement {
        
        /// <summary>
        /// The type of this RoadMark
        /// </summary>
        public RoadMarkType RoadMarkType { get; set; }

        /// <summary>
        /// The Lane this RoadMark belongs to
        /// </summary>
        public Lane ParentLane { get; set; }
        
        /// <summary>
        /// The Color of this RoadMark
        /// </summary>
        public Color RoadMarkColor { get; set; }
        
        /// <summary>
        /// The overall RoadDesign used for visualizing the road network
        /// </summary>
        public RoadDesign RoadDesign { get; set; }

        /// <summary>
        /// The width of this RoadMark
        /// </summary>
        public float Width { get; set; }

        /// <summary>
        /// Starts the generation of the Mesh for this RoadMark.
        /// </summary>
        public void GenerateMesh() {
            if (RoadMarkType == RoadMarkType.None) return;

            var mesh = new Mesh();
            
            RoadHelper.GenerateMeshForRoadMark(ref mesh, this);

            GetComponent<MeshFilter>().mesh = mesh;
            AddMaterial();

            transform.position += new Vector3(0, RoadDesign.offsetHeight, 0);
        }

        /// <summary>
        /// Adds a Material to the MeshFilter for this RoadMark.
        /// </summary>
        private void AddMaterial() {
            var meshRenderer = GetComponent<MeshRenderer>();
            var material = new Material(RoadMarkType == RoadMarkType.Broken ? RoadDesign.broken : RoadDesign.solid);
            
            var p = material.GetTextureScale(BaseMap);
            var v = new Vector2(p.x, ParentLane.Parent.Length * p.y);
            material.SetTextureScale(BaseMap, v);
            material.SetColor(Color, RoadMarkColor);
            meshRenderer.material = material;
        }

        /// <summary>
        /// Scenery will not be handled on occlusion, ignore
        /// </summary>
        public override void HandleHit() {
            // Ignore
        }

        /// <summary>
        /// Scenery will not be handled on occlusion, ignore
        /// </summary>
        public override void HandleNonHit() {
            // Ignore
        }

        protected override Vector3[] GetReferencePointsRenderer() {
            return new Vector3[0]; // assume scenery is never target, Ignore
        }

        protected override Vector3[] GetReferencePointsCustom() {
            return new Vector3[0]; // assume scenery is never target, Ignore
        }
    }
}