using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using Visualization.Agents;
using Visualization.Labels.Detail;

namespace Visualization.Labels {
    public class LabelManager : MonoBehaviour {
        public static LabelManager Instance { get; private set; }

        private readonly Dictionary<string, Label> _activeLabels = new();
        
        /// <summary>
        /// This dictionary is used to temporarily store information about sensors.
        /// If a sensor is registered before the label exists its info will be put into this dictionary.
        /// </summary>
        private readonly Dictionary<string, List<Tuple<Agent, AgentSensor>>> _sensorStorage = new();

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(this);
            } else {
                Instance = this;
            }
        }

        public void CollectAgents() {
            var allAgents = FindObjectsOfType<Agent>();
            foreach (var agent in allAgents) {
                agent.TargetStatusChanged += AgentTargetStatusChanged;
            }
        }

        private void AgentTargetStatusChanged(object sender, bool newStatus) {
            var agent = (Agent)sender;
            if (_activeLabels.ContainsKey(agent.Id)) {
                var label = _activeLabels[agent.Id];
                _activeLabels.Remove(agent.Id);
                agent.StaticData.AgentCamera.gameObject.SetActive(false);
                agent.AgentUpdated -= label.TriggerUpdate;
                Destroy(label.gameObject);
            } else {
                var label = CreateLabelForAgent(agent);
                agent.AgentUpdated += label.TriggerUpdate;
            }
        }

        private Label CreateLabelForAgent(Agent agent) {
            var labelPrefab = Resources.Load<Label>("Prefabs/UI/Visualization/Labels/Label");
            var label = Instantiate(labelPrefab, transform);
            label.name = $"Label (Agent #{agent.Id})";
            label.Initialize(agent); // initializes the main view

            label.AddLabelTextEntry("Position:", new Reference<string>(() => $"({agent.DynamicData.Position2D.x:F2}, {agent.DynamicData.Position2D.y:F2})"));
            label.AddLabelTextEntry("Velocity:", new Reference<string>(() => $"{agent.DynamicData.ActiveSimulationStep.Velocity:F2} m/s"));
            label.AddLabelTextEntry("Acceleration:", new Reference<string>(() => $"{agent.DynamicData.ActiveSimulationStep.Acceleration:F2} m/s²"));

            if (VisualizationMaster.Instance.ActiveModules.DReaM) {
                label.AddLabelTextEntry("Crossing Phase:", new Reference<string>(() => $"{agent.DynamicData.ActiveSimulationStep.AdditionalInformation.CrossingPhase}"));
                label.AddLabelTextEntry("Scan AOI:", new Reference<string>(() => $"{agent.DynamicData.ActiveSimulationStep.AdditionalInformation.ScanAoI}"));
                label.AddLabelTextEntry("Gaze Type:", new Reference<string>(() => $"{agent.DynamicData.ActiveSimulationStep.AdditionalInformation.GlanceType}"));
            }
            
            if (agent.StaticData.AgentCamera != null) {
                var targetTexture = new RenderTexture(194, 194, 1);
                label.CognitiveMap.texture = targetTexture;
                agent.StaticData.AgentCamera.aspect = 1f;
                agent.StaticData.AgentCamera.orthographicSize = 100f;
                agent.StaticData.AgentCamera.targetTexture = targetTexture;
                agent.StaticData.AgentCamera.gameObject.SetActive(true);
            } 
            
            _activeLabels.Add(agent.Id, label);

            if (_sensorStorage.ContainsKey(agent.Id)) {
                _sensorStorage[agent.Id].ForEach(x => CreateSensorEntry(x.Item1, x.Item2));
                _sensorStorage.Remove(agent.Id);
            }
            
            return label;
        }

        private void CreateSensorEntry(Agent agent, AgentSensor sensor) {
            var label = _activeLabels[agent.Id]; 
            label.AddLabelSensorEntry(sensor, new Reference<string>(() => $"hdg: {sensor.CurrentStatus.Heading:F2}, opn: {sensor.CurrentStatus.OpeningAngle:F2}"));
        }

        public void AddSensorToLabel(Agent agent, AgentSensor sensor) {
            if (!_activeLabels.ContainsKey(agent.Id)) {
                Debug.Log($"Agent {agent.Id} does not have an active label. Cannot add sensor.");
                if (!_sensorStorage.ContainsKey(agent.Id)) 
                    _sensorStorage[agent.Id] = new List<Tuple<Agent, AgentSensor>>();
                
                _sensorStorage[agent.Id].Add(new Tuple<Agent, AgentSensor>(agent, sensor));
                return;
            }

            CreateSensorEntry(agent, sensor);
        }
    }
}