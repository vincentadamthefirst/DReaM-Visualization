using System.Collections.Generic;
using UnityEngine;

namespace Visualization.Agents {

    /**
     * Setup information for a sensor.
     */
    public struct SensorSetup {
        // 2D offset relative to the center of the agent
        public Vector2 inAgentOffset;
        // color of this sensor
        public Color color;
    }

    /**
     * Per time step data
     */
    public struct SensorData {
        // opening angle of this sensor
        public float openingAngle;
        // direction this sensor is looking at
        public float direction;
        // distance that this sensor covers
        public float distance;
    }
    
    
    /// <summary>
    /// Class representing a sensor in the scene. Has a child object containing the actual mesh representing the sensor
    /// view.
    /// </summary>
    public class AgentSensor : MonoBehaviour {
        // information on this sensors mesh 
        private Transform _child;
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;

        private bool _active = false;
        private bool _on = true;

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
            _child.rotation = Quaternion.Euler(0, (-globalRotation) * Mathf.Rad2Deg, 0);
        }

        /// <summary>
        /// Generates a new mesh based on a new opening angle and viewing distance.
        /// </summary>
        /// <param name="angleRadians">The new opening angle in radians</param>
        /// <param name="distance">The new sensor viewing distance in units (meters)</param>
        public void UpdateOpeningAngle(float angleRadians, float distance) {
            var newMesh = new Mesh();

            var currentAngle = -angleRadians / 2f;
            
            var verts = new List<Vector3> { Vector3.zero };
            var tris = new List<int>();

            for (var i = 0; i < 11; i++) {
                var x = distance * Mathf.Cos(currentAngle);
                var y = distance * Mathf.Sin(currentAngle);
                verts.Add(new Vector3(x, 0, y));
                if (i != 0)
                    tris.AddRange(new [] {0, verts.Count - 1, verts.Count - 2});

                currentAngle += angleRadians / 10;
            }

            newMesh.vertices = verts.ToArray();
            newMesh.triangles = tris.ToArray();

            newMesh.RecalculateNormals();
            _meshFilter.mesh = newMesh;


            // var newMesh = new Mesh();
            // var height = Mathf.Sin(angleRadians / 2f) * distance;
            // newMesh.vertices = new[]
            //     {Vector3.zero, new Vector3(height, 0, distance), new Vector3(-height, 0, distance)};
            // newMesh.triangles = new[] {0, 2, 1};
            //
            // newMesh.RecalculateNormals();
            // _meshFilter.mesh = newMesh;
        }

        /// <summary>
        /// De-/activates the display of this sensor. Overwritten by Active.
        /// </summary>
        public void SetOn(bool on) {
            _on = on;
            UpdateVisibility();
        }

        public void SetActive(bool active) {
            _active = active;
            UpdateVisibility();
        }

        private void UpdateVisibility() {
            _child.GetChild(0).gameObject.SetActive(_active && _on);
        }
    }
}