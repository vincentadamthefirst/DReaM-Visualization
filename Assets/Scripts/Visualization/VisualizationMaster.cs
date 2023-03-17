using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json.Serialization;
using TMPro;
using UI.Settings;
using UI.Visualization;
using UnityEngine;
using Visualization.Agents;
using Visualization.Labels.BasicLabels;
using Visualization.OcclusionManagement;

namespace Visualization {

    public class ActiveModules {
        public bool DReaM { get; set; } = false;
    }
    
    public class VisualizationMaster : MonoBehaviour {

        public static VisualizationMaster Instance { get; private set; }

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(this);
            } else {
                Instance = this;
            }
        }

        private PlaybackControl _playbackControl;
        private IdLabelController _idLabelController;

        public ApplicationSettings settings { get; set; }

        /// <summary>
        /// The catalog of possible vehicle models, populated by VehicleModelsXmlHandler
        /// </summary>
        public Dictionary<string, VehicleModelInformation> VehicleModelCatalog { get; } = new();

        /// <summary>
        /// The catalog of possible pedestrian models, populated by PedestrianModelsXmlHandler
        /// </summary>
        public Dictionary<string, PedestrianModelInformation> PedestrianModelCatalog { get; } = new();

        /// <summary>
        /// The models that this simulation run can use
        /// </summary>
        public AgentDesigns agentDesigns;

        public int MaxSampleTime { get; set; }
        public int MinSampleTime { get; set; }

        public bool DisableLabels { get; set; }

        public int SampleStep { get; set; }

        public int CurrentTime { get; set; }

        public bool PlayBackwards { get; set; }

        public bool Pause { get; set; } = true;

        public event EventHandler<int> TimeChanged;

        public ActiveModules ActiveModules { get; } = new ActiveModules();

        // fallback vehicle model
        private readonly VehicleModelInformation _basicVehicle = new()
            { Width = 2f, Length = 5f, Height = 1.8f, Center = Vector3.zero, WheelDiameter = 0.65f };

        // fallback pedestrian model
        private readonly PedestrianModelInformation _basicPedestrian = new()
            { Width = 0.7f, Length = 0.7f, Height = 1.8f, Center = Vector3.zero };

        // all Agents of the current visualization run
        public List<Agent> Agents { get; } = new List<Agent>();

        public void FindAll() {
            FindObjectOfType<LabelOcclusionManager>();
            _playbackControl = FindObjectOfType<PlaybackControl>();
            _idLabelController = FindObjectOfType<IdLabelController>();
        }

        /// <summary>
        /// Prepares all the agents for the simulation run, also assigns colors
        /// </summary>
        public void PrepareAgents() {
            // setting a color for each agent
            for (var i = 0; i < Agents.Count; i++) {
                var c = Color.HSVToRGB(i / (float)Agents.Count, .9f, .7f, false);
                var agentMaterial = new Material(agentDesigns.vehicleChassisBase) { color = c };

                Agents[i].StaticData.ColorMaterial = agentMaterial;
            }

            Agents.ForEach(a => a.Prepare());

            _playbackControl.SetTotalTime(MinSampleTime, MaxSampleTime);
        }

        /// <summary>
        /// Initializes a new vehicle agent and returns a reference to it.
        /// </summary>
        /// <param name="modelType">The model type of the agent</param>
        /// <returns>The instantiated agent</returns>
        public Agent InstantiateVehicleAgent(string modelType, string id) {
            var agentModel = agentDesigns.GetAgentModel(AgentType.Vehicle, modelType);
            Agent instantiated;

            if (agentModel.modelName.Contains("fallback"))
                instantiated = Instantiate(agentDesigns.boxPrefab, transform, true);
            else
                instantiated = Instantiate(agentDesigns.vehiclePrefab, transform, true);

            // adding the RoadNetworkHolder
            instantiated.settings = settings;
            instantiated.Master = this;

            // retrieving prefab for 3d model
            var model = Instantiate(agentDesigns.GetAgentModel(AgentType.Vehicle, modelType).model,
                instantiated.transform, true);
            Agents.Add(instantiated);
            instantiated.StaticData.Model = model;

            // adding basic ID label
            var idLabel = Resources.Load<TextLabel>("Prefabs/UI/Visualization/Labels/TextLabel");
            var idLabelObject = Instantiate(idLabel, model.transform);
            idLabelObject.name = "IdLabel";
            idLabelObject.GetComponent<TMP_Text>().SetText($"Agent {id}");
            idLabelObject.gameObject.SetActive(false);
            idLabelObject.MainCamera = Camera.main;
            _idLabelController.AddLabel(idLabelObject);

            // adding label
            // FIXME
            // if (!DisableLabels) {
            //     var label = Instantiate(agentDesigns.vehicleScreenLabel, _labelOcclusionManager.transform);
            //     label.Agent = instantiated;
            //     label.LabelOcclusionManager = _labelOcclusionManager;
            //     label.AgentCamera = instantiated.Model.transform.Find("Camera").GetComponent<Camera>();
            //     _labelOcclusionManager.AddLabel(label);
            //     instantiated.OwnLabel = label;
            // } else {
            Destroy(instantiated.StaticData.Model.transform.Find("Camera").gameObject);
            // }

            // retrieving model information
            instantiated.StaticData.ModelInformation = VehicleModelCatalog.ContainsKey(modelType)
                ? VehicleModelCatalog[modelType]
                : _basicVehicle;

            return instantiated;
        }

        /// <summary>
        /// Initializes a new pedestrian agent and returns a reference to it.
        /// </summary>
        /// <returns>The instantiated agent</returns>
        public PedestrianAgent InstantiatePedestrian(string modelType, string id) {
            var pedestrianAgent = Instantiate(agentDesigns.pedestrianPrefab, transform, true);

            // adding the RoadNetworkHolder
            pedestrianAgent.settings = settings;
            pedestrianAgent.Master = this;

            // retrieving prefab for 3d model
            var model = Instantiate(agentDesigns.GetAgentModel(AgentType.Pedestrian, modelType).model,
                pedestrianAgent.transform, true);
            Agents.Add(pedestrianAgent);
            pedestrianAgent.StaticData.Model = model;

            // adding basic ID label
            var idLabel = Resources.Load<TextLabel>("Prefabs/UI/Visualization/Labels/TextLabel");
            var idLabelObject = Instantiate(idLabel, model.transform);
            idLabelObject.name = "IdLabel";
            idLabelObject.GetComponent<TMP_Text>().SetText($"Agent {id}");
            idLabelObject.gameObject.SetActive(false);
            idLabelObject.MainCamera = Camera.main;
            _idLabelController.AddLabel(idLabelObject);

            // adding label TODO use other Label then vehicle!
            // FIXME re-enable
            // if (!DisableLabels) {
            //     var label = Instantiate(agentDesigns.pedestrianScreenLabel, _labelOcclusionManager.transform);
            //     label.Agent = pedestrianAgent;
            //     label.LabelOcclusionManager = _labelOcclusionManager;
            //     label.AgentCamera = pedestrianAgent.Model.transform.Find("Camera").GetComponent<Camera>();
            //     _labelOcclusionManager.AddLabel(label);
            //     pedestrianAgent.OwnLabel = label;
            // } else {
            Destroy(pedestrianAgent.StaticData.Model.transform.Find("Camera").gameObject);
            // }

            // retrieving model information
            pedestrianAgent.StaticData.ModelInformation = PedestrianModelCatalog.ContainsKey(modelType)
                ? PedestrianModelCatalog[modelType]
                : _basicPedestrian;

            return pedestrianAgent;
        }

        public void SmallUpdate() {
            if (CurrentTime < 0) CurrentTime = 0;
            else if (CurrentTime > MaxSampleTime) CurrentTime = MaxSampleTime;

            foreach (var agent in Agents) {
                agent.UpdateForTimeStep(CurrentTime, PlayBackwards);
            }

            _playbackControl.UpdateCurrentTime(CurrentTime);
        }

        /// <summary>
        /// Main function for the visualization, triggers the position / rotation change in all agents
        /// </summary>
        private void Update() {
            if (Pause) return;

            CurrentTime = PlayBackwards
                ? CurrentTime - Mathf.RoundToInt(Time.deltaTime * 1000f)
                : CurrentTime + Mathf.RoundToInt(Time.deltaTime * 1000f);

            if (CurrentTime < 0) CurrentTime = 0;
            else if (CurrentTime > MaxSampleTime) CurrentTime = MaxSampleTime;
            TimeChanged?.Invoke(this, CurrentTime);

            foreach (var agent in Agents) {
                agent.UpdateForTimeStep(CurrentTime, PlayBackwards);
            }

            _playbackControl.UpdateCurrentTime(CurrentTime);
        }
    }
}