using Scenery.RoadNetwork;
using UI.Main_Menu;
using UI.Settings;
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

        private LabelOcclusionManager _labelOcclusionManager;
        private AgentOcclusionManager _agentOcclusionManager;
        // private SettingsPanel _settingsPanel;
        private TargetController _targetController;
        private PlaybackControl _playbackControl;
        private DataMover _dataMover;

        private void Start() {
            FindObjectOfType<LabelOcclusionManager>().Disable = true;
            FindObjectOfType<AgentOcclusionManager>().Disable = true;

            // _settingsPanel = FindObjectOfType<SettingsPanel>();
            _targetController = FindObjectOfType<TargetController>();
            _playbackControl = FindObjectOfType<PlaybackControl>();
            _agentOcclusionManager = FindObjectOfType<AgentOcclusionManager>();
            _labelOcclusionManager = FindObjectOfType<LabelOcclusionManager>();
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

            // moving all agents
            visualizationMaster.SmallUpdate();

            // enabling the occlusion managers
            if (_dataMover.DReaMOutputXmlHandler == null) {
                var loc = FindObjectOfType<LabelOcclusionManager>();
                loc.DisableAllLabels();
                loc.Disable = true;

                // foreach (var label in loc.AllLabels) {
                //     label.gameObject.SetActive(false);
                //     label.AgentCamera.gameObject.SetActive(false);
                // }
                //
                // loc.gameObject.SetActive(false);
            } else {
                FindObjectOfType<LabelOcclusionManager>().Disable = false;
                FindObjectOfType<AgentOcclusionManager>().Disable = false;
            }

            // prepare side panel
            var sidePanel = FindObjectOfType<SidePanel.SidePanel>();
            if (PlayerPrefs.GetInt("app_wip_features") > 0) {
                sidePanel.Setup(_dataMover.SimulationOutputXmlHandler.GetUnknownAttributeNames());
            } else {
                sidePanel.gameObject.SetActive(false);
            }
        }

        private void Prerequisites() {
            // finding necessary elements in the different components
            // _settingsPanel.FindAll();
            _playbackControl.FindAll();
            visualizationMaster.FindAll();
            _agentOcclusionManager.FindAll();
            _targetController.FindAll();
            _labelOcclusionManager.FindAll();
            
            // Debug.Log(PlayerPrefs.GetString("app_occ_min_opacity_agent").Replace(',', '.'));
            // TODO actually parse values

            var appSettings = new ApplicationSettings {
                checkOcclusion = PlayerPrefs.GetInt("app_handleOcclusions") > 0,
                fullscreen = PlayerPrefs.GetInt("app_fullscreen") > 0,
                minimumAgentOpacity = .7f, // float.Parse(PlayerPrefs.GetString("app_occ_min_opacity_agent").Replace(',', '.'), CultureInfo.InvariantCulture),
                minimumObjectOpacity = .3f //float.Parse(PlayerPrefs.GetString("app_occ_min_opacity_object").Replace(',', '.'), CultureInfo.InvariantCulture),
            };
            
            visualizationMaster.settings = appSettings;
            visualizationMaster.DisableLabels = _dataMover.DReaMOutputXmlHandler == null;
            
            roadNetworkHolder.settings = appSettings;
            _agentOcclusionManager.Settings = appSettings;
        }
    }
}