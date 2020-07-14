using System.Linq;
using UnityEngine;
using Utils;

namespace Visualization.Agents {
    public class VehicleAgent : Agent {

        private Transform _wheelFrontRight, _wheelFrontLeft;
        private Transform _wheelRearRight, _wheelRearLeft;

        private float _wheelCircumference;

        private float _currentHdg;

        public override void Prepare() {
            base.Prepare();
            
            // TODO actually read these values
            
            // offsetting the agent model inside its parent
            Model.transform.GetChild(0).transform.localPosition -= new Vector3(1.285f, 0, 0); // TODO non static

            // scaling up the model
            Model.transform.GetChild(0).ScaleToValue(5.26f,  2.18f, 1.51f); // TODO non static
            
            // getting all wheel information
            _wheelFrontLeft = Model.transform.GetChild(0).GetChild(0);
            _wheelFrontRight = Model.transform.GetChild(0).GetChild(1);
            _wheelRearLeft = Model.transform.GetChild(0).GetChild(2);
            _wheelRearRight = Model.transform.GetChild(0).GetChild(3);

            _wheelCircumference = Mathf.PI * 0.68f;

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
           // OwnLabel.SetStrings(gameObject.name);
            //OwnLabel.SetFloats(1.51f + 0.5f); // TODO non static
        }

        protected override void UpdatePosition() {
            var nextPositionPointer = new Vector2(deltaS + 1.35f, 0);
            nextPositionPointer.RotateRadians(previous.Rotation);

            Model.transform.position = new Vector3(previous.Position.x + nextPositionPointer.x, 0,
                previous.Position.y + nextPositionPointer.y);
            
            UpdateWheelRotation();
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

        protected override void UpdateRotation() {
            _currentHdg = deltaTMs * ((previous.Next.Rotation - previous.Rotation) / 100) + previous.Rotation;
            Model.transform.rotation = Quaternion.Euler(0, -_currentHdg * Mathf.Rad2Deg, 0);
        }

        protected override void UpdateLabel() {
            var modelPosition = Model.transform.position;
            OwnLabel.UpdateFloats(modelPosition.x, modelPosition.z, previous.Velocity, previous.Acceleration);
            OwnLabel.transform.position = modelPosition + new Vector3(0, 1.51f + 1.4f, 0); // TODO non static values

            // TODO update strings
        }
    }
}