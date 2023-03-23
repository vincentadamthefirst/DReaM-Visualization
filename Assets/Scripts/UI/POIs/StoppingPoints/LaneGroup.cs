using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.POIs.StoppingPoints {
    public class LaneGroup : MonoBehaviour {

        public TMP_Text laneText;
        public Image colorImage;
        
        public Toggle toggle;

        public List<StoppingPointEntry> StoppingPointEntries { get; } = new();
        
        public IntersectionGroup Parent { get; set; }

        public void InitializeData(string laneId, Color color) {
            laneText.text = laneId;
            colorImage.color = color;
            toggle.onValueChanged.AddListener(delegate { ToggleChange(); });
        }

        private void ToggleChange() {
            StoppingPointEntries.ForEach(x => x.toggle.isOn = toggle.isOn);
        }
        
    }
}