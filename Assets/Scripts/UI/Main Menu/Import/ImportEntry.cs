using Importer.XMLHandlers;
using TMPro;
using UI.Main_Menu.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Main_Menu.Import {

    public class ImportEntry : MonoBehaviour {
        public bool IsSelected => toggle.isOn;

        protected TextMeshProUGUI fileText;
        protected TextMeshProUGUI typeText;

        protected Toggle toggle;

        protected Image background;

        private static readonly string[] TypeNames = { "w.i.p", "ped", "veh", "scene", "out", "profiles", "dream" };
        
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
        
        public void SetType(XmlType type) {
            typeText.SetText(TypeNames[(int) type]);
        }

        public void SetIndex(int index) {
            if (index % 2 == 0) {
                background.color = background.color.WithAlpha(0);
            }
        }
    }
}