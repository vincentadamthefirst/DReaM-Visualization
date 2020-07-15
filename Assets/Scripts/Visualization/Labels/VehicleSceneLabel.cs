using TMPro;
using UnityEngine;

namespace Visualization.Labels {
    public class VehicleSceneLabel : SceneLabel {

        private TextMeshPro _brake;
        
        private readonly Color _brakeOffColor = new Color(.4f, 0, 0);
        private readonly Color _brakeOnColor = new Color(.8f, 0, 0);
        
        private readonly Color _indicatorOffColor = new Color(0, .4f, 0);
        private readonly Color _indicatorOnColor = new Color(0, .8f, 0);
        
        protected override void FindLabels() {
            base.FindLabels();

            _brake = labelMainObject.GetChild(1).GetChild(2).GetChild(0).GetComponent<TextMeshPro>();
            _brake.text = "Brake";
            _brake.color = _brakeOffColor;
        }

        public override void UpdateFloats(params float[] parameters) {
            base.UpdateFloats(parameters);
        }

        public override void UpdateStrings(params string[] parameters) {
            base.UpdateStrings(parameters);
        }

        public override void UpdateIntegers(params int[] parameters) {
            base.UpdateIntegers();
            
            // ReSharper disable once ConvertSwitchStatementToSwitchExpression
            switch (parameters[0]) {
                case 0:
                    _brake.color = _brakeOffColor;
                    break;
                default:
                    _brake.color = _brakeOnColor;
                    break;
            }
        }
    }
}