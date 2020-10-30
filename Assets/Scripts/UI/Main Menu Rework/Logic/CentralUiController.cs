using TMPro;
using UI.Main_Menu_Rework.Elements;
using UI.Main_Menu_Rework.Utils;
using UnityEngine;

namespace UI.Main_Menu_Rework.Logic {
    
    /// <summary>
    /// Class to provide access to any UI settings currently active by the user. An object with this class wont be
    /// destroyed on scene change. There should only be one object with this component attached in the whole project.
    /// </summary>
    public class CentralUiController : MonoBehaviour {

        [Tooltip("The currently active color for all elements.")]
        public ApplicationDesign applicationDesign;

        [Tooltip("Time the mouse ov the user has to be still over an element until the tooltip will be shown (in ms).")]
        public int timeToTooltip = 1000;

        [Tooltip("The object to be used for tooltips.")]
        public RectTransform tooltipTransform;
        private TextMeshProUGUI _tooltipText;

        // TODO add localization

        private void Start() {
            DontDestroyOnLoad(this);
            _tooltipText = tooltipTransform.GetChild(0).GetComponent<TextMeshProUGUI>();
        }

        private void OnValidate() {
            UpdateAllUiElements();
        }

        public void ShowTooltip(string text, Vector2 position) {
            tooltipTransform.gameObject.SetActive(true);
            _tooltipText.text = text;
            tooltipTransform.position = position;
        }

        public void HideTooltip() {
            tooltipTransform.gameObject.SetActive(false);
        }

        /// <summary>
        /// Changes the application color. Also performs a refresh of all UI elements in the scene.
        /// </summary>
        /// <param name="ad">The new color scheme to be used.</param>
        public void ChangeApplicationColor(ApplicationDesign ad) {
            applicationDesign = ad;
            
            // updating all elements
            UpdateAllUiElements();
        }

        /// <summary>
        /// Updates all UI elements in the scene.
        /// </summary>
        private void UpdateAllUiElements() {
            foreach (var customUiElement in FindObjectsOfType<CustomUiElement>()) {
                customUiElement.PerformUpdate();
            }
        }
    }
}