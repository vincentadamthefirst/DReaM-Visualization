using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UI.Main_Menu_Rework.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Main_Menu_Rework.Elements {
    
    [RequireComponent(typeof(TMP_Dropdown))]
    public class CustomUiDropdown : CustomUiElement {

        public List<CustomUiDropdownElement> elements;

        public ApplicationColor mainDropdownColor;
        public ApplicationColor textColor;
        public ApplicationColor dropdownBackground;
        public TextStyle textStyle;

        // for the label
        private TextMeshProUGUI _label;
        private Image _labelImage;
        private Image _iconImage;
        
        // for the actual dropdown
        private Image _dropdownBackground;
        private Image _itemBackground;
        private TextMeshProUGUI _itemText;
        private Image _itemIcon;

        private TMP_Dropdown _dropdown;

        public override void UpdateUiElement() {
            FindAllComponents();

            var textCol = centralUiController.applicationDesign.GetColor(textColor);
            var textSty = centralUiController.applicationDesign.GetFont(textStyle);
            var mainCol = centralUiController.applicationDesign.GetColor(mainDropdownColor);

            _label.font = textSty;
            _label.color = textCol;
            _labelImage.color = mainCol;
            _iconImage.color = textCol;
            
            _dropdownBackground.color = centralUiController.applicationDesign.GetColor(dropdownBackground);
            _itemBackground.color = mainCol;
            _itemText.color = textCol;
            _itemText.font = textSty;
            _itemIcon.color = textCol;
            
            _dropdown.ClearOptions();
            var options = new List<TMP_Dropdown.OptionData>();
            elements.ForEach(x => options.Add(new TMP_Dropdown.OptionData {text = x.text}));
            _dropdown.AddOptions(options);
        }

        private void FindAllComponents() {
            _labelImage = GetComponent<Image>();
            _iconImage = transform.Find("Arrow").GetComponent<Image>();
            _label = transform.Find("Label").GetComponent<TextMeshProUGUI>();

            _dropdownBackground = transform.Find("Template").GetComponent<Image>();
            var item = transform.Find("Template").Find("Viewport").Find("Content").Find("Item");
            
            _itemBackground = item.Find("Item Background").GetComponent<Image>();
            _itemText = item.Find("Item Label").GetComponent<TextMeshProUGUI>();
            _itemIcon = item.Find("Item Checkmark").GetComponent<Image>();

            _dropdown = GetComponent<TMP_Dropdown>();
        }
    }

    /// <summary>
    /// Class to represent an option in the dropdown. The settings of this class will be injected into the dropdown.
    /// This is done to add localization support for dropdowns.
    /// </summary>
    [Serializable]
    public class CustomUiDropdownElement {
        // for now get the actual text
        public string text;
    }
}