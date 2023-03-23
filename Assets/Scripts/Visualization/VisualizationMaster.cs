using System;
using System.Collections.Generic;
using TMPro;
using UI.Visualization;
using UnityEngine;
using Visualization.Agents;
using Visualization.Labels.BasicLabels;

namespace Visualization {
    public class ActiveModules {
        public bool DReaM { get; set; }
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

        public ActiveModules ActiveModules { get; } = new();

        // fallback vehicle model
        private readonly VehicleModelInformation _basicVehicle = new()
            { Width = 2f, Length = 5f, Height = 1.8f, Center = Vector3.zero, WheelDiameter = 0.65f };

        // fallback pedestrian model
        private readonly PedestrianModelInformation _basicPedestrian = new()
            { Width = 0.7f, Length = 0.7f, Height = 1.8f, Center = Vector3.zero };

        // all Agents of the current visualization run
        public List<Agent> Agents { get; } = new();

        public Dictionary<string, Agent> AgentIdMapping { get; } = new();

        public void FindAll() {
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
        /// <param name="id">id of the agent</param>
        /// <returns>The instantiated agent</returns>
        public Agent InstantiateVehicleAgent(string modelType, string id) {
            var agentModel = agentDesigns.GetAgentModel(AgentType.Vehicle, modelType);

            Agent vehiclePrefab = agentModel.modelName.Contains("fallback")
                ? Resources.Load<BoxAgent>("Prefabs/Agents/BoxAgent")
                : Resources.Load<VehicleAgent>("Prefabs/Agents/Agent");
            var vehicleAgent = Instantiate(vehiclePrefab, transform, true);

            // adding the RoadNetworkHolder
            vehicleAgent.Master = this;
            vehicleAgent.Id = id;

            // retrieving prefab for 3d model
            var modelPrefab =
                Resources.Load<GameObject>(
                    $"Prefabs/Agents/Models/{agentDesigns.GetAgentModel(AgentType.Vehicle, modelType).model}");
            var model = Instantiate(modelPrefab, vehicleAgent.transform, true);
            Agents.Add(vehicleAgent);
            AgentIdMapping.Add(vehicleAgent.Id, vehicleAgent);
            vehicleAgent.StaticData.Model = model;

            // adding basic ID label
            var idLabel = Resources.Load<TextLabel>("Prefabs/UI/Visualization/Labels/TextLabel");
            var idLabelObject = Instantiate(idLabel, model.transform);
            idLabelObject.name = "IdLabel";
            idLabelObject.GetComponent<TMP_Text>().SetText($"Agent {id}");
            idLabelObject.gameObject.SetActive(false);
            idLabelObject.MainCamera = Camera.main;
            _idLabelController.AddLabel(idLabelObject);

            if (DisableLabels)
                Destroy(vehicleAgent.StaticData.Model.transform.Find("Camera").gameObject);
            else {
                vehicleAgent.StaticData.AgentCamera =
                    vehicleAgent.StaticData.Model.transform.Find("Camera").GetComponent<Camera>();
                vehicleAgent.StaticData.AgentCamera.gameObject.SetActive(false);
            }

            // retrieving model information
            vehicleAgent.StaticData.ModelInformation = VehicleModelCatalog.ContainsKey(modelType)
                ? VehicleModelCatalog[modelType]
                : _basicVehicle;

            return vehicleAgent;
        }

        /// <summary>
        /// Initializes a new pedestrian agent and returns a reference to it.
        /// </summary>
        /// <returns>The instantiated agent</returns>
        public PedestrianAgent InstantiatePedestrian(string modelType, string id) {
            var pedestrianPrefab = Resources.Load<PedestrianAgent>("Prefabs/Agents/Pedestrian");
            var pedestrianAgent = Instantiate(pedestrianPrefab, transform, true);

            // adding the RoadNetworkHolder
            pedestrianAgent.Master = this;
            pedestrianAgent.Id = id;

            // retrieving prefab for 3d model
            var modelPrefab =
                Resources.Load<GameObject>(
                    $"Prefabs/Agents/Models/{agentDesigns.GetAgentModel(AgentType.Vehicle, modelType).model}");
            var model = Instantiate(modelPrefab, pedestrianAgent.transform, true);
            Agents.Add(pedestrianAgent);
            AgentIdMapping.Add(pedestrianAgent.Id, pedestrianAgent);
            pedestrianAgent.StaticData.Model = model;

            // adding basic ID label
            var idLabel = Resources.Load<TextLabel>("Prefabs/UI/Visualization/Labels/TextLabel");
            var idLabelObject = Instantiate(idLabel, model.transform);
            idLabelObject.name = "IdLabel";
            idLabelObject.GetComponent<TMP_Text>().SetText($"Agent {id}");
            idLabelObject.gameObject.SetActive(false);
            idLabelObject.MainCamera = Camera.main;
            _idLabelController.AddLabel(idLabelObject);

            if (DisableLabels)
                Destroy(pedestrianAgent.StaticData.Model.transform.Find("Camera").gameObject);
            else {
                pedestrianAgent.StaticData.AgentCamera =
                    pedestrianAgent.StaticData.Model.transform.Find("Camera").GetComponent<Camera>();
                pedestrianAgent.StaticData.AgentCamera.gameObject.SetActive(false);
            }

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