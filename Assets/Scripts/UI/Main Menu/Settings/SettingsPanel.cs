using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace UI.Main_Menu.Settings {

    public class SettingsPanel : MonoBehaviour {

        [Header("Necessary Fields")] 
        public RectTransform scrollField;
        public int spacing;
        public int startSpacing;

        private readonly Dictionary<string, Setting> _settings = new();

        public void AddCheckBox(string settingName, string text, bool defaultValue = false, string parent = null) {
            var checkBox = Resources.Load<CheckBox>("Prefabs/UI/Visualization/RuntimeMenu/Settings/SettingsCheckBox");
            var newObject = Instantiate(checkBox, scrollField);

            if (newObject == null)
                throw new NullReferenceException("Something went terribly wrong when initializing the main menu.");

            newObject.name = settingName;
            newObject.SetData(text, defaultValue);

            // if (parent != null) {
            //     if (!_settings.ContainsKey(parent))
            //         throw new NullReferenceException("Parent not found");
            //     var parentSetting = _settings[parent];
            //     newObject.SetSpacing(parentSetting.GetSpacing() + spacing);
            // } else 
            //     newObject.SetSpacing(startSpacing);

            _settings.Add(settingName, newObject);
        }

        public void AddDropDown(string settingName, string text, string[] values, string defaultValue, string parent = null) {
            // TODO
        }

        public InputField AddInputField(string settingName, string text, string placeholder, string defaultValue,
            string parent = null) {
            
            var inputField = Resources.Load<InputField>("Prefabs/UI/Visualization/RuntimeMenu/Settings/SettingsInputField");
            var newObject = Instantiate(inputField, scrollField);

            if (newObject == null)
                throw new NullReferenceException("Something went terribly wrong when initializing the main menu.");

            newObject.name = settingName;
            newObject.SetData(text, placeholder, defaultValue);

            // if (parent != null) {
            //     if (!_settings.ContainsKey(parent))
            //         throw new NullReferenceException("Parent not found");
            //     var parentSetting = _settings[parent];
            //     newObject.SetSpacing(parentSetting.GetSpacing() + spacing);
            // } else 
            //     newObject.SetSpacing(startSpacing);

            _settings.Add(settingName, newObject);
            return newObject;
        }
        
        public void AddRuler(int thickness) {
            var ruler = Resources.Load<Ruler>("Prefabs/UI/Visualization/RuntimeMenu/Settings/SettingsRuler");
            var newObject = Instantiate(ruler, scrollField);

            if (newObject == null)
                throw new NullReferenceException("Something went terribly wrong when initializing the main menu.");
            
            newObject.SetData(thickness);
            newObject.name = "ruler";
        }

        public void AddHeading(string settingName, string text, string parent = null) {
            var heading = Resources.Load<Heading>("Prefabs/UI/Visualization/RuntimeMenu/Settings/SettingsHeading");
            var newObject = Instantiate(heading, scrollField);
            
            if (newObject == null)
                throw new NullReferenceException("Something went terribly wrong when initializing the main menu.");
            
            // if (parent != null) {
            //     if (!_settings.ContainsKey(parent))
            //         throw new NullReferenceException("Parent not found");
            //     var parentSetting = _settings[parent];
            //     newObject.SetSpacing(parentSetting.GetSpacing() + spacing);
            // } else 
            //     newObject.SetSpacing(startSpacing);
            
            newObject.SetData(text);
            newObject.name = settingName;
            
            _settings.Add(settingName, newObject);
        }

        public bool GetBool(string settingName) {
            var setting = _settings[settingName];
            if (setting is CheckBox)
                return ((CheckBox)_settings[settingName]).IsOn();
            
            throw new ArgumentException("CheckBox does not exist.");
        }

        public int GetInt(string settingName) {
            var setting = _settings[settingName];
            if (setting is InputField)
                return int.Parse(((InputField)_settings[settingName]).GetValue());
            
            throw new ArgumentException("InputField does not exist.");
        }
        
        public float GetFloat(string settingName) {
            var setting = _settings[settingName];
            if (setting is InputField)
                return float.Parse(((InputField)_settings[settingName]).GetValue(), CultureInfo.InvariantCulture);
            
            throw new ArgumentException("InputField does not exist.");
        }

        public string GetString(string settingName) {
            var setting = _settings[settingName];
            if (setting is InputField)
                return ((InputField)_settings[settingName]).GetValue();
            if (setting is DropDown)
                return "TODO"; // TODO
            
            throw new ArgumentException("Setting does not exist.");
        }

        public void SaveSettings() {
            foreach (var entry in _settings) {
                var settingName = entry.Key;
                var setting = entry.Value;

                switch (setting) {
                    case CheckBox checkBox:
                        PlayerPrefs.SetInt(settingName, checkBox.IsOn() ? 1 : 0);
                        break;
                    case InputField inputField:
                        PlayerPrefs.SetString(settingName, inputField.GetValue());
                        break;
                    case DropDown _:
                        // TODO
                        break;
                }
            }
        }

        public void LoadSettings() {
            foreach (var entry in _settings) {
                var settingName = entry.Key;
                var setting = entry.Value;

                switch (setting) {
                    case CheckBox checkBox:
                        checkBox.SetValue(PlayerPrefs.GetInt(settingName) > 0);
                        break;
                    case InputField inputField:
                        inputField.SetValue(PlayerPrefs.GetString(settingName));
                        break;
                    case DropDown _:
                        // TODO
                        break;
                }
            }
        }
    }
}