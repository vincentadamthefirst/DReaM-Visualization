using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UI.Main_Menu.Utils;
using UnityEngine;
using Visualization.Agents;
using Visualization.Labels;

namespace Visualization {
    public class SensorManager : MonoBehaviour {

        public Material sensorMaterial;
        
        public static SensorManager Instance { get; private set; }

        private Dictionary<string, List<AgentSensor>> _activeSensors = new();
        
        public void CollectAgents() {
            var allAgents = FindObjectsOfType<Agent>();
            foreach (var agent in allAgents) {
                agent.TargetStatusChanged += AgentTargetStatusChanged;
            }
        }

        private void Awake() {
            if (Instance != null && Instance != this)
                Destroy(this);
            else {
                Instance = this;
            }
        }

        private void AgentTargetStatusChanged(object sender, bool status) {
            var agent = sender as Agent;

            if (agent == null)
                throw new ArgumentNullException(nameof(sender), "Object must be an agent.");

            if (_activeSensors.ContainsKey(agent.Id)) {
                var sensors = _activeSensors[agent.Id];
                _activeSensors.Remove(agent.Id);
                foreach (var sensor in sensors) {
                    Destroy(sensor.gameObject);
                }
            } else {
                foreach (var (sensorName, sensorSetup) in agent.StaticData.UniqueSensors) {
                    Debug.Log("Creating sensor " + sensorName);
                    
                    var sensorPrefab = Resources.Load<AgentSensor>("Prefabs/Objects/AgentSensor");
                    var sensor = Instantiate(sensorPrefab, agent.transform.parent);
                    sensor.SensorSetup = sensorSetup;
                    
                    var sensorMat = new Material(sensorMaterial) {
                        color = sensorSetup.color.WithAlpha(sensorMaterial.color.a)
                    };
                    sensor.SetMeshMaterial(sensorMat);

                    agent.AgentUpdated += sensor.AgentUpdated;
                    
                    LabelManager.Instance.AddSensorToLabel(agent, sensor);
                }
            }
        }
    }
}