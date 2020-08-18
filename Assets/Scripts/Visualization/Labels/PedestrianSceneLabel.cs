using System.Diagnostics.CodeAnalysis;
using TMPro;
using UnityEngine;

namespace Visualization.Labels {
    public class PedestrianSceneLabel : SceneLabel {
        private TextMeshProUGUI _stopping;

        private readonly Color _stoppingOffColor = new Color(.4f, 0, 0);
        private readonly Color _stoppingOnColor = new Color(.8f, 0, 0);

        public override void FindLabels() {
            base.FindLabels();

            _stopping = LabelMainObject.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>();
            _stopping.text = "Stopping";
            _stopping.color = _stoppingOffColor;
        }
        
        // params[0] --> brake
        // params[1] --> indicator state left
        // params[2] --> indicator state right
        [SuppressMessage("ReSharper", "ConvertSwitchStatementToSwitchExpression")]
        public override void UpdateIntegers(params int[] parameters) {
            base.UpdateIntegers();
            
            switch (parameters[0]) {
                case 0:
                    _stopping.color = _stoppingOffColor;
                    break;
                default:
                    _stopping.color = _stoppingOnColor;
                    break;
            }
        }
    }
}