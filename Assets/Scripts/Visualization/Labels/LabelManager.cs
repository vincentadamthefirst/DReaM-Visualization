using System;
using System.Collections.Generic;
using UnityEngine;
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
                Debug.Log("Label exists, removing");
                var label = _activeLabels[agent.Id];
                _activeLabels.Remove(agent.Id);
                Destroy(label.gameObject);
            } else {
                var label = CreateLabelForAgent(agent);
                agent.AgentUpdated += delegate { label.TriggerUpdate(); };
                _activeLabels.Add(agent.Id, label);
            }
        }

        private Label CreateLabelForAgent(Agent agent) {
            var labelPrefab = Resources.Load<Label>("Prefabs/UI/Visualization/Labels/Label");
            var label = Instantiate(labelPrefab, transform);
            label.name = $"Label (Agent #{agent.Id})"; 

            label.Initialize(agent); // initializes the main view
            
            // initialize secondary view
            var labelTextEntryPrefab = Resources.Load<LabelTextEntry>("Prefabs/UI/Visualization/Labels/LabelTextEntry");
            var position = Instantiate(labelTextEntryPrefab, label.secondary);
            position.Initialize("Position:");
            position.Reference = new Reference<string>(() => $"({agent.DynamicData.Position2D.x:F2}, {agent.DynamicData.Position2D.y:F2})");
            label.AddLabelEntry(position);
            
            var velocity = Instantiate(labelTextEntryPrefab, label.secondary);
            velocity.Initialize("Velocity:");
            velocity.Reference = new Reference<string>(() => $"{agent.DynamicData.ActiveSimulationStep.Velocity} m/s");
            label.AddLabelEntry(velocity);
            
            var acceleration = Instantiate(labelTextEntryPrefab, label.secondary);
            acceleration.Initialize("Acceleration:");
            acceleration.Reference = new Reference<string>(() => $"{agent.DynamicData.ActiveSimulationStep.Acceleration} m/s²");
            label.AddLabelEntry(acceleration);

            if (VisualizationMaster.Instance.ActiveModules.DReaM) {
                var crossingPhase = Instantiate(labelTextEntryPrefab, label.secondary);
                crossingPhase.Initialize("Crossing Phase:");
                crossingPhase.Reference = new Reference<string>(() => $"{agent.DynamicData.ActiveSimulationStep.AdditionalInformation.CrossingPhase}");
                label.AddLabelEntry(crossingPhase);
                
                var scanAoi = Instantiate(labelTextEntryPrefab, label.secondary);
                scanAoi.Initialize("Scan AOI:");
                scanAoi.Reference = new Reference<string>(() => $"{agent.DynamicData.ActiveSimulationStep.AdditionalInformation.ScanAoI}");
                label.AddLabelEntry(scanAoi);
                
                var gazeType = Instantiate(labelTextEntryPrefab, label.secondary);
                gazeType.Initialize("Gaze Type:");
                gazeType.Reference = new Reference<string>(() => $"{agent.DynamicData.ActiveSimulationStep.AdditionalInformation.GlanceType}");
                label.AddLabelEntry(gazeType);
            }

            if (_sensorStorage.ContainsKey(agent.Id)) {
                _sensorStorage[agent.Id].ForEach(x => CreateSensorEntry(x.Item1, x.Item2));
                _sensorStorage.Remove(agent.Id);
            }

            return label;
        }

        private void CreateSensorEntry(Agent agent, AgentSensor sensor) {
            var label = _activeLabels[agent.Id];
            var sensorEntryPrefab = Resources.Load<LabelSensorEntry>("Prefabs/UI/Visualization/Labels/LabelSensorEntry");
            var sensorEntry = Instantiate(sensorEntryPrefab, label.tertiary);
            sensorEntry.Reference = new Reference<string>(() => $"{agent.DynamicData.ActiveSimulationStep.AdditionalInformation.CrossingPhase}");
            sensorEntry.AddSensor(sensor);
            label.AddLabelEntry(sensorEntry);
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