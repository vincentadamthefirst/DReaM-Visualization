using System.Linq;
using Scenery.RoadNetwork;
using Settings;
using UI.Main_Menu;
using UI.Visualization;
using UnityEngine;
using Visualization;
using Visualization.Labels;
using Visualization.OcclusionManagement;
using Visualization.POIs;

namespace UI {
    public class VisualizationStarter : MonoBehaviour {
        [Header("Scene")]
        public VisualizationMaster visualizationMaster;
        public RoadNetworkHolder roadNetworkHolder;
        public Terrain terrain;
        
        private AgentOcclusionManager _agentOcclusionManager;
        private TargetController _targetController;
        private PlaybackControl _playbackControl;
        private DataMover _dataMover;

        private void Start() {
            FindObjectOfType<AgentOcclusionManager>().Disable = true;
            
            _targetController = FindObjectOfType<TargetController>();
            _playbackControl = FindObjectOfType<PlaybackControl>();
            _agentOcclusionManager = FindObjectOfType<AgentOcclusionManager>();
            _dataMover = FindObjectOfType<DataMover>();
            
            VisualizationMaster.Instance.ActiveModules.DReaM = _dataMover.DReaMOutputXmlHandler != null;
            LoadAll();
            LabelManager.Instance.CollectAgents();
            SensorManager.Instance.CollectAgents();

            _dataMover.SceneryXmlHandler = null;
            _dataMover.PedestrianModelsXmlHandler = null;
            _dataMover.ProfilesCatalogXmlHandler = null;
            _dataMover.SimulationOutputXmlHandler = null;
            _dataMover.VehicleModelsXmlHandler = null;
            _dataMover.DReaMOutputXmlHandler = null;
        }

        /// <summary>
        /// Starts the import / loading process for all handlers.
        /// </summary>
        private void LoadAll() {
            Prerequisites();

            // Starting Model Import
            _dataMover.VehicleModelsXmlHandler.StartImport();
            _dataMover.PedestrianModelsXmlHandler.StartImport();

            // Starting Scenery import
            _dataMover.SceneryXmlHandler.roadNetworkHolder = roadNetworkHolder;
            _dataMover.SceneryXmlHandler.StartImport();
            roadNetworkHolder.ShowSimpleGround(terrain);

            // Starting Simulation Output import & profiles import
            _dataMover.SimulationOutputXmlHandler.StartImport();
            _dataMover.DReaMOutputXmlHandler?.StartImport();
            _dataMover.ProfilesCatalogXmlHandler.StartImport();

            // preparing visualization
            _agentOcclusionManager.Prepare();
            visualizationMaster.PrepareAgents();
            _targetController.Prepare();
            
            if (_dataMover.DReaMOutputXmlHandler != null) {
                // prepare conflict areas
                var cav = FindObjectOfType<ConflictAreaVisualizer>();
                cav.ConflictAreaMapping = _dataMover.DReaMOutputXmlHandler.ConflictAreaMapping;
                cav.GenerateObjects();

                // prepare stopping points
                var spv = FindObjectOfType<StoppingPointVisualizer>();
                spv.IntersectionStoppingPoints = _dataMover.DReaMOutputXmlHandler.StoppingPoints;
                spv.GenerateObjects();
            }
            
            FindObjectsOfType<HoverableElement>().ToList().ForEach(x => x.FindOutlines());
            // moving all agents
            visualizationMaster.SmallUpdate();

            // enabling the occlusion managers
            if (_dataMover.DReaMOutputXmlHandler != null) {
                FindObjectOfType<AgentOcclusionManager>().Disable = false;
            }

            // prepare side panel
            var sidePanel = FindObjectOfType<SidePanel.SidePanel>();
            if (SettingsManager.Instance.Settings.useSidePanel) {
                sidePanel.CollectAgents();
            } else {
                Destroy(sidePanel.gameObject);
            }
        }

        private void Prerequisites() {
            // finding necessary elements in the different components
            _playbackControl.FindAll();
            visualizationMaster.FindAll();
            _agentOcclusionManager.FindAll();
            _targetController.FindAll();
            visualizationMaster.DisableLabels = _dataMover.DReaMOutputXmlHandler == null;
        }
    }
}