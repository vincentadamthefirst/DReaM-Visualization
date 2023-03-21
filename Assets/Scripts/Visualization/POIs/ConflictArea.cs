using System.Linq;
using Scenery;
using Scenery.RoadNetwork;
using UnityEngine;
using UnityEngine.Serialization;
using Visualization.Labels.BasicLabels;

namespace Visualization.POIs {
    public class ConflictArea : HoverableElement {

        public GameObject MeshObject { get; private set; }
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private MeshCollider _meshCollider;
        
        public TextLabel InfoLabel { get; set; }
        
        public override ElementOrigin ElementOrigin => ElementOrigin.OpenPass;
        
        private void Awake() {
            MeshObject = transform.Find("Mesh").gameObject;
            _meshFilter = MeshObject.GetComponent<MeshFilter>();
            _meshRenderer = MeshObject.GetComponent<MeshRenderer>();
            _meshCollider = MeshObject.GetComponent<MeshCollider>();
            FindOutlines();
        }

        public void SetMeshData(Mesh mesh, Material[] materials) {
            _meshFilter.mesh = mesh;
            _meshCollider.sharedMesh = mesh;
            _meshRenderer.materials = materials;
            FindOutlines();

            foreach (var outline in GetComponentsInChildren<Outline>().ToList()) {
                outline.LoadSmoothNormals();
                outline.enabled = false;
            }
        }

        public override void MouseEnter() {
            base.MouseEnter();
            InfoLabel.gameObject.SetActive(!SimpleCameraController.Instance.RightMouseClicked && !SimpleCameraController.Instance.SettingsOpen);
        }

        public override void MouseExit() {
            base.MouseExit();
            InfoLabel.gameObject.SetActive(false);
        }
    }
}