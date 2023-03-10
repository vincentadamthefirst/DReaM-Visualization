using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.POIs.ConflictAreas {
    public class RoadAGroup : MonoBehaviour {

        public TMP_Text roadAText;
        
        public Toggle toggle;

        public List<RoadBGroup> RoadBGroups { get; } = new List<RoadBGroup>();

        public IntersectionGroup Parent { get; set; }
        
        private bool _matchesCurrentSearch = false;

        public void InitializeData(string currentRoad) {
            roadAText.text = currentRoad;
        }

        public void ToggleChange() {
            RoadBGroups.ForEach(x => x.toggle.isOn = toggle.isOn);
        }

        public bool Search(string search) {
            if (roadAText.text == search) {
                foreach (var org in RoadBGroups) {
                    org.ActivateAll();
                }
                _matchesCurrentSearch = true;
                return _matchesCurrentSearch;
            }

            foreach (var org in RoadBGroups) {
                if (org.Search(search))
                    _matchesCurrentSearch = true;
                else 
                    org.gameObject.SetActive(false);
            }

            return _matchesCurrentSearch;
        }

        public void ResetSearch() {
            foreach (var rbg in RoadBGroups) {
                rbg.ResetSearch();
                rbg.gameObject.SetActive(true);
            }

            _matchesCurrentSearch = false;
        }

        public void ActivateAll() {
            foreach (var rbg in RoadBGroups) {
                rbg.ActivateAll();
                rbg.gameObject.SetActive(true);
            }
        }
    }
}