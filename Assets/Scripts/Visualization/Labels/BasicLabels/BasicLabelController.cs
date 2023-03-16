using System.Collections.Generic;
using UnityEngine;

namespace Visualization.Labels.BasicLabels {
    public class BasicLabelController : MonoBehaviour {

        private readonly List<TextLabel> _idLabels = new();

        private void Update() {
            if (Input.GetKeyDown(KeyCode.F2)) {
                _idLabels.ForEach(x => x.gameObject.SetActive(true));
            } else if (Input.GetKeyUp(KeyCode.F2)) {
                _idLabels.ForEach(x => x.gameObject.SetActive(false));
            }
        }

        public void AddLabel(TextLabel label) {
            _idLabels.Add(label);
        }
    }
}