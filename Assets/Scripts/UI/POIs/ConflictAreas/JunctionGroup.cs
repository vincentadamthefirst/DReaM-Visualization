using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.POIs.ConflictAreas {
    public class JunctionGroup : MonoBehaviour {
        public TMP_Text junctionText;
        
        public Toggle toggle;

        public List<RoadAGroup> RoadAGroups { get; } = new List<RoadAGroup>();
        
        private bool _matchesCurrentSearch = false;

        public void InitializeData(string junction) {
            junctionText.text = junction;
        }

        public void ToggleChange() {
            RoadAGroups.ForEach(x => x.toggle.isOn = toggle.isOn);
        }

        public bool Search(string search) {
            if (junctionText.text == search) {
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