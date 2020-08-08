using System.Collections.Generic;
using Scenery;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Visualization.OcclusionManagement;

namespace UI {
    [RequireComponent(typeof(AgentOcclusionManager))]
    public class SettingsController : MonoBehaviour {

        // the images for the buttons on the left side
        public Image objSetImage;
        public Image occSetImage;
        public Image bscSetImage;

        // the views for the settings
        public RectTransform objectSettings;
        public RectTransform basicSettings;
        public RectTransform occlusionSettings;

        public ObjectEntry objectEntryPrefab;
        public RectTransform paddingPrefab;
        
        /// <summary>
        /// If the settings window should be disabled
        /// </summary>
        public bool Disable { get; set; }

        /// <summary>
        /// The list of all objects in the scene that can be deactivated (buildings, trees, ...)
        /// </summary>
        public List<VisualizationElement> Elements { get; } = new List<VisualizationElement>();
        
        // Used to propagate any changes made to the occlusion options
        private AgentOcclusionManager _agentOcclusionManager;

        // the content for objects
        private RectTransform _objectContent;

        // the actual Settings Panel
        private RectTransform _allContent;
        
        // all the occlusion settings
        private Toggle _randomPointSampling;
        private Toggle _nearClipPlaneAsStart;
        private Toggle _staggeredCheck;
        private Toggle _preCheckViewFrustum;
        
        private TMP_Dropdown _handlingMethod;
        private TMP_Dropdown _detectionMethod;

        private TMP_InputField _agentTransparency;
        private TMP_InputField _objectTransparency;
        private TMP_InputField _randomPointAmount;

        private bool _handlingMethodChanged;
        private bool _majorChange;
        
        // the camera script
        private SimpleCameraController _cameraController;
        
        // target controller
        private TargetController _targetController;

        public void FindAll() {
            _objectContent = objectSettings.GetChild(0).GetChild(0).GetChild(0).GetComponent<RectTransform>();
            _allContent = transform.GetChild(0).GetComponent<RectTransform>();

            _cameraController = FindObjectOfType<SimpleCameraController>();
            _targetController = FindObjectOfType<TargetController>();

            _randomPointSampling = occlusionSettings.GetChild(0).GetChild(0).GetChild(0).GetChild(2).GetChild(1)
                .GetChild(0).GetComponent<Toggle>();
            _randomPointAmount = occlusionSettings.GetChild(0).GetChild(0).GetChild(0).GetChild(2).GetChild(2)
                .GetChild(0).GetChild(1).GetComponent<TMP_InputField>();
            _nearClipPlaneAsStart = occlusionSettings.GetChild(0).GetChild(0).GetChild(0).GetChild(2).GetChild(3)
                .GetChild(0).GetComponent<Toggle>();

            _preCheckViewFrustum = occlusionSettings.GetChild(0).GetChild(0).GetChild(0).GetChild(5).GetChild(0)
                .GetComponent<Toggle>();
            _staggeredCheck = occlusionSettings.GetChild(0).GetChild(0).GetChild(0).GetChild(6).GetChild(0)
                .GetComponent<Toggle>();

            _detectionMethod = occlusionSettings.GetChild(0).GetChild(0).GetChild(0).GetChild(1).GetChild(2)
                .GetComponent<TMP_Dropdown>();
            _handlingMethod = occlusionSettings.GetChild(0).GetChild(0).GetChild(0).GetChild(3).GetChild(2)
                .GetComponent<TMP_Dropdown>();

            _agentTransparency = occlusionSettings.GetChild(0).GetChild(0).GetChild(0).GetChild(4).GetChild(1)
                .GetChild(0).GetChild(1).GetComponent<TMP_InputField>();
            _objectTransparency = occlusionSettings.GetChild(0).GetChild(0).GetChild(0).GetChild(4).GetChild(2)
                .GetChild(0).GetChild(1).GetComponent<TMP_InputField>();
        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.Escape) && !Disable) {
                if (_allContent.gameObject.activeSelf) {
                    _allContent.gameObject.SetActive(false);

                    if (_majorChange) {
                        ChangeOcclusionMajor();
                        _majorChange = false;
                    }
                    
                    if (_handlingMethodChanged) {
                        ChangeHandlingMethod();
                        _handlingMethodChanged = false;
                    }

                    _cameraController.SetSettingsOpen(false);
                    _targetController.SetSettingsOpen(false);
                } else {
                    _allContent.gameObject.SetActive(true);
                    _cameraController.SetSettingsOpen(true);
                    _targetController.SetSettingsOpen(true);
                }
            }
        }

        /// <summary>
        /// To perform a change in occlusion handling.
        /// </summary>
        private void ChangeHandlingMethod() {
            var testA = float.TryParse(_agentTransparency.text.Replace(".", ","), out var agentTransparency);
            if (!testA) {
                agentTransparency = .3f;
               _agentTransparency.SetTextWithoutNotify("0,3");
            }
            
            var testB = float.TryParse(_objectTransparency.text.Replace(".", ","), out var objectTransparency);
            if (!testB) {
                objectTransparency = .1f;
                _objectTransparency.SetTextWithoutNotify("0,1");
            }

            _agentOcclusionManager.OcclusionManagementOptions.agentTransparencyValue = agentTransparency;
            _agentOcclusionManager.OcclusionManagementOptions.objectTransparencyValue = objectTransparency;

            _agentOcclusionManager.OcclusionManagementOptions.occlusionHandlingMethod =
                (OcclusionHandlingMethod) _handlingMethod.value;
            
            _agentOcclusionManager.MinorUpdate();
        }

        /// <summary>
        /// To perform a major change in occlusion handling.
        /// </summary>
        private void ChangeOcclusionMajor() {
            var testA = int.TryParse(_randomPointAmount.text, out var randomPointAmount);

            if (!testA) {
                randomPointAmount = 15;
                _randomPointAmount.SetTextWithoutNotify("15");
            }

            _agentOcclusionManager.OcclusionManagementOptions.nearClipPlaneAsStart = _nearClipPlaneAsStart.isOn;
            _agentOcclusionManager.OcclusionManagementOptions.preCheckViewFrustum = _preCheckViewFrustum.isOn;
            _agentOcclusionManager.OcclusionManagementOptions.staggeredCheck = _staggeredCheck.isOn;
            _agentOcclusionManager.OcclusionManagementOptions.sampleRandomPoints = _randomPointSampling.isOn;
            _agentOcclusionManager.OcclusionManagementOptions.randomPointAmount = randomPointAmount;
            _agentOcclusionManager.OcclusionManagementOptions.occlusionDetectionMethod =
                (OcclusionDetectionMethod) _detectionMethod.value;
            
            _agentOcclusionManager.MajorUpdate();
        }

        public void Rebuild() {
            // clearing all entries
            foreach (Transform child in _objectContent.transform) {
                Destroy(child);
            }
            
            // adding padding
            Instantiate(paddingPrefab, _objectContent);

            // adding all elements
            foreach (var element in Elements) {
                var newEntry = Instantiate(objectEntryPrefab, _objectContent);
                newEntry.Object = element;
                newEntry.name = element.name;
                newEntry.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = element.name;
            }
            
            // adding padding
            Instantiate(paddingPrefab, _objectContent);
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(_objectContent);
        }

        /// <summary>
        /// This method is called when the occlusion handling method (or transparency values) is changed.
        /// </summary>
        public void OcclusionHandlingMethodChange() {
            _handlingMethodChanged = true;
        }

        /// <summary>
        /// This method is called when there is a major change in the occlusion handling settings.
        /// </summary>
        public void OcclusionMajorChange() {
            _majorChange = true;
        }

        public void SetOcclusionManager(AgentOcclusionManager occlusionManager) {
            _agentOcclusionManager = occlusionManager;

            _objectTransparency.SetTextWithoutNotify(
                _agentOcclusionManager.OcclusionManagementOptions.objectTransparencyValue.ToString("0.000"));
            _agentTransparency.SetTextWithoutNotify(
                _agentOcclusionManager.OcclusionManagementOptions.agentTransparencyValue.ToString("0.000"));
            _randomPointAmount.SetTextWithoutNotify(_agentOcclusionManager.OcclusionManagementOptions.randomPointAmount
                .ToString());

            _detectionMethod.SetValueWithoutNotify((int) _agentOcclusionManager.OcclusionManagementOptions.occlusionDetectionMethod);
            _handlingMethod.SetValueWithoutNotify((int) _agentOcclusionManager.OcclusionManagementOptions.occlusionHandlingMethod);

            _preCheckViewFrustum.SetIsOnWithoutNotify(_agentOcclusionManager.OcclusionManagementOptions
                .preCheckViewFrustum);
            _nearClipPlaneAsStart.SetIsOnWithoutNotify(_agentOcclusionManager.OcclusionManagementOptions
                .nearClipPlaneAsStart);
            _randomPointSampling.SetIsOnWithoutNotify(_agentOcclusionManager.OcclusionManagementOptions
                .sampleRandomPoints);
            _staggeredCheck.SetIsOnWithoutNotify(_agentOcclusionManager.OcclusionManagementOptions.staggeredCheck);
        }

        public void OpenBasicSettings() {
            basicSettings.gameObject.SetActive(true);
            bscSetImage.color = new Color(1, 1, 1, .2f);
            occlusionSettings.gameObject.SetActive(false);
            occSetImage.color = new Color(1, 1, 1, 0f);
            objectSettings.gameObject.SetActive(false);
            objSetImage.color = new Color(1, 1, 1, 0f);
        }

        public void OpenOcclusionSettings() {
            basicSettings.gameObject.SetActive(false);
            bscSetImage.color = new Color(1, 1, 1, 0f);
            occlusionSettings.gameObject.SetActive(true);
            occSetImage.color = new Color(1, 1, 1, .2f);
            objectSettings.gameObject.SetActive(false);
            objSetImage.color = new Color(1, 1, 1, 0f);
        }

        public void OpenObjectSettings() {
            basicSettings.gameObject.SetActive(false);
            bscSetImage.color = new Color(1, 1, 1, 0f);
            occlusionSettings.gameObject.SetActive(false);
            occSetImage.color = new Color(1, 1, 1, 0f);
            objectSettings.gameObject.SetActive(true);
            objSetImage.color = new Color(1, 1, 1, .2f);
        }
    }
}
