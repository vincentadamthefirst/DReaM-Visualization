using Scenery;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    
    /// <summary>
    /// Class for an entry in the list of all objects. Allows toggling of some objects.
    /// </summary>
    public class ObjectEntry : MonoBehaviour {
        
        public VisualizationElement Object { get; set; }

        public Toggle toggle;
        public TMP_Text text;

        public void ValueChanged() {
            Object.gameObject.SetActive(toggle.isOn);
        }

        public void SetText(string t) {
            text.text = t;
        }
    }
}