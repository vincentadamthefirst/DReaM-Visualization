using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
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

        private void Start() {
            occlusionManagementOptions.LoadFromPrefs();
            SetFields();
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
            var omo = ScriptableObject.CreateInstance<OcclusionManagementOptions>();

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
        }
    }
}