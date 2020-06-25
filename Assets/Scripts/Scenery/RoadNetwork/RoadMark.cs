using UnityEngine;

namespace Scenery.RoadNetwork {
    
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class RoadMark : SceneryElement {
        
        public RoadMarkType RoadMarkType { get; set; }

        public Lane ParentLane { get; set; }
        
        public Color RoadMarkColor { get; set; }
        
        public RoadDesign RoadDesign { get; set; }

        public float Width { get; set; }

        public void GenerateMesh() {
            if (RoadMarkType == RoadMarkType.None) return;

            var mesh = new Mesh();
            
            RoadHelper.GenerateMeshForRoadMark(ref mesh, this);

            GetComponent<MeshFilter>().mesh = mesh;

            GetComponent<MeshRenderer>().material = RoadDesign.none;
            
            transform.position += new Vector3(0, RoadDesign.offsetHeight, 0);
        }

        public void FillMaterials(Material material1, Material material2 = null) {
            // TODO fill
        }
    }
}