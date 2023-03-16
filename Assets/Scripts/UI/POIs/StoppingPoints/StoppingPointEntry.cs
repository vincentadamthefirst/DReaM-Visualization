using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Visualization.POIs;

namespace UI.POIs.StoppingPoints {
    public class StoppingPointEntry : MonoBehaviour {

        public TMP_Text road;
        public TMP_Text lane;
        public TMP_Text type;

        public Toggle toggle;
        
        public LaneGroup Parent { get; set; }

        private StoppingPoint _stoppingPoint;

        public void InitializeData(StoppingPoint stoppingPoint) {
            _stoppingPoint = stoppingPoint;
            toggle.onValueChanged.AddListener(delegate { ToggleChange(); });

            road.text = stoppingPoint.roadId;
            lane.text = stoppingPoint.laneId;
            type.text = stoppingPoint.type;
        }

        private void ToggleChange() {
            if (!toggle.isOn) {
                Parent.toggle.SetIsOnWithoutNotify(false);
                Parent.Parent.toggle.SetIsOnWithoutNotify(false);
            }

            _stoppingPoint.active = toggle.isOn;
            _stoppingPoint.Update();
        }
        
    }
}