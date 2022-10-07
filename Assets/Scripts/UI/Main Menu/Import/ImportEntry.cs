using TMPro;
using UI.Main_Menu.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Main_Menu.Import {

    public enum ImportType {
        PedestrianModels,
        VehicleModels,
        Scenery,
        Output,
        Profiles,
        DReaM,
        Unsupported,
    }
    
    public class ImportEntry : MonoBehaviour {
        public bool IsSelected => toggle.isOn;

        protected TextMeshProUGUI fileText;
        protected TextMeshProUGUI typeText;

        protected Toggle toggle;

        protected Image background;

        private static readonly string[] TypeNames = { "ped", "veh", "scene", "out", "profiles", "dream", "w.i.p." };
        
        private void Awake() {
            toggle = transform.Find("Toggle").GetComponent<Toggle>();
            fileText = transform.Find("File").GetComponent<TextMeshProUGUI>();
            typeText = transform.Find("Type").GetComponent<TextMeshProUGUI>();

            background = transform.GetComponent<Image>();
        }

        public void SetSelected() {
            toggle.SetIsOnWithoutNotify(true);
        }

        public void SetName(string newName) {
            fileText.SetText(newName);
        }
        
        public void SetType(ImportType type) {
            typeText.SetText(TypeNames[(int) type]);
        }

        public void SetIndex(int index) {
            if (index % 2 == 0) {
                background.color = background.color.WithAlpha(0);
            }
        }
    }
}