using System;
using Evaluation;
using Scenery.RoadNetwork;
using UI;
using UI.Main_Menu;
using UnityEngine;
using Utils;
using Visualization.OcclusionManagement;

namespace Visualization {
    public class VisualizationStarter : MonoBehaviour {

        public VisualizationMaster visualizationMaster;

        public RoadNetworkHolder roadNetworkHolder;

        public OcclusionManagementOptions occlusionManagementOptions;

        public Terrain terrain;

        private LabelOcclusionManager _labelOcclusionManager;
        private AgentOcclusionManager _agentOcclusionManager;
        private RoadOcclusionManager _roadOcclusionManager;
        private SettingsController _settingsController;
        private TargetController _targetController;
        private PlaybackControl _playbackControl;
        private DataMover _dataMover;

        private ExtendedCamera _extendedCamera;

        private void Start() {
            _settingsController = FindObjectOfType<SettingsController>();
            _targetController = FindObjectOfType<TargetController>();
            _playbackControl = FindObjectOfType<PlaybackControl>();
            
            _agentOcclusionManager = FindObjectOfType<AgentOcclusionManager>();
            _labelOcclusionManager = FindObjectOfType<LabelOcclusionManager>();
            _roadOcclusionManager = FindObjectOfType<RoadOcclusionManager>();
            
            _dataMover = FindObjectOfType<DataMover>();
            
            // build necessary objects for the evaluation
            SetupEvaluationMeasurement();

            if (_dataMover.FpsTestType == FpsTest.None) {
                switch (_dataMover.EvaluationType) {
                    case EvaluationType.None:
                        BigImport();
                        break;
                    case EvaluationType.CountingOccOff:
                        Evaluation_Setup(true, true, true);
                        break;
                    case EvaluationType.CountingOccOn:
                        Evaluation_Setup(false, true, true);
                        _agentOcclusionManager.SetAllTargets();
                        break;
                    case EvaluationType.LabelScene:
                    case EvaluationType.LabelScreen:
                        Evaluation_Setup(false, false, false);
                        break;
                    case EvaluationType.OccTransparency:
                    case EvaluationType.OccWireFrame:
                    case EvaluationType.OccShader:
                        Evaluation_Setup(false, false, false);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            } else {
                Evaluation_Setup(false, false, true);
                _agentOcclusionManager.SetAllTargets();
            }

            // self destruct after complete import
            Destroy(this);
        }

        private void BigImport() {
            // finding necessary elements in the different components
            _settingsController.FindAll();
            _playbackControl.FindAll();
            visualizationMaster.FindAll();
            _agentOcclusionManager.FindAll();
            _targetController.FindAll();
            _labelOcclusionManager.FindAll();

            // setting the OcclusionManagementOptions where they are needed
            visualizationMaster.OcclusionManagementOptions = occlusionManagementOptions;
            roadNetworkHolder.OcclusionManagementOptions = occlusionManagementOptions;
            _agentOcclusionManager.OcclusionManagementOptions = occlusionManagementOptions;

            // setting the Visualization Master
            _dataMover.SceneryXmlHandler.VisualizationMaster = visualizationMaster;
            _dataMover.SimulationOutputXmlHandler.VisualizationMaster = visualizationMaster;
            _dataMover.PedestrianModelsXmlHandler.VisualizationMaster = visualizationMaster;
            _dataMover.VehicleModelsXmlHandler.VisualizationMaster = visualizationMaster;

            // Starting Model Import
            _dataMover.VehicleModelsXmlHandler.StartImport();
            _dataMover.PedestrianModelsXmlHandler.StartImport();
            
            // Starting Scenery import
            _dataMover.SceneryXmlHandler.roadNetworkHolder = roadNetworkHolder;
            _dataMover.SceneryXmlHandler.StartImport();
            roadNetworkHolder.ShowSimpleGround(terrain);

            // Starting Simulation Output import
            _dataMover.SimulationOutputXmlHandler.StartImport();

            // preparing visualization
            _agentOcclusionManager.Prepare();
            visualizationMaster.PrepareAgents();
            _settingsController.SetOcclusionManager(_agentOcclusionManager);
            _targetController.Prepare();
            
            // moving all agents
            visualizationMaster.SmallUpdate();
        }

        /// <summary>
        /// Sets up the scene for evaluation, can disable agentOcclusion, labelOcclusion, settings
        /// </summary>
        private void Evaluation_Setup(bool disableAgentOcclusion, bool disableLabelOcclusion, bool disableTargetSelection) {
            // finding the data mover
            var dataMover = FindObjectOfType<DataMover>();

            // finding necessary elements in the different components and disabling unused components
            _settingsController.FindAll();
            _playbackControl.FindAll();
            visualizationMaster.FindAll();
            _agentOcclusionManager.FindAll();
            _targetController.FindAll();
            _labelOcclusionManager.FindAll();
            
            _settingsController.Disable = true;
            _agentOcclusionManager.Disable = disableAgentOcclusion;
            _targetController.Disable = disableTargetSelection;
            _labelOcclusionManager.Disable = disableLabelOcclusion;

            // setting the OcclusionManagementOptions where they are needed
            visualizationMaster.OcclusionManagementOptions = dataMover.occlusionManagementOptions;
            roadNetworkHolder.OcclusionManagementOptions = dataMover.occlusionManagementOptions;
            _agentOcclusionManager.OcclusionManagementOptions = dataMover.occlusionManagementOptions;

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
            _settingsController.SetOcclusionManager(_agentOcclusionManager);
            _targetController.Prepare();

            // moving all agents
            visualizationMaster.SmallUpdate();
            
            _targetController.gameObject.SetActive(!disableTargetSelection);
            _settingsController.gameObject.SetActive(false);
        }

        /// <summary>
        /// Prepares the Measurement of time, button presses and similar
        /// </summary>
        private void SetupEvaluationMeasurement() {
            var holder = new GameObject("Evaluation");

            _roadOcclusionManager.ExecutionMeasurement = new ExecutionMeasurement();
            _labelOcclusionManager.ExecutionMeasurement = new ExecutionMeasurement();
            _agentOcclusionManager.DetectionMeasurement = new ExecutionMeasurement();
            _agentOcclusionManager.HandlingMeasurement = new ExecutionMeasurement();

            switch (_dataMover.FpsTestType) {
                case FpsTest.None:
                    _roadOcclusionManager.ExecutionMeasurement.Disable = true;
                    _labelOcclusionManager.ExecutionMeasurement.Disable = true;
                    _agentOcclusionManager.DetectionMeasurement.Disable = true;
                    _agentOcclusionManager.HandlingMeasurement.Disable = true;

                    // no qualitative evaluation needed
                    if (_dataMover.EvaluationType == EvaluationType.None) {
                        Destroy(holder);
                        return;
                    }
                    
                    var qe = holder.AddComponent<QualitativeEvaluation>();
                    qe.TestPersonId = _dataMover.EvaluationPersonString;
                    qe.EvaluationType = _dataMover.EvaluationType;
                    return;
                default: // RayCast / Polygon / Shader
                    var eval = holder.AddComponent<QuantitativeEvaluation>();
                    eval.FpsTestType = _dataMover.FpsTestType;

                    eval.LabelPlacementMeasurement = _labelOcclusionManager.ExecutionMeasurement;
                    eval.RoadOcclusionMeasurement = _roadOcclusionManager.ExecutionMeasurement;
                    eval.DetectionMeasurement = _agentOcclusionManager.DetectionMeasurement;
                    eval.HandlingMeasurement = _agentOcclusionManager.HandlingMeasurement;
                    return;
            }
        }
    }
}