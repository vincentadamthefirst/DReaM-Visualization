using System;
using Scenery.RoadNetwork;
using UI;
using UI.Main_Menu;
using UnityEngine;
using Utils;
using Visualization.OcclusionManagement;
using Visualization.RoadOcclusion;

namespace Visualization {
    public class VisualizationStarter : MonoBehaviour {

        public VisualizationMaster visualizationMaster;

        public RoadNetworkHolder roadNetworkHolder;

        public OcclusionManagementOptions occlusionManagementOptions;

        public Terrain terrain;

        private LabelOcclusionManager _labelOcclusionManager;
        private AgentOcclusionManager _agentOcclusionManager;
        private SettingsControl _settingsControl;
        private TargetController _targetController;
        private PlaybackControl _playbackControl;

        private ExtendedCamera _extendedCamera;

        private void Start() {
            _settingsControl = FindObjectOfType<SettingsControl>();
            _targetController = FindObjectOfType<TargetController>();
            _playbackControl = FindObjectOfType<PlaybackControl>();
            
            _agentOcclusionManager = FindObjectOfType<AgentOcclusionManager>();
            _labelOcclusionManager = FindObjectOfType<LabelOcclusionManager>();

            _extendedCamera = FindObjectOfType<ExtendedCamera>();

            BigImport();
            
            // self destruct after complete import
            Destroy(this);
        }

        private void BigImport() {
            // finding the data mover
            var dataMover = FindObjectOfType<DataMover>();

            // finding necessary elements in the different components
            _settingsControl.FindAll();
            _playbackControl.FindAll();
            visualizationMaster.FindAll();
            _agentOcclusionManager.FindAll();
            _targetController.FindAll();
            _labelOcclusionManager.FindAll();

            // setting the OcclusionManagementOptions where they are needed
            visualizationMaster.OcclusionManagementOptions = occlusionManagementOptions;
            roadNetworkHolder.OcclusionManagementOptions = occlusionManagementOptions;
            _agentOcclusionManager.OcclusionManagementOptions = occlusionManagementOptions;
            _agentOcclusionManager.OcclusionManagementOptions = occlusionManagementOptions;

            // setting the Visualization Master
            dataMover.SceneryXmlHandler.VisualizationMaster = visualizationMaster;
            dataMover.SimulationOutputXmlHandler.VisualizationMaster = visualizationMaster;
            dataMover.PedestrianModelsXmlHandler.VisualizationMaster = visualizationMaster;
            dataMover.VehicleModelsXmlHandler.VisualizationMaster = visualizationMaster;

            // Starting Model Import
            dataMover.VehicleModelsXmlHandler.StartImport();
            dataMover.PedestrianModelsXmlHandler.StartImport();
            
            // Starting Scenery import
            dataMover.SceneryXmlHandler.roadNetworkHolder = roadNetworkHolder;
            dataMover.SceneryXmlHandler.StartImport();
            roadNetworkHolder.ShowSimpleGround(terrain);

            // Starting Simulation Output import
            dataMover.SimulationOutputXmlHandler.StartImport();

            // preparing visualization
            _agentOcclusionManager.Prepare();
            visualizationMaster.PrepareAgents();
            _settingsControl.SetOcclusionManager(_agentOcclusionManager);
            _targetController.Prepare();
        }
    }
}