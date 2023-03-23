using System;
using System.Collections.Generic;
using System.IO;
using Importer.XMLHandlers;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Settings {
    [Serializable]
    public class LoadConfiguration {
        public Dictionary<XmlType, string> filePaths = new();
    }

    [Serializable]
    public class Settings {
        // load settings
        public string parentFolder;
        public LoadConfiguration defaultConfiguration;
        public List<LoadConfiguration> storedConfigurations;
        public int showSampleSelectionThreshold = -1;

        // visualization settings
        public bool handleOcclusions;
        public float minimalOpacity = .3f;
        public bool useSidePanel = true;

        // Video settings
        public bool fullscreen;
        public string resolution = "1920x1080";
        public int framerate = 50;
    }

    public class SettingsManager : MonoBehaviour {
        public static SettingsManager Instance { get; private set; }

        public Settings Settings { get; private set; }

        public void ApplySettings() {
            var resWidth = int.Parse(Settings.resolution.Split("x")[0]);
            var resHeight = int.Parse(Settings.resolution.Split("x")[1]);
            
            Screen.SetResolution(resWidth, resHeight, Settings.fullscreen, Settings.framerate);
        }

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(this);
            } else {
                Instance = this;
            }

            DontDestroyOnLoad(this);
            LoadSettings();
        }

        private void LoadSettings() {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var settingsFolder = $"{appData}/DReaMVisualization";

            if (!Directory.Exists(settingsFolder)) {
                Settings = new Settings();
                StoreSettings();
                return;
            }

            var content = File.ReadAllText(settingsFolder + "/settings.json");
            Settings = JsonConvert.DeserializeObject<Settings>(content);
        }

        private void StoreSettings() {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var settingsFolder = $"{appData}/DReaMVisualization";
            Directory.CreateDirectory(settingsFolder);

            var content = JsonConvert.SerializeObject(Settings);
            File.WriteAllText(settingsFolder + "/settings.json", content);
        }

        private void OnDestroy() {
            StoreSettings();
        }
    }
}