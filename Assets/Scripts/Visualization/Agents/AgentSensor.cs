using UnityEngine;

namespace Visualization.Agents {
    
    public class AgentSensor : MonoBehaviour {

        private Transform _child;
        private MeshFilter _meshFilter;
        
        /// <summary>
        /// If the opening angle of this sensor changes
        /// </summary>
        public bool IsStatic { get; set; }

        private void Start() {
            _child = transform.GetChild(0);
            _meshFilter = _child.GetComponent<MeshFilter>();
        }

        public void UpdatePositionAndRotation(Vector3 position, float globalRotation) {
            transform.position = position;
            transform.rotation = Quaternion.Euler(0, -globalRotation * Mathf.Rad2Deg, 0);
        }

        public void UpdateOpeningAngle(float angleRadians, float distance) {
            if (IsStatic) return;
            
            var newMesh = new Mesh();
            var height = Mathf.Sin(angleRadians / 2f) * distance;
            newMesh.vertices = new[]
                {Vector3.zero, new Vector3(height, 0, distance), new Vector3(-height, 0, distance)};
            newMesh.triangles = new[] {0, 1, 2};
            
            newMesh.RecalculateNormals();
            _meshFilter.mesh = newMesh;
            
            _child.localPosition = new Vector3(0, 0, (Mathf.Tan(angleRadians / 2f) * distance) / 2f);
        }
    }
}