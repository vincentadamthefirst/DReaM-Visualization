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
        
        // Properties for materials
        private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
        private static readonly int Color = Shader.PropertyToID("_Color");

        public void GenerateMesh() {
            if (RoadMarkType == RoadMarkType.None) return;

            var mesh = new Mesh();
            
            RoadHelper.GenerateMeshForRoadMark(ref mesh, this);

            GetComponent<MeshFilter>().mesh = mesh;
            AddMaterial();

            transform.position += new Vector3(0, RoadDesign.offsetHeight, 0);
        }

        private void AddMaterial() {
            var meshRenderer = GetComponent<MeshRenderer>();
            var material = new Material(RoadMarkType == RoadMarkType.Broken ? RoadDesign.broken : RoadDesign.solid);
            
            var p = material.GetTextureScale(BaseMap);
            var v = new Vector2(p.x, ParentLane.Parent.Length * p.y);
            material.SetTextureScale(BaseMap, v);
            material.SetColor(Color, RoadMarkColor);
            meshRenderer.material = material;
        }
    }
}