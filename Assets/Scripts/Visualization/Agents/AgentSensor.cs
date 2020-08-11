using UnityEngine;

namespace Visualization.Agents {
    
    /// <summary>
    /// Class representing a sensor in the scene. Has a child object containing the actual mesh representing the sensor
    /// view.
    /// </summary>
    public class AgentSensor : MonoBehaviour {
        // information on this sensors mesh 
        private Transform _child;
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;

        /// <summary>
        /// Method to retrieve all necessary objects for this sensor (its child object and mesh rendering components)
        /// </summary>
        public void FindAll() {
            _child = transform.GetChild(0);
            _meshFilter = _child.GetChild(0).GetComponent<MeshFilter>();
            _meshRenderer = _child.GetChild(0).GetComponent<MeshRenderer>();
        }

        /// <summary>
        /// Sets the material for the sensor mesh of this sensor.
        /// </summary>
        /// <param name="meshMaterial">the new material for the sensor mesh</param>
        public void SetMeshMaterial(Material meshMaterial) {
            _meshRenderer.material = meshMaterial;
        }

        /// <summary>
        /// Updates the position and rotation of this sensor.
        /// </summary>
        /// <param name="position">the global position of this sensor</param>
        /// <param name="globalRotation">the global rotation of this sensors view frustum</param>
        public void UpdatePositionAndRotation(Vector3 position, float globalRotation) {
            _child.position = position;
            _child.rotation = Quaternion.Euler(0, (-globalRotation +  + Mathf.PI / 2f) * Mathf.Rad2Deg, 0);
        }

        /// <summary>
        /// Generates a new mesh based on a new opening angle and viewing distance.
        /// </summary>
        /// <param name="angleRadians">The new opening angle in radians</param>
        /// <param name="distance">The new sensor viewing distance in units (meters)</param>
        public void UpdateOpeningAngle(float angleRadians, float distance) {
            var newMesh = new Mesh();
            var height = Mathf.Sin(angleRadians / 2f) * distance;
            newMesh.vertices = new[]
                {Vector3.zero, new Vector3(height, 0, distance), new Vector3(-height, 0, distance)};
            newMesh.triangles = new[] {0, 2, 1};
            
            newMesh.RecalculateNormals();
            _meshFilter.mesh = newMesh;
        }

        /// <summary>
        /// Sets the child object active or not. Used to hide the mesh in the scene.
        /// </summary>
        /// <param name="active">The new active status of the child object.</param>
        public void SetActive(bool active) {
            _child.GetChild(0).gameObject.SetActive(active);
        }
    }
}