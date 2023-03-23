using Importer.XMLHandlers;
using Settings;
using TMPro;
using UI.Main_Menu.Notifications;
using UI.Main_Menu.Settings;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI.Main_Menu {
    public class MenuController : MonoBehaviour {
        [Header("Buttons")] public Button start;
        public Button import;
        public Button settings;
        public Button exit;

        [Header("Views")] public Transform importView;
        public Transform settingsView;

        [Header("Other")] public ImportController importController;
        public NotificationManager notificationManager;
        public SettingsPanel settingsPanel;
        public VisualizationLoader visualizationLoader;

        private DataMover _dataMover;

        public void Start() {
            start.onClick.AddListener(StartButtonClicked);
            import.onClick.AddListener(OpenImport);
            settings.onClick.AddListener(OpenSettings);
            exit.onClick.AddListener(ExitApplication);

            SetupSettingsController();

            _dataMover = FindObjectOfType<DataMover>();
        }

        private void SetupSettingsController() {
            // Occlusion Settings
            settingsPanel.AddHeading("hdg_occ", "Occlusion");
            settingsPanel.AddCheckBox("handle_occ", "Reduce Occlusion:",
                new Reference<bool>(() => SettingsManager.Instance.Settings.handleOcclusions,
                    x => SettingsManager.Instance.Settings.handleOcclusions = x));
            var minOpacityInput = settingsPanel.AddInputField("min_opacity", "Minimum Object Opacity", "Decimal Value",
                new Reference<string>(() => $"{SettingsManager.Instance.Settings.minimalOpacity:F2}",
                    x => SettingsManager.Instance.Settings.minimalOpacity = float.Parse(x)));
            minOpacityInput.Field.contentType = TMP_InputField.ContentType.DecimalNumber;
            settingsPanel.AddRuler(2);

            // Resolution Settings
            settingsPanel.AddHeading("hdg_look", "Graphics");
            settingsPanel.AddCheckBox("fullscreen", "Fullscreen:",
                new Reference<bool>(() => SettingsManager.Instance.Settings.fullscreen,
                    x => SettingsManager.Instance.Settings.fullscreen = x));
            // TODO resolution
            settingsPanel.AddRuler(2);

            // Other Settings
            settingsPanel.AddHeading("hdg_other", "Other");
            settingsPanel.AddCheckBox("sidepanel", "Use Sidepanel",
                new Reference<bool>(() => SettingsManager.Instance.Settings.useSidePanel,
                    x => SettingsManager.Instance.Settings.useSidePanel = x));

            settingsPanel.LoadSettings();
        }

        private void OpenImport() {
            settingsPanel.SaveSettings();
            SettingsManager.Instance.ApplySettings();
            importView.gameObject.SetActive(true);
            settingsView.gameObject.SetActive(false);
        }

        private void OpenSettings() {
            importView.gameObject.SetActive(false);
            settingsView.gameObject.SetActive(true);
            settingsPanel.LoadSettings();
        }

        private void ExitApplication() {
            Application.Quit();
        }

        private void StartButtonClicked() {
            SettingsManager.Instance.ApplySettings();
            StartVisualization();
        }

        private void StartVisualization() {
            var sceneryHandler = importController.GetXmlHandler<SceneryXmlHandler>();
            var outputHandler = importController.GetXmlHandler<SimulationOutputXmlHandler>();
            var vehicleHandler = importController.GetXmlHandler<VehicleModelsXmlHandler>();
            var pedestrianHandler = importController.GetXmlHandler<PedestrianModelsXmlHandler>();
            var profilesHandler = importController.GetXmlHandler<ProfilesCatalogXmlHandler>();
            var dreamHandler = importController.GetXmlHandler<DReaMOutputXmlHandler>();

            if (sceneryHandler == null || outputHandler == null || vehicleHandler == null ||
                pedestrianHandler == null || profilesHandler == null) {
                Debug.LogError("Missing one of the required XmlHandlers.");
                return;
            }

            SettingsManager.Instance.Settings.defaultConfiguration = new LoadConfiguration {
                filePaths = {
                    { XmlType.Scenery, sceneryHandler.GetFilePath() },
                    { XmlType.SimulationOutput, outputHandler.GetFilePath() },
                    { XmlType.VehicleModels, vehicleHandler.GetFilePath() },
                    { XmlType.PedestrianModels, pedestrianHandler.GetFilePath() },
                    { XmlType.ProfilesCatalog, profilesHandler.GetFilePath() },
                }
            };

            if (dreamHandler != null)
                SettingsManager.Instance.Settings.defaultConfiguration.filePaths.Add(XmlType.DReaM,
                    dreamHandler.GetFilePath());

            _dataMover.SceneryXmlHandler = sceneryHandler;
            _dataMover.SimulationOutputXmlHandler = outputHandler;
            _dataMover.VehicleModelsXmlHandler = vehicleHandler;
            _dataMover.PedestrianModelsXmlHandler = pedestrianHandler;
            _dataMover.ProfilesCatalogXmlHandler = profilesHandler;
            _dataMover.DReaMOutputXmlHandler = dreamHandler;

            if (dreamHandler != null)
                _dataMover.DReaMOutputXmlHandler.RunId = importController.GetRunId();
            _dataMover.SimulationOutputXmlHandler.RunId = importController.GetRunId();

            settingsPanel.SaveSettings();

            visualizationLoader.gameObject.SetActive(true);
            visualizationLoader.StartSceneLoad(_dataMover);
        }
    }
}