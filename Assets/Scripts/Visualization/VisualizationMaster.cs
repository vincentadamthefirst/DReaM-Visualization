using System.Collections.Generic;
using Scenery.RoadNetwork;
using UI;
using UnityEngine;
using Visualization.Agents;
using Visualization.OcclusionManagement;

namespace Visualization {
    public class VisualizationMaster : MonoBehaviour {
        private PlaybackControl _playbackControl;
        private LabelOcclusionManager _labelOcclusionManager;
        private RoadOcclusionManager _roadOcclusionManager;

        public OcclusionManagementOptions OcclusionManagementOptions { get; set; }

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

        // fallback vehicle model
        private readonly VehicleModelInformation _basicVehicle = new VehicleModelInformation
            {Width = 2f, Length = 5f, Height = 1.8f, Center = Vector3.zero, WheelDiameter = 0.65f};
        
        // fallback pedestrian model
        private readonly PedestrianModelInformation _basicPedestrian = new PedestrianModelInformation
            {Width = 0.7f, Length = 0.7f, Height = 1.8f, Center = Vector3.zero};
        
        // all Agents of the current visualization run
        private readonly List<Agent> _agents = new List<Agent>();

        public void FindAll() {
            _roadOcclusionManager = FindObjectOfType<RoadOcclusionManager>();
            _labelOcclusionManager = FindObjectOfType<LabelOcclusionManager>();
            _playbackControl = FindObjectOfType<PlaybackControl>();
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
            
            _playbackControl.SetTotalTime(MaxSampleTime);
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
            vehicleAgent.OcclusionManagementOptions = OcclusionManagementOptions;
            
            // retrieving prefab for 3d model
            var model = Instantiate(agentDesigns.GetAgentModel(AgentType.Vehicle, modelType).model, vehicleAgent.transform, true);
            _agents.Add(vehicleAgent);
            vehicleAgent.Model = model;

            // adding label
            if (OcclusionManagementOptions.labelLocation == LabelLocation.Screen) {
                var label = Instantiate(agentDesigns.labelPrefabScreen, _labelOcclusionManager.transform);
                label.Agent = vehicleAgent;
                label.LabelOcclusionManager = _labelOcclusionManager;
                label.AgentCamera = vehicleAgent.Model.transform.Find("Camera").GetComponent<Camera>();
                _labelOcclusionManager.AddLabel(label);
                vehicleAgent.OwnLabel = label;
            } else {
                var label = Instantiate(agentDesigns.labelPrefabScene, transform);
                label.AgentCamera = vehicleAgent.Model.transform.Find("Camera").GetComponent<Camera>();
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
            pedestrianAgent.OcclusionManagementOptions = OcclusionManagementOptions;
            
            // retrieving prefab for 3d model
            var model = Instantiate(agentDesigns.GetAgentModel(AgentType.Pedestrian, "pedestrian").model, pedestrianAgent.transform, true);
            _agents.Add(pedestrianAgent);
            pedestrianAgent.Model = model;
            
            // adding label TODO use other Label then vehicle!
            if (OcclusionManagementOptions.labelLocation == LabelLocation.Screen) {
                var label = Instantiate(agentDesigns.labelPrefabScreen, _labelOcclusionManager.transform);
                label.Agent = pedestrianAgent;
                label.LabelOcclusionManager = _labelOcclusionManager;
                label.AgentCamera = pedestrianAgent.Model.transform.Find("Camera").GetComponent<Camera>();
                _labelOcclusionManager.AddLabel(label);
                pedestrianAgent.OwnLabel = label;
            } else {
                var label = Instantiate(agentDesigns.labelPrefabScene, transform);
                label.AgentCamera = pedestrianAgent.Model.transform.Find("Camera").GetComponent<Camera>();
                pedestrianAgent.OwnLabel = label;
            }
            

            // retrieving model information
            pedestrianAgent.ModelInformation = PedestrianModelCatalog.ContainsKey(modelType)
                ? PedestrianModelCatalog[modelType]
                : _basicPedestrian;
            
            return pedestrianAgent;
        }

        public void SmallUpdate() {
            if (CurrentTime < 0) CurrentTime = 0;
            else if (CurrentTime > MaxSampleTime) CurrentTime = MaxSampleTime;

            foreach (var agent in _agents) {
                agent.UpdateForTimeStep(CurrentTime, PlayBackwards);
            }
            
            _roadOcclusionManager.ChangeRoadLayers();
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

            foreach (var agent in _agents) {
                agent.UpdateForTimeStep(CurrentTime, PlayBackwards);
            }
            
            if (OcclusionManagementOptions.occlusionDetectionMethod == OcclusionDetectionMethod.Shader) 
                _roadOcclusionManager.ChangeRoadLayers();
            _playbackControl.UpdateCurrentTime(CurrentTime);
        }
    }
}