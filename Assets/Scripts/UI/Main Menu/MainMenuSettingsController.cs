using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Visualization.Labels;
using Visualization.OcclusionManagement;

namespace UI.Main_Menu {
    public class MainMenuSettingsController : MonoBehaviour {

        public OcclusionManagementOptions occlusionManagementOptions;

        public TMP_Dropdown labelLocation;
        public TMP_Dropdown detectionMethod;
        public TMP_Dropdown handlingMethod;

        public TMP_InputField agentTransparency;
        public TMP_InputField objectTransparency;
        public TMP_InputField randomPointAmount;

        public Toggle nearClipToggle;
        public Toggle randomPointToggle;
        public Toggle preCheckViewFrustumToggle;
        public Toggle staggeredToggle;
        
        // Application Settings
        public TMP_Dropdown resolution;
        public Toggle fullscreen;

        private Resolution[] _resolutions;

        private void Start() {
            _resolutions = Screen.resolutions;
            resolution.ClearOptions();

            // retrieving all possible resolutions
            var resolutionStrings = new List<string>();
            var currentResolution = 0;
            for (var i = 0; i < _resolutions.Length; i++) {
                resolutionStrings.Add(_resolutions[i].width + " x " + _resolutions[i].height + " [" +
                                      _resolutions[i].refreshRate + "Hz]");

                if (_resolutions[i].width == Screen.currentResolution.width &&
                    _resolutions[i].height == Screen.currentResolution.height &&
                    _resolutions[i].refreshRate == Screen.currentResolution.refreshRate) {
                    currentResolution = i;
                }
            }
            
            // updating the dropdown
            resolution.AddOptions(resolutionStrings);
            resolution.value = currentResolution;
            resolution.RefreshShownValue();
            
            // loading screen dimensions & fullscreen mode
            LoadApplicationSettings();
            
            // loading occlusion settings
            occlusionManagementOptions.LoadFromPrefs();
            
            // adding listeners for the application settings
            resolution.onValueChanged.AddListener(SetResolution);
            fullscreen.onValueChanged.AddListener(SetFullscreen);
            
            // updating the fields for occlusion settings
            SetFields();
        }

        private void LoadApplicationSettings() {
            if (!PlayerPrefs.HasKey("app_RES_width") || !PlayerPrefs.HasKey("app_RES_height") ||
                PlayerPrefs.HasKey("app_RES_refresh")) return;
            var fs = PlayerPrefs.HasKey("app_FS") && PlayerPrefs.GetInt("app_FS") == 1;
            fullscreen.isOn = fs;

            var screenWidth = PlayerPrefs.GetInt("app_RES_width");
            var screenHeight = PlayerPrefs.GetInt("app_RES_height");
            var screenRefresh = PlayerPrefs.GetInt("app_RES_refresh");
            
            Screen.SetResolution(screenWidth, screenHeight, fs, screenRefresh);

            for (var i = 0; i < resolution.options.Count; i++) {
                if (_resolutions[i].width != screenWidth || _resolutions[i].height != screenHeight ||
                    _resolutions[i].refreshRate != screenRefresh) continue;
                resolution.SetValueWithoutNotify(i);
                break;
            }
        }

        private void StoreApplicationSettings() {
            PlayerPrefs.SetInt("app_RES_width", _resolutions[resolution.value].width);
            PlayerPrefs.SetInt("app_RES_height", _resolutions[resolution.value].height);
            PlayerPrefs.SetInt("app_RES_refresh", _resolutions[resolution.value].refreshRate);
            PlayerPrefs.SetInt("app_FS", Screen.fullScreen ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void SetFullscreen(bool value) {
            Screen.fullScreen = value;
        }

        private void SetResolution(int value) {
            Screen.SetResolution(_resolutions[value].width, _resolutions[value].height, Screen.fullScreen);
        }

        private void SetFields() {
            labelLocation.SetValueWithoutNotify((int) occlusionManagementOptions.labelLocation);
            detectionMethod.SetValueWithoutNotify((int) occlusionManagementOptions.occlusionDetectionMethod);
            handlingMethod.SetValueWithoutNotify((int) occlusionManagementOptions.occlusionHandlingMethod);

            agentTransparency.SetTextWithoutNotify(occlusionManagementOptions.agentTransparencyValue + "");
            objectTransparency.SetTextWithoutNotify(
                occlusionManagementOptions.objectTransparencyValue + "");
            randomPointAmount.SetTextWithoutNotify(occlusionManagementOptions.randomPointAmount.ToString());
            
            nearClipToggle.SetIsOnWithoutNotify(occlusionManagementOptions.nearClipPlaneAsStart);
            randomPointToggle.SetIsOnWithoutNotify(occlusionManagementOptions.sampleRandomPoints);
            staggeredToggle.SetIsOnWithoutNotify(occlusionManagementOptions.staggeredCheck);
            preCheckViewFrustumToggle.SetIsOnWithoutNotify(occlusionManagementOptions.preCheckViewFrustum);
        }

        public void GetOcclusionManagementOptions() {
            var omo = occlusionManagementOptions;

            omo.labelLocation = (LabelLocation) labelLocation.value;
            omo.occlusionDetectionMethod = (OcclusionDetectionMethod) detectionMethod.value;
            omo.occlusionHandlingMethod = (OcclusionHandlingMethod) handlingMethod.value;

            var testA = float.TryParse(agentTransparency.text.Replace(".", ","), out omo.agentTransparencyValue);
            if (!testA) {
                omo.agentTransparencyValue = .18f;
                agentTransparency.SetTextWithoutNotify("0,18");
            }
            
            var testB = float.TryParse(objectTransparency.text.Replace(".", ","), out omo.objectTransparencyValue);
            if (!testB) {
                omo.objectTransparencyValue = .1f;
                objectTransparency.SetTextWithoutNotify("0,1");
            }
            
            var testC = int.TryParse(randomPointAmount.text, out omo.randomPointAmount);
            if (!testC) {
                omo.randomPointAmount = 10;
                randomPointAmount.SetTextWithoutNotify("10");
            }

            omo.nearClipPlaneAsStart = nearClipToggle.isOn;
            omo.sampleRandomPoints = randomPointToggle.isOn;
            omo.preCheckViewFrustum = preCheckViewFrustumToggle.isOn;
            omo.staggeredCheck = staggeredToggle.isOn;

            occlusionManagementOptions = omo;
        }
        
        private void OnDestroy() {
            occlusionManagementOptions.StoreToPrefs();
            StoreApplicationSettings();
        }
    }
}