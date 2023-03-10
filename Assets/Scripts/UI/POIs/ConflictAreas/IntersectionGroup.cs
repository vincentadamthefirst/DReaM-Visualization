using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI.POIs.ConflictAreas {
    public class IntersectionGroup : MonoBehaviour {
        public TMP_Text intersectionText;
        
        public Toggle toggle;

        public List<RoadAGroup> RoadAGroups { get; } = new();
        
        private bool _matchesCurrentSearch;

        public void InitializeData(string intersection) {
            intersectionText.text = intersection;
        }

        public void ToggleChange() {
            RoadAGroups.ForEach(x => x.toggle.isOn = toggle.isOn);
        }

        public bool Search(string search) {
            if (intersectionText.text == search) {
                foreach (var rag in RoadAGroups) {
                    rag.ActivateAll();
                }
                _matchesCurrentSearch = true;
                return _matchesCurrentSearch;
            }

            foreach (var rag in RoadAGroups) {
                if (rag.Search(search))
                    _matchesCurrentSearch = true;
                else 
                    rag.gameObject.SetActive(false);
            }

            return _matchesCurrentSearch;
        }

        public void ResetSearch() {
            foreach (var rag in RoadAGroups) {
                rag.ResetSearch();
                rag.gameObject.SetActive(true);
            }

            _matchesCurrentSearch = false;
        }
    }
}