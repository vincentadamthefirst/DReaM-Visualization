using TMPro;
using UnityEngine;

namespace Visualization.Labels {
    public class VehicleScreenLabel : ScreenLabel {
        private TextMeshProUGUI _brake;
        
        private readonly Color _brakeOffColor = new Color(.4f, 0, 0);
        private readonly Color _brakeOnColor = new Color(.8f, 0, 0);
        
        private readonly Color _indicatorOffColor = new Color(0, .4f, 0);
        private readonly Color _indicatorOnColor = new Color(0, .8f, 0);
        
        protected override void FindLabels() {
            base.FindLabels();

            _brake = LabelMainObject.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>();
            _brake.text = "Brake";
            _brake.color = _brakeOffColor;
        }
        
        // params[0] --> brake
        // params[1] --> indicator state left
        // params[2] --> indicator state right
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
            
            switch (parameters[1]) {
                // TODO
            }

            switch (parameters[2]) {
                // TODO
            }
        }
    }
}