using TMPro;
using UnityEngine.UI;
using Visualization.Agents;

namespace Visualization.Labels.Detail {
    public class LabelSensorEntry : LabelEntry {
        
        public Reference<string> Reference { get; set; }

        private TMP_Text _sensorText;
        private TMP_Text _sensorValueText;
        private Image _sensorColorImage;

        private Toggle _toggle;

        private void Awake() {
            _sensorText = transform.Find("Title").GetComponent<TMP_Text>();
            _sensorValueText = transform.Find("Value").GetComponent<TMP_Text>();
            _toggle = transform.Find("Toggle").GetComponent<Toggle>();
            _sensorColorImage = transform.Find("Color").GetComponent<Image>();
        }

        public void AddSensor(AgentSensor sensor) {
            _sensorText.SetText(sensor.SensorSetup.sensorName);
            _toggle.onValueChanged.AddListener(sensor.SetOn);
            _sensorColorImage.color = sensor.SensorSetup.color;
        }

        public override void TriggerUpdate() {
            _sensorValueText.text = Reference.Value;
        }
    }
}