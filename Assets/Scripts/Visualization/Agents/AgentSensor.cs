using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Visualization.Agents {

    /**
     * Setup information for a sensor.
     */
    public struct SensorSetup {
        // name of this sensor
        public string sensorName;
        // color of this sensor
        public Color color;
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
        
        private bool _on = true;

        public SensorInformation CurrentStatus { get; private set; } = new();

        public SensorSetup SensorSetup { get; set; }

        private bool _newSensor = true;

        private void Awake() {
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
        /// <param name="agentPosition">the global position of the agent this sensor is attached to</param>
        /// <param name="localPosition">the local position of this sensor inside the agent</param>
        /// <param name="globalRotation">the global rotation of this sensors view frustum</param>
        private void UpdatePositionAndRotation(Vector3 agentPosition, Vector3 localPosition, float globalRotation) {
            _child.position = agentPosition + localPosition;
            _child.rotation = Quaternion.Euler(0, (-globalRotation) * Mathf.Rad2Deg, 0);
        }

        /// <summary>
        /// Generates a new mesh based on a new opening angle and viewing distance.
        /// </summary>
        /// <param name="angleRadians">The new opening angle in radians</param>
        /// <param name="distance">The new sensor viewing distance in units (meters)</param>
        private void UpdateOpeningAngle(float angleRadians, float distance) {
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
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public void AgentUpdated(object sender, EventArgs _) {
            var agent = sender as Agent;
            var sensorInfo = agent.GetSensorData(SensorSetup.sensorName);
            if (sensorInfo.ValuesChangedTowardsNeighbors || _newSensor) {
                UpdateOpeningAngle(sensorInfo.OpeningAngle * Mathf.Deg2Rad, sensorInfo.Distance);
                _newSensor = false;
            }
            var localPos = new Vector3(sensorInfo.LocalPosition.x, 0, sensorInfo.LocalPosition.y);
            UpdatePositionAndRotation(agent.DynamicData.Position3D + new Vector3(0, 1, 0), localPos, sensorInfo.Heading);
            CurrentStatus = sensorInfo;
        }

        public void AgentActiveStatusChanged(object _, bool newStatus) {
            _meshRenderer.enabled = newStatus;
        }

        /// <summary>
        /// De-/activates the display of this sensor. Overwritten by Active.
        /// </summary>
        public void SetOn(bool on) {
            _on = on;
            UpdateVisibility();
        }

        private void UpdateVisibility() {
            _child.GetChild(0).gameObject.SetActive(_on);
        }
    }
}