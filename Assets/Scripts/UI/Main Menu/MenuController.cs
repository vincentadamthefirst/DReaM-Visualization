using System;
using Importer.XMLHandlers;
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
            XmlHandler sceneryHandler, outputHandler, vehicleHandler, pedestrianHandler, profilesHandler, dreamHandler;

            try {
                sceneryHandler = importController.GetXmlHandler<SceneryXmlHandler>();
                outputHandler = importController.GetXmlHandler<SimulationOutputXmlHandler>();
                vehicleHandler = importController.GetXmlHandler<VehicleModelsXmlHandler>();
                pedestrianHandler = importController.GetXmlHandler<PedestrianModelsXmlHandler>();
                profilesHandler = importController.GetXmlHandler<ProfilesCatalogXmlHandler>();
                dreamHandler = importController.GetXmlHandler<DReaMOutputXmlHandler>();
            } catch (ArgumentNullException e) {
                notificationManager.ShowNotification(NotificationType.Error,
                    "Bitte OpenDrive, Output sowie Pedestrian- und VehicleModelsCatalog auswählen!");
                return;
            }

            var selectedConfigs = sceneryHandler.GetFilePath() + "," + outputHandler.GetFilePath() + "," +
                                  vehicleHandler.GetFilePath() + "," + pedestrianHandler.GetFilePath() + "," +
                                  profilesHandler.GetFilePath();
            PlayerPrefs.SetString("selectedConfigs", selectedConfigs);

            _dataMover.SceneryXmlHandler = (SceneryXmlHandler)sceneryHandler;
            _dataMover.SimulationOutputXmlHandler = (SimulationOutputXmlHandler)outputHandler;
            _dataMover.VehicleModelsXmlHandler = (VehicleModelsXmlHandler)vehicleHandler;
            _dataMover.PedestrianModelsXmlHandler = (PedestrianModelsXmlHandler)pedestrianHandler;
            _dataMover.ProfilesCatalogXmlHandler = (ProfilesCatalogXmlHandler)profilesHandler;
            _dataMover.DReaMOutputXmlHandler = (DReaMOutputXmlHandler)dreamHandler;

            if (_dataMover.DReaMOutputXmlHandler != null)
                _dataMover.DReaMOutputXmlHandler.RunId = importController.GetRunId();
            _dataMover.SimulationOutputXmlHandler.RunId = importController.GetRunId();

            settingsPanel.SaveSettings();
            
            visualizationLoader.gameObject.SetActive(true);
            visualizationLoader.StartSceneLoad(_dataMover);
        }
    }
}