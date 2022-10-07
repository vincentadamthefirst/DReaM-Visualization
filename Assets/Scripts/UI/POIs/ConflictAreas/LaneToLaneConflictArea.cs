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

        private ConflictArea _conflictArea;
        
        private bool _matchesCurrentSearch = false;

        public void InitializeData(ConflictArea conflictArea) {
            _conflictArea = conflictArea;

            currentLane.text = "" + conflictArea.laneIdA;
            otherLane.text = "" + conflictArea.laneIdB;
            colorImage.color = conflictArea.color;
        }

        public void ToggleChange() {
            if (!toggle.isOn) {
                Parent.toggle.SetIsOnWithoutNotify(false);
                Parent.Parent.toggle.SetIsOnWithoutNotify(false);
                Parent.Parent.Parent.toggle.SetIsOnWithoutNotify(false);
            }

            _conflictArea.obj.SetActive(toggle.isOn);
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