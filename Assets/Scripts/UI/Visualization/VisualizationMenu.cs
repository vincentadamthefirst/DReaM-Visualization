using System.Collections.Generic;
using Scenery;
using TMPro;
using UI.Main_Menu.Settings;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Visualization.OcclusionManagement;

namespace UI.Visualization {

    public class VisualizationMenu : PanelMenu {

        [Header("Menu Elements")]
        public Button mainMenuButton;
        public Button exitButton;
        public RectTransform menuPanel;

        [Header("Panel Elements")] 
        public SettingsPanel settingsPanel;
        
        private SimpleCameraController _cameraController;
        private PlaybackControl _playbackControl;
        private TargetController _targetController;
        
        public List<VisualizationElement> Elements { get; } = new();
        
        protected override void AfterButtonSetup() {
            mainMenuButton.onClick.AddListener(UnloadVisualization);
            exitButton.onClick.AddListener(ExitApplication);
            FindAll();
            SetupSettingsPanel();
        }

        private void FindAll() {
            _cameraController = FindObjectOfType<SimpleCameraController>();
            _playbackControl = FindObjectOfType<PlaybackControl>();
            _targetController = FindObjectOfType<TargetController>();
        }
        
        public void Rebuild() {
            // // clearing all entries
            // foreach (Transform child in objectsContent.transform) {
            //     Destroy(child);
            // }
            //
            // // adding all elements
            // foreach (var element in Elements) {
            //     var newEntry = Instantiate(objectEntryPrefab, objectsContent);
            //     newEntry.Object = element;
            //     newEntry.name = element.name;
            //     newEntry.SetText(element.name);
            // }

            // LayoutRebuilder.ForceRebuildLayoutImmediate(objectsContent);
        }

        private void SetupSettingsPanel() {
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

            settingsPanel.LoadSettings();
        }

        private void Update() {
            if (!Input.GetKeyDown(KeyCode.Escape)) return;
            if (menuPanel.gameObject.activeSelf) {
                menuPanel.gameObject.SetActive(false);
                _cameraController.SetMenuOpen(false);
                _targetController.SetMenuOpen(false);
                _playbackControl.Disable = false;
            } else {
                menuPanel.gameObject.SetActive(true);
                _playbackControl.Disable = true;
                _cameraController.SetMenuOpen(true);
                _targetController.SetMenuOpen(true);
            }
        }

        private void UnloadVisualization() {
            SceneManager.LoadScene(0);
        }
        
        private void ExitApplication() {
            Application.Quit();
        }
    }
}