using System;
using System.Collections.Generic;
using Importer.XMLHandlers;
using Scenery.RoadNetwork;
using UI;
using UnityEngine;
using Visualization.Agents;
using Visualization.Labels;
using Visualization.RoadOcclusion;

namespace Visualization {
    public class VisualizationMaster : MonoBehaviour {

        public PlaybackControl playbackControl;

        public LabelOcclusionManager labelOcclusionManager;

        private RoadOcclusionManager _roadOcclusionManager;

        /// <summary>
        /// The catalog of possible vehicle models, populated by VehicleModelsXmlHandler
        /// </summary>
        public Dictionary<string, VehicleModelInformation> VehicleModelCatalog { get; } =
            new Dictionary<string, VehicleModelInformation>();
        
        /// <summary>
        /// The catalog of possible pedestrian models, populated by PedestrianModelsXmlHandler
        /// </summary>
        public Dictionary<string, PedestrianModelInformation> PedestrianModelCatalog { get; } =
            new Dictionary<string, PedestrianModelInformation>();

        /// <summary>
        /// The models that this simulation run can use
        /// </summary>
        public AgentDesigns agentDesigns;
        
        public int MaxSampleTime { get; set; }
        
        public int CurrentTime { get; set; }
        
        public bool PlayBackwards { get; set; }

        public bool Pause { get; set; } = true;
        
        public bool UseScreenLabel { get; set; }
        
        // fallback vehicle model
        private readonly VehicleModelInformation _basicVehicle = new VehicleModelInformation
            {Width = 2f, Length = 5f, Height = 1.8f, Center = Vector2.zero, WheelDiameter = 0.65f};
        
        // fallback pedestrian model
        private readonly PedestrianModelInformation _basicPedestrian = new PedestrianModelInformation
            {Width = 0.7f, Length = 0.7f, Height = 1.8f, Center = Vector2.zero};
        
        // all Agents of the current visualization run
        private readonly List<Agent> _agents = new List<Agent>();

        private void Start() {
            _roadOcclusionManager = FindObjectOfType<RoadOcclusionManager>();
        }

        /// <summary>
        /// Prepares all the agents for the simulation run, also assigns colors
        /// </summary>
        public void PrepareAgents() {
            // setting a color for each agent
            for (var i = 0; i < _agents.Count; i++) {
                var c = Color.HSVToRGB(i / (float) _agents.Count, .9f, .7f, false);
                var agentMaterial = new Material(agentDesigns.vehicleChassisBase) {color = c};

                _agents[i].ColorMaterial = agentMaterial;
            }
            
            _agents.ForEach(a => a.Prepare());
            
            playbackControl.SetTotalTime(MaxSampleTime);

            // TODO remove (only for debugging)
            _agents.ForEach(a => a.SetIsTarget(true));
        }

        /// <summary>
        /// Initializes a new vehicle agent and returns a reference to it.
        /// </summary>
        /// <param name="modelType">The model type of the agent</param>
        /// <returns>The instantiated agent</returns>
        public VehicleAgent InstantiateVehicleAgent(string modelType) {
            var vehicleAgent = Instantiate(agentDesigns.vehiclePrefab, transform, true);
            
            // adding the RoadNetworkHolder
            vehicleAgent.RoadNetworkHolder = FindObjectOfType<RoadNetworkHolder>();
            vehicleAgent.RoadOcclusionManager = _roadOcclusionManager;
            
            // retrieving prefab for 3d model
            var model = Instantiate(agentDesigns.GetAgentModel(AgentType.Vehicle, modelType).model, vehicleAgent.transform, true);
            _agents.Add(vehicleAgent);
            vehicleAgent.Model = model;

            // adding label
            if (UseScreenLabel) {
                var label = Instantiate(agentDesigns.labelPrefabScreen, labelOcclusionManager.transform);
                label.Agent = vehicleAgent;
                label.LabelOcclusionManager = labelOcclusionManager;
                label.AgentCamera = vehicleAgent.Model.transform.Find("Camera").GetComponent<Camera>();
                labelOcclusionManager.AllLabels.Add(label);
                vehicleAgent.OwnLabel = label;
            } else {
                var label = Instantiate(agentDesigns.labelPrefabScene, transform);
                vehicleAgent.OwnLabel = label;
            }

            // retrieving model information
            vehicleAgent.ModelInformation = VehicleModelCatalog.ContainsKey(modelType)
                ? VehicleModelCatalog[modelType]
                : _basicVehicle;

            return vehicleAgent;
        }
        
        /// <summary>
        /// Initializes a new pedestrian agent and returns a reference to it.
        /// </summary>
        /// <returns>The instantiated agent</returns>
        public PedestrianAgent InstantiatePedestrian(string modelType) {
            var pedestrianAgent = Instantiate(agentDesigns.pedestrianPrefab, transform, true);
            
            // adding the RoadNetworkHolder
            pedestrianAgent.RoadNetworkHolder = FindObjectOfType<RoadNetworkHolder>();
            pedestrianAgent.RoadOcclusionManager = _roadOcclusionManager;
            
            // retrieving prefab for 3d model
            var model = Instantiate(agentDesigns.GetAgentModel(AgentType.Pedestrian, "pedestrian").model, pedestrianAgent.transform, true);
            _agents.Add(pedestrianAgent);
            pedestrianAgent.Model = model;
            
            // adding label TODO use other Label then vehicle!
            if (UseScreenLabel) {
                var label = Instantiate(agentDesigns.labelPrefabScreen, labelOcclusionManager.transform);
                label.Agent = pedestrianAgent;
                label.LabelOcclusionManager = labelOcclusionManager;
                label.AgentCamera = pedestrianAgent.Model.transform.Find("Camera").GetComponent<Camera>();
                labelOcclusionManager.AllLabels.Add(label);
                pedestrianAgent.OwnLabel = label;
            } else {
                var label = Instantiate(agentDesigns.labelPrefabScene, transform);
                pedestrianAgent.OwnLabel = label;
            }
            
            // retrieving model information
            pedestrianAgent.ModelInformation = PedestrianModelCatalog.ContainsKey(modelType)
                ? PedestrianModelCatalog[modelType]
                : _basicPedestrian;
            
            return pedestrianAgent;
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

            foreach (var agent in _agents) {
                agent.UpdateForTimeStep(CurrentTime, PlayBackwards);
            }
            
            _roadOcclusionManager.ChangeRoadLayers();
            playbackControl.UpdateCurrentTime(CurrentTime);
        }
    }
}