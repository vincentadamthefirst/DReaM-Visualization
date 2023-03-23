using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Visualization.POIs;

namespace UI.POIs.ConflictAreas {
    public class LaneToLaneConflictArea : MonoBehaviour {

        public TMP_Text currentLane;
        public TMP_Text otherLane;
        public Image colorImage;

        public Toggle toggle;
        
        public RoadBGroup Parent { get; set; }

        private ConflictAreaInfo _conflictAreaInfo;
        
        private bool _matchesCurrentSearch;

        public void InitializeData(ConflictAreaInfo conflictAreaInfo) {
            _conflictAreaInfo = conflictAreaInfo;

            currentLane.text = "" + conflictAreaInfo.laneIdA;
            otherLane.text = "" + conflictAreaInfo.laneIdB;
            colorImage.color = conflictAreaInfo.color;
        }

        public void ToggleChange() {
            if (!toggle.isOn) {
                Parent.toggle.SetIsOnWithoutNotify(false);
                Parent.Parent.toggle.SetIsOnWithoutNotify(false);
                Parent.Parent.Parent.toggle.SetIsOnWithoutNotify(false);
            }

            _conflictAreaInfo.conflictArea.MeshObject.SetActive(toggle.isOn);
        }

        public bool Match(string search) {
            _matchesCurrentSearch = currentLane.text == search || otherLane.text == search;
            return _matchesCurrentSearch;
        }

        public void ResetSearch() {
            _matchesCurrentSearch = false;
        }
    }
}