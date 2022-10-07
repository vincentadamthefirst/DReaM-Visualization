using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

namespace UI.Main_Menu.Import {
    public class SimulationOutputImportEntry : ImportEntry {
        
        private TMP_Dropdown _dropdown;
        
        private void Awake() {
            toggle = transform.Find("Toggle").GetComponent<Toggle>();
            fileText = transform.Find("File").GetComponent<TextMeshProUGUI>();
            typeText = transform.Find("Type").GetComponent<TextMeshProUGUI>();
            _dropdown = transform.GetComponentInChildren<TMP_Dropdown>();

            background = transform.GetComponent<Image>();
        }

        public void SetRunIds(List<string> runIds) {
            _dropdown.ClearOptions();
            _dropdown.AddOptions(runIds);
        }

        public string CurrentlySelectedRunId => _dropdown.options[_dropdown.value].text;
        
    }
}