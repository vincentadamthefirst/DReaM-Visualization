using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.POIs.ConflictAreas {
    public class RoadBGroup : MonoBehaviour {

        public TMP_Text roadBText;

        public Toggle toggle;

        public List<LaneToLaneConflictArea> ConflictAreas { get; } = new();
        
        public RoadAGroup Parent { get; set; }

        private bool _matchesCurrentSearch;

        public void InitializeData(string otherRoad) {
            roadBText.text = otherRoad;
        }

        public void ToggleChange() {
            ConflictAreas.ForEach(x => x.toggle.isOn = toggle.isOn);
        }

        public bool Search(string search) {
            if (roadBText.text == search) {
                ActivateAll();
                _matchesCurrentSearch = true;
                return _matchesCurrentSearch;
            }

            foreach (var ca in ConflictAreas) {
                if (ca.Match(search))
                    _matchesCurrentSearch = true;
                else
                    ca.gameObject.SetActive(false);
            }

            return _matchesCurrentSearch;
        }
        
        public void ResetSearch() {
            foreach (var ca in ConflictAreas) {
                ca.ResetSearch();
                ca.gameObject.SetActive(true);
            }
            
            _matchesCurrentSearch = false;
        }

        public void ActivateAll() {
            foreach (var ca in ConflictAreas) {
                ca.gameObject.SetActive(true);
            }
        }
    }
}