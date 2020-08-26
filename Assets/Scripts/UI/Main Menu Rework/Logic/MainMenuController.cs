using System;
using UI.Main_Menu_Rework.Elements;
using UI.Main_Menu_Rework.Utils;
using UnityEngine;

namespace UI.Main_Menu_Rework.Logic {
    public class MainMenuController : MonoBehaviour {
        
        public CustomUiMainMenuButton dashboardButton;
        public CustomUiMainMenuButton importButton;
        public CustomUiMainMenuButton settingsButton;
        public CustomUiMainMenuButton changelogButton;
        public CustomUiMainMenuButton helpButton;
        public CustomUiMainMenuButton exitButton;

        private Transform _dashboard;
        private Transform _import;
        private Transform _help;
        private Transform _settings;
        private Transform _changelog; 

        private void Start() {
            _dashboard = transform.Find("Dashboard");
            _import = transform.Find("Import");
            _changelog = transform.Find("Changelogs");
            _settings = transform.Find("Settings");
            _help = transform.Find("Help");

            DeactivateAll();
            AddButtonListeners();
            
            _dashboard.SetActiveChildren(true);
            dashboardButton.SetSelected(true);
        }

        private void AddButtonListeners() {
            dashboardButton.ButtonClicked += () => {
                DeactivateAll();
                _dashboard.SetActiveChildren(true);
                dashboardButton.SetSelected(true);
            };
            
            importButton.ButtonClicked += () => {
                DeactivateAll();
                _import.SetActiveChildren(true);
                importButton.SetSelected(true);
            };
            
            settingsButton.ButtonClicked += () => {
                DeactivateAll();
                _settings.SetActiveChildren(true);
                settingsButton.SetSelected(true);
            };
            
            changelogButton.ButtonClicked += () => {
                DeactivateAll();
                _changelog.SetActiveChildren(true);
                changelogButton.SetSelected(true);
            };
            
            helpButton.ButtonClicked += () => {
                DeactivateAll();
                _help.SetActiveChildren(true);
                helpButton.SetSelected(true);
            };
            
            exitButton.ButtonClicked += () => {
                DeactivateAll();
                exitButton.SetSelected(true);
                Application.Quit();
            };
        }

        private void DeactivateAll() {
            _dashboard.SetActiveChildren(false);
            _import.SetActiveChildren(false);
            _help.SetActiveChildren(false);
            _settings.SetActiveChildren(false);
            _changelog.SetActiveChildren(false);

            dashboardButton.SetSelected(false);
            importButton.SetSelected(false);
            helpButton.SetSelected(false);
            settingsButton.SetSelected(false);
            changelogButton.SetSelected(false);
            exitButton.SetSelected(false);
        }
    }
}