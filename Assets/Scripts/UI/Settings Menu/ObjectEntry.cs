using System;
using Scenery;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    
    /// <summary>
    /// Class for an entry in the list of all objects. Allows toggling of some objects.
    /// </summary>
    public class ObjectEntry : MonoBehaviour {
        
        public VisualizationElement Object { get; set; }

        private Toggle _toggle;
        
        private void Start() {
            _toggle = transform.GetChild(0).GetComponent<Toggle>();
        }

        public void ValueChanged() {
            Object.gameObject.SetActive(_toggle.isOn);
        }
    }
}