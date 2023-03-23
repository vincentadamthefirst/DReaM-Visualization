using TMPro;
using Utils;

namespace Visualization.Labels.Detail {
    public class LabelTextEntry : LabelEntry {

        public Reference<string> Reference { get; set; }

        private TMP_Text _title;
        private TMP_Text _value;

        private void Awake() {
            _title = transform.Find("Title").GetComponent<TMP_Text>();
            _value = transform.Find("Value").GetComponent<TMP_Text>();
        }

        public void Initialize(string title) {
            _title.text = title;
        }

        public override void TriggerUpdate() {
            _value.text = Reference.Value;
        }
    }
}