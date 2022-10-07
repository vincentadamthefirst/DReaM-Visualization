using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.POIs.StoppingPoints {
    public class IntersectionGroup : MonoBehaviour {

        public TMP_Text intersectionText;
        
        public Toggle toggle;

        public List<LaneGroup> LaneGroups { get; } = new List<LaneGroup>();

        public void InitializeData(string intersectionId) {
            intersectionText.text = intersectionId;
        }

        public void ToggleChange() {
            LaneGroups.ForEach(x => x.toggle.isOn = toggle.isOn);
        }
        
    }
}