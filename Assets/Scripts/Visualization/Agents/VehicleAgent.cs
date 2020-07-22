using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Utils;

namespace Visualization.Agents {
    public class VehicleAgent : Agent {

        public Material indicatorOffMaterial;
        public Material indicatorOnMaterial;

        public Material brakeOffMaterial;
        public Material brakeOnMaterial;

        private Transform _wheelFrontRight, _wheelFrontLeft;
        private Transform _wheelRearRight, _wheelRearLeft;

        private MeshRenderer _modelMeshRenderer;
        
        private float _wheelCircumference;

        private float _currentHdg;

        private int _indicatorTimer;
        private bool _wasIndicatorOn;

        public override void Prepare() {
            base.Prepare();
            
            // coloring the chassis
            _modelMeshRenderer = Model.transform.GetChild(0).GetComponent<MeshRenderer>();
            _modelMeshRenderer.material = ColorMaterial;
            
            // offsetting the agent model inside its parent
            Model.transform.GetChild(0).transform.localPosition -=
                new Vector3(ModelInformation.Center.x, 0, ModelInformation.Center.y);

            // scaling up the model
            var chassis = Model.transform.GetChild(0);
            //chassis.SetSize(ModelInformation.Length, ModelInformation.Height, ModelInformation.Width);
            
            chassis.SetTotalSize(ModelInformation.Width, ModelInformation.Height, ModelInformation.Length);

            // getting all wheel information
            _wheelFrontLeft = Model.transform.GetChild(1).GetChild(0);
            _wheelFrontRight = Model.transform.GetChild(1).GetChild(1);
            _wheelRearLeft = Model.transform.GetChild(1).GetChild(2);
            _wheelRearRight = Model.transform.GetChild(1).GetChild(3);

            var diameter = ((VehicleModelInformation) ModelInformation).WheelDiameter;
            
            _wheelFrontLeft.SetTotalSize(diameter, .3f, diameter);
            _wheelFrontRight.SetTotalSize(diameter, .3f, diameter);
            _wheelRearLeft.SetTotalSize(diameter, .3f, diameter);
            _wheelRearRight.SetTotalSize(diameter, .3f, diameter);
            
            _wheelCircumference = Mathf.PI * diameter;

            // preparing the wheel rotations for each sample
            var stepValues = SimulationSteps.Values.ToList();
            for (var i = 1; i < stepValues.Count; i++) {
                var prevInfo = stepValues[i - 1].AdditionalInformation as AdditionalVehicleInformation;
                var currInfo = stepValues[i].AdditionalInformation as AdditionalVehicleInformation;

                if (prevInfo == null || currInfo == null) return;
                
                var dist = Vector2.Distance(stepValues[i - 1].Position, stepValues[i].Position);
                var rot = prevInfo.WheelRotation + ((dist / _wheelCircumference) * 360f) / 4f;
                currInfo.WheelRotation = rot % 360;
            }
            
            // preparing the label
            OwnLabel.SetStrings(gameObject.name);
            OwnLabel.SetFloats(ModelInformation.Height + 1.4f);
        }

        protected override void UpdatePosition() {
            var nextPositionPointer = new Vector2(deltaS, 0);
            nextPositionPointer.RotateRadians(previous.Rotation);

            Model.transform.position = new Vector3(previous.Position.x + nextPositionPointer.x, 0,
                previous.Position.y + nextPositionPointer.y);
            
            UpdateWheelRotation();
            UpdateIndicators();
            UpdateBrakes();
        }

        private void UpdateWheelRotation() {
            var rot = (deltaS / _wheelCircumference * 360f) / 4f;
            rot += ((AdditionalVehicleInformation) previous.AdditionalInformation).WheelRotation;
            rot %= 360f;

            _wheelFrontLeft.localRotation = Quaternion.Euler(0, 180, 90);
            _wheelFrontRight.localRotation = Quaternion.Euler(0, 0, 90);
            _wheelRearLeft.localRotation = Quaternion.Euler(0, 180, 90);
            _wheelRearRight.localRotation = Quaternion.Euler(0, 0, 90);
            
            _wheelFrontLeft.Rotate(Vector3.up, -rot);
            _wheelFrontRight.Rotate(Vector3.up, rot);
            _wheelRearLeft.Rotate(Vector3.up, -rot);
            _wheelRearRight.Rotate(Vector3.up, rot);
        }

        private void UpdateBrakes() {
            var material = _modelMeshRenderer.materials;
            material[4] = ((AdditionalVehicleInformation) previous.AdditionalInformation).Brake
                ? brakeOnMaterial
                : brakeOffMaterial;
            _modelMeshRenderer.materials = material;
        }
        
        /// <summary>
        /// Updates the information on this agents indicators
        /// </summary>
        private void UpdateIndicators() {
            _indicatorTimer += deltaTMs;
            if (_indicatorTimer < 1500) return;
            ChangeIndicatorLightMaterial();

            _indicatorTimer -= 1500;
            _wasIndicatorOn = !_wasIndicatorOn;
        }

        private void ChangeIndicatorLightMaterial() {
            var to = _wasIndicatorOn ? indicatorOffMaterial : indicatorOnMaterial;
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            var materials = _modelMeshRenderer.materials;
            switch (((AdditionalVehicleInformation) previous.AdditionalInformation).IndicatorState) {
                case IndicatorState.None:
                    materials[2] = indicatorOffMaterial;
                    materials[7] = indicatorOffMaterial;
                    break;
                case IndicatorState.Left:
                    materials[2] = indicatorOffMaterial;
                    materials[7] = to;
                    break;
                case IndicatorState.Right:
                    materials[2] = to;
                    materials[7] = indicatorOffMaterial;
                    break;
                case IndicatorState.Warn:
                    materials[2] = to;
                    materials[7] = to;
                    break;
            }

            _modelMeshRenderer.materials = materials;
        }

        protected override void UpdateRotation() {
            _currentHdg = deltaTMs * ((previous.Next.Rotation - previous.Rotation) / 100) + previous.Rotation;
            Model.transform.rotation = Quaternion.Euler(0, -_currentHdg * Mathf.Rad2Deg, 0);
        }

        public override Vector3 GetAnchorPoint() {
            var position = Model.transform.position;
            return new Vector3(position.x, ModelInformation.Height, position.z);
        }

        protected override void UpdateLabel() {
            var avi = previous.AdditionalInformation as AdditionalVehicleInformation;

            var modelPosition = Model.transform.position;
            OwnLabel.UpdateFloats(modelPosition.x, modelPosition.z, previous.Velocity, previous.Acceleration);
            OwnLabel.UpdateStrings(avi.CrossingPhase, avi.ScanAoI, avi.GlanceType);
            OwnLabel.UpdatePositions(avi.OtherAgents);
            OwnLabel.UpdateIntegers(avi.Brake ? 1 : 0,
                avi.IndicatorState == IndicatorState.Left || avi.IndicatorState == IndicatorState.Warn ? 1 : 0,
                avi.IndicatorState == IndicatorState.Right || avi.IndicatorState == IndicatorState.Warn ? 1 : 0);
        }

        public override void HandleHit() {
            if (isTarget) return; // targets will not be handled on occlusion
        }

        public override void HandleNonHit() {
            // TODO implement
        }
    }
}