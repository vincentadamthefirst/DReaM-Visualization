using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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

        private Transform _chassis;
        
        private Vector3 _centerHeightOffset; // TODO maybe remove

        public override void Prepare() { 
            // coloring the chassis
            _modelMeshRenderer = Model.transform.GetChild(0).GetComponent<MeshRenderer>();
            _modelMeshRenderer.material = ColorMaterial;
            
            base.Prepare();

            // scaling up the model
            _chassis = Model.transform.Find("chassis");

            // offsetting the agent model inside its parent
            _chassis.transform.localPosition += ModelInformation.Center;

            _chassis.SetTotalSize(ModelInformation.Width, ModelInformation.Height, ModelInformation.Length);

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
            OwnLabel.SetStrings(gameObject.name.Split(new [] {" ["}, StringSplitOptions.None)[0]);
            OwnLabel.SetFloats(ModelInformation.Height + 1.4f);
            OwnLabel.SetColors(ColorMaterial.color);

            boundingBox = new Bounds(new Vector3(0, ModelInformation.Height / 2f, 0),
                new Vector3(ModelInformation.Width, ModelInformation.Height, ModelInformation.Length));
            
            _centerHeightOffset = new Vector3(0, ModelInformation.Height / 2f, 0);
        }

        protected override void UpdatePosition() {
            var nextPositionPointer = new Vector2(deltaS, 0);
            nextPositionPointer.RotateRadians(previous.Rotation);

            Model.transform.position = new Vector3(previous.Position.x + nextPositionPointer.x, 0,
                previous.Position.y + nextPositionPointer.y);
            
            UpdateWheelRotation();
            UpdateIndicators();
            UpdateBrakes();
            
            boundingBox.center = Model.transform.GetChild(1).position + _centerHeightOffset;

            // for (var i = 0; i < AgentSensors.Count; i++) {
            //     AgentSensors[i]
            //         .UpdatePositionAndRotation(Model.transform.position, previous.SensorInformation[i].Heading);
            //     
            //     if (AgentSensors[i].IsStatic) continue;
            // }
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

            CurrentRotation = _currentHdg;
        }

        public override Vector3 GetAnchorPoint() {
            var position = Model.transform.position;
            return new Vector3(position.x, ModelInformation.Height, position.z);
        }

        protected override void UpdateLabel() {
            //if (Time.frameCount % 10 != 0) return; // reduce the load by only updating every 10 frames
            
            var avi = previous.AdditionalInformation as AdditionalVehicleInformation;

            var modelPosition = Model.transform.position;
            OwnLabel.UpdateFloats(modelPosition.x, modelPosition.z, previous.Velocity, previous.Acceleration);
            OwnLabel.UpdateStrings(avi.CrossingPhase, avi.ScanAoI, avi.GlanceType);
            OwnLabel.UpdatePositions(avi.OtherAgents);
            OwnLabel.UpdateIntegers(avi.Brake ? 1 : 0,
                avi.IndicatorState == IndicatorState.Left || avi.IndicatorState == IndicatorState.Warn ? 1 : 0,
                avi.IndicatorState == IndicatorState.Right || avi.IndicatorState == IndicatorState.Warn ? 1 : 0);
        }

        protected override Vector3[] GetReferencePointsRenderer() {
            var points = new List<Vector3>();
            
            if (renderers.Length == 0) return points.ToArray();
            
            var completeBounds = new Bounds();
            foreach (var r in renderers) {
                completeBounds.Encapsulate(r.bounds);
            }

            var ext = completeBounds.extents;
            var tr = Model.transform.localToWorldMatrix;
                
            // adding global points to the list, first the lower 4 points of the box, then the upper 4 points,
            // ordered counter clockwise

            points.Add(tr.MultiplyPoint3x4(new Vector3(-ext.x / 2f, -ext.y / 2f, ext.z / 2f)));
            points.Add(tr.MultiplyPoint3x4(new Vector3(ext.x / 2f, -ext.y / 2f, ext.z / 2f)));
            points.Add(tr.MultiplyPoint3x4(new Vector3(ext.x / 2f, -ext.y / 2f, -ext.z / 2f)));
            points.Add(tr.MultiplyPoint3x4(new Vector3(-ext.x / 2f, -ext.y / 2f, -ext.z / 2f)));
                
            points.Add(tr.MultiplyPoint3x4(new Vector3(-ext.x / 2f, ext.y / 2f, ext.z / 2f)));
            points.Add(tr.MultiplyPoint3x4(new Vector3(ext.x / 2f, ext.y / 2f, ext.z / 2f)));
            points.Add(tr.MultiplyPoint3x4(new Vector3(ext.x / 2f, ext.y / 2f, -ext.z / 2f)));
            points.Add(tr.MultiplyPoint3x4(new Vector3(-ext.x / 2f, ext.y / 2f, -ext.z / 2f)));
            
            return points.ToArray();
        }

        private void OnDrawGizmos() {
            Gizmos.color = ColorMaterial.color;
            foreach (var point in GetReferencePointsCustom()) {
                Gizmos.DrawSphere(point, .1f);
            }
        }

        protected override Vector3[] GetReferencePointsCustom() {
            var toReturn = new Vector3[customPoints.customPoints.Count];
            var tr2 = Model.transform.localToWorldMatrix;
            for (var i = 0; i < toReturn.Length; i++) {
                var tmp = customPoints.customPoints[i];
                toReturn[i] =
                    tr2.MultiplyPoint3x4(
                        new Vector3(tmp.x * _chassis.lossyScale.x, tmp.y * _chassis.lossyScale.y,
                            tmp.z * _chassis.lossyScale.z) + ModelInformation.Center);

            }

            return toReturn;
        }
    }
}