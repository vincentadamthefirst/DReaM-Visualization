using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.POIs.StoppingPoints {
    public class LaneGroup : MonoBehaviour {

        public TMP_Text laneText;
        public Image colorImage;
        
        public Toggle toggle;

        public List<StoppingPointEntry> StoppingPointEntries { get; } = new List<StoppingPointEntry>();
        
        public IntersectionGroup Parent { get; set; }

        public void InitializeData(string laneId, Color color) {
            laneText.text = laneId;
            colorImage.color = color;
        }

        public void ToggleChange() {
            StoppingPointEntries.ForEach(x => x.toggle.isOn = toggle.isOn);
        }
        
    }
}