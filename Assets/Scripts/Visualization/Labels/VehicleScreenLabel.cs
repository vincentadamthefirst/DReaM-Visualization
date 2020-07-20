using System.Diagnostics.CodeAnalysis;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Visualization.Labels {
    public class VehicleScreenLabel : ScreenLabel {
        private TextMeshProUGUI _brake;

        private Image _indicatorLeft;
        private Image _indicatorRight;
        
        private readonly Color _brakeOffColor = new Color(.4f, 0, 0);
        private readonly Color _brakeOnColor = new Color(.8f, 0, 0);
        
        private readonly Color _indicatorOffColor = new Color(0, .4f, 0);
        private readonly Color _indicatorOnColor = new Color(0, .8f, 0);
        
        protected override void FindLabels() {
            base.FindLabels();

            _brake = LabelMainObject.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>();
            _brake.text = "Brake";
            _brake.color = _brakeOffColor;
            
            _indicatorLeft = LabelMainObject.GetChild(1).GetChild(2).GetComponent<Image>();
            _indicatorRight = LabelMainObject.GetChild(1).GetChild(4).GetComponent<Image>();

            _indicatorLeft.color = _indicatorOffColor;
            _indicatorRight.color = _indicatorOffColor;
        }
        
        // params[0] --> brake
        // params[1] --> indicator state left
        // params[2] --> indicator state right
        [SuppressMessage("ReSharper", "ConvertSwitchStatementToSwitchExpression")]
        public override void UpdateIntegers(params int[] parameters) {
            base.UpdateIntegers();
            
            switch (parameters[0]) {
                case 0:
                    _brake.color = _brakeOffColor;
                    break;
                default:
                    _brake.color = _brakeOnColor;
                    break;
            }

            switch (parameters[1]) {
                case 0:
                    _indicatorLeft.color = _indicatorOffColor;
                    break;
                default:
                    _indicatorLeft.color = _indicatorOnColor;
                    break;
            }

            switch (parameters[2]) {
                case 0:
                    _indicatorRight.color = _indicatorOffColor;
                    break;
                default:
                    _indicatorRight.color = _indicatorOnColor;
                    break;
            }
        }
    }
}