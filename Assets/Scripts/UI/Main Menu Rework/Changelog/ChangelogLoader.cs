using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Resolvers;
using TMPro;
using UI.Main_Menu_Rework.Elements;
using UI.Main_Menu_Rework.Logic;
using UI.Main_Menu_Rework.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Main_Menu_Rework.Changelog {
    public class ChangelogLoader : MonoBehaviour {

        [Tooltip("The current ApplicationDesign")]
        public ApplicationDesign design;
        
        [Header("Sprites for the entries")]
        public Sprite addSprite;
        public Sprite changeSprite;
        public Sprite removeSprite;

        [Header("Prefabs for the entries")]
        public Changelog changelogPrefab;
        public ChangelogEntry changelogEntryPrefab;
        public RectTransform separatorPrefab;
        public TextMeshProUGUI startTextPrefab;
        public CustomPadding paddingPrefab;
        
        [Header("Buttons to cycle the changelogs")]
        public Button left;
        public Button right;

        private readonly List<Changelog> _changelogs = new List<Changelog>();
        private int _active;

        private void Start() {
            left.onClick.AddListener(DecreaseActive);
            right.onClick.AddListener(IncreaseActive);
            
            LoadChangelog();
            UpdateActiveStatus();
        }

        private void DecreaseActive() {
            _active--;
            if (_active < 0) _active = 0;
            UpdateActiveStatus();
        }

        private void IncreaseActive() {
            _active++;
            if (_active > _changelogs.Count - 1) _active = _changelogs.Count - 1;
            UpdateActiveStatus();
        }

        private void UpdateActiveStatus() {
            for (var i = 0; i < _changelogs.Count; i++) {
                _changelogs[i].gameObject.SetActive(i == _active);
            }
        }

        private void LoadChangelog() {
            var textAsset = (TextAsset) Resources.Load("changelog");
            var tmp = new XmlDocument();
            tmp.LoadXml(textAsset.text);
            var xml = XDocument.Parse(tmp.OuterXml);

            const string locale = "en";
            if (xml.Root == null) return;
            
            var latestChanged = xml.Root.Element("InfoText");
            if (latestChanged != null) {
                FindObjectOfType<DashboardController>().UpdateChangesText(latestChanged.Attribute("version")?.Value ?? "",
                    string.Concat(latestChanged.Nodes()));
            }

            var changelogsForLocale =
                xml.Root.Elements("Changelog").First(x => (x.Attribute("locale")?.Value ?? "en") == locale);
            LoadChangelogs(changelogsForLocale);
        }

        private void LoadChangelogs(XElement cls) {
            var parent = transform.Find("Content");

            var first = true;
            foreach (var ver in cls.Elements("Version")) {
                var title = ver.Attribute("title")?.Value ?? "";
                var number = ver.Attribute("number")?.Value ?? "";
                var addon = ver.Attribute("addon")?.Value ?? "";

                var newChangeLog = Instantiate(changelogPrefab, parent);
                newChangeLog.name = "Version " + number + addon;
                newChangeLog.additional.text = first ? "(Newest)" : "";
                first = false;

                newChangeLog.title.text = title;
                newChangeLog.version.text = number + addon;
                
                _changelogs.Add(newChangeLog);
                
                foreach (var element in ver.Elements()) {
                    if (element.Name == "Text") {
                        // instantiation some text
                        var newText = Instantiate(startTextPrefab, newChangeLog.content);
                        newText.SetText(string.Concat(element.Nodes()).Replace("\\n", "\n").Replace("\\t", "\t"));
                    } else if (element.Name == "Divider") {
                        // instantiating a padding and divider line
                        var newPadding = Instantiate(paddingPrefab, newChangeLog.content);
                        newPadding.sizeY = int.Parse(element.Attribute("padding")?.Value ?? "0");
                        Instantiate(separatorPrefab, newChangeLog.content);
                    } else if (element.Name == "Changes") {
                        // instantiating a bunch of entries
                        foreach (var entry in element.Elements("Entry")) {
                            var newEntry = Instantiate(changelogEntryPrefab, newChangeLog.content);
                            newEntry.text.SetText(string.Concat(entry.Nodes()));
                            switch (element.Attribute("type")?.Value ?? "#") {
                                case "+":
                                    newEntry.icon.sprite = addSprite;
                                    newEntry.icon.GetComponent<CustomUiImage>().color = ApplicationColor.HighlightC;
                                    break;
                                case "#":
                                    newEntry.icon.sprite = changeSprite;
                                    newEntry.icon.GetComponent<CustomUiImage>().color = ApplicationColor.HighlightD;
                                    break;
                                case "-":
                                    newEntry.icon.sprite = removeSprite;
                                    newEntry.icon.GetComponent<CustomUiImage>().color = ApplicationColor.HighlightA;
                                    break;
                            }
                        }
                    }
                }
                
                LayoutRebuilder.ForceRebuildLayoutImmediate(newChangeLog.GetComponent<RectTransform>());
            }
        }
    }
}