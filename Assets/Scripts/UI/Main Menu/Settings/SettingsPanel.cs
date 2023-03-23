using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace UI.Main_Menu.Settings {

    public class SettingsPanel : MonoBehaviour {

        [Header("Necessary Fields")] 
        public RectTransform scrollField;

        private readonly Dictionary<string, Setting> _settings = new();

        public void AddCheckBox(string settingName, string text, Reference<bool> reference) {
            var checkBoxPrefab = Resources.Load<CheckBox>("Prefabs/UI/Visualization/RuntimeMenu/Settings/SettingsCheckBox");
            var checkBox = Instantiate(checkBoxPrefab, scrollField);

            if (checkBox == null)
                throw new NullReferenceException("Something went terribly wrong when initializing the main menu.");

            checkBox.name = settingName;
            checkBox.Reference = reference;
            checkBox.SetInfo(text);
            checkBox.LoadData();

            _settings.Add(settingName, checkBox);
        }

        public void AddDropDown(string settingName, string text, string[] values, string defaultValue) {
            // TODO
        }

        public InputField AddInputField(string settingName, string text, string placeholder, Reference<string> reference) {
            var inputFieldPrefab = Resources.Load<InputField>("Prefabs/UI/Visualization/RuntimeMenu/Settings/SettingsInputField");
            var inputField = Instantiate(inputFieldPrefab, scrollField);

            if (inputField == null)
                throw new NullReferenceException("Something went terribly wrong when initializing the main menu.");

            inputField.name = settingName;
            inputField.Reference = reference;
            inputField.SetInfo(text, placeholder);
            inputField.LoadData();

            _settings.Add(settingName, inputField);
            return inputField;
        }
        
        public void AddRuler(int thickness) {
            var ruler = Resources.Load<Ruler>("Prefabs/UI/Visualization/RuntimeMenu/Settings/SettingsRuler");
            var newObject = Instantiate(ruler, scrollField);

            if (newObject == null)
                throw new NullReferenceException("Something went terribly wrong when initializing the main menu.");
            
            newObject.name = "ruler";
            newObject.SetThickness(thickness);
        }

        public void AddHeading(string settingName, string text) {
            var headingPrefab = Resources.Load<Heading>("Prefabs/UI/Visualization/RuntimeMenu/Settings/SettingsHeading");
            var heading = Instantiate(headingPrefab, scrollField);
            
            if (heading == null)
                throw new NullReferenceException("Something went terribly wrong when initializing the main menu.");
            
            heading.name = settingName;
            heading.SetInfo(text);
            _settings.Add(settingName, heading);
        }

        public void SaveSettings() {
            foreach (var (_, s) in _settings) {
                s.StoreData();
            }
        }

        public void LoadSettings() {
            foreach (var (_, s) in _settings) {
                s.LoadData();
            }
        }
    }
}