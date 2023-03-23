using Importer.XMLHandlers;
using Settings;
using TMPro;
using UI.Main_Menu.Notifications;
using UI.Main_Menu.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Main_Menu {
    public class MenuController : MonoBehaviour {
        [Header("Buttons")] 
        public Button start;
        public Button import;
        public Button settings;
        public Button exit;

        [Header("Views")]
        public Transform importView;
        public Transform settingsView;

        [Header("Other")]
        public ImportController importController;
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
            settingsPanel.AddHeading("hdg_occ", "Verdeckungs - Einstellungen");
            settingsPanel.AddCheckBox("app_handleOcclusions", "Verdeckungen Vermeiden:", true, "hdg_occ");
            var inField1 = settingsPanel.AddInputField("app_occ_min_opacity_agent", "Minimale Agent-Sichtbarkeit",
                "Komma-Zahl", "0,7", "app_handleOcclusions");
            inField1.inputField.contentType = TMP_InputField.ContentType.DecimalNumber;
            var inField2 = settingsPanel.AddInputField("app_occ_min_opacity_other", "Minimale Objekt-Sichtbarkeit",
                "Komma-Zahl", "0,3", "app_handleOcclusions");
            inField2.inputField.contentType = TMP_InputField.ContentType.DecimalNumber;
            settingsPanel.AddRuler(2);

            // Resolution Settings
            settingsPanel.AddHeading("hdg_look", "Grafik - Einstellungen");
            settingsPanel.AddCheckBox("app_fullscreen", "Fullscreen:", true, "hdg_look");
            // TODO resolution
            settingsPanel.AddRuler(2);

            // Other Settings
            settingsPanel.AddHeading("hdg_other", "Anwendungs - Einstellungen");
            settingsPanel.AddCheckBox("app_wip_features", "W.I.P. Features nutzen:", true, "hdg_other");
            var inField3 = settingsPanel.AddInputField("app_samples_show_selection",
                "Sample Anzahl für Cutoff Trigger", "Zahl", "100", "app_wip_features");
            inField3.inputField.contentType = TMP_InputField.ContentType.IntegerNumber;

            settingsPanel.LoadSettings();
        }

        private void OpenImport() {
            settingsPanel.SaveSettings();
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