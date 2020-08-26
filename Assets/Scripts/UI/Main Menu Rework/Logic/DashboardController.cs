using TMPro;
using UnityEngine;

namespace UI.Main_Menu_Rework.Logic {
    public class DashboardController : MonoBehaviour {
        
        public void UpdateChangesText(string version, string text) {
            // TODO add locale
            transform.Find("Cards").Find("ChangelogCard").Find("Content").Find("Description")
                .GetComponent<TextMeshProUGUI>().SetText(text);
            transform.Find("Cards").Find("ChangelogCard").Find("Title").Find("Text").GetComponent<TextMeshProUGUI>()
                .SetText("Version " + version);
        }
    }
}