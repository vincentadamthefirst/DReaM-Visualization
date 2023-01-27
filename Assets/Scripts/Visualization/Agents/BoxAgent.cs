using System;
using System.Collections.Generic;
using System.Linq;
using Scenery;
using UnityEngine;
using Utils;
using Visualization.SimulationEvents;

namespace Visualization.Agents {
    public class BoxAgent : Agent {
        private MeshRenderer _modelMeshRenderer;

        private float _currentHdg;

        private int _indicatorTimer;
        private bool _wasIndicatorOn;

        private Transform _chassis;

        public override void Prepare() {
            // coloring the chassis
            _modelMeshRenderer = StaticData.Model.transform.GetChild(0).GetComponent<MeshRenderer>();
            _modelMeshRenderer.material = StaticData.ColorMaterial;

            base.Prepare();

            // scaling up the model
            _chassis = StaticData.Model.transform.Find("chassis");

            // offsetting the agent model inside its parent
            _chassis.transform.localPosition += StaticData.ModelInformation.Center;

            _chassis.SetTotalSize(StaticData.ModelInformation.Width, StaticData.ModelInformation.Height,
                StaticData.ModelInformation.Length);

            // preparing the wheel rotations for each sample
            var stepValues = SimulationSteps.Values.ToList();
            for (var i = 1; i < stepValues.Count; i++) {
                var prevInfo = stepValues[i - 1].AdditionalInformation as AdditionalVehicleInformation;
                var currInfo = stepValues[i].AdditionalInformation as AdditionalVehicleInformation;

                if (prevInfo == null || currInfo == null) return;

                currInfo.AEBActive = prevInfo.AEBActive;
                var aebActiveEvent = stepValues[i].Events.Find(x => x.EventType == SimulationEventType.AEBActive);
                var aebInactiveEvent = stepValues[i].Events.Find(x => x.EventType == SimulationEventType.AEBInactive);
                if (aebActiveEvent != null) {
                    currInfo.AEBActive = true;
                } else if (aebInactiveEvent != null) {
                    currInfo.AEBActive = false;
                }
            }

            // preparing the label
            // if (OwnLabel != null) {
            //     OwnLabel.SetStrings(gameObject.name.Split(new[] { " [" }, StringSplitOptions.None)[0]);
            //     OwnLabel.SetFloats(ModelInformation.Height + 1.4f);
            //     OwnLabel.SetColors(ColorMaterial.color);
            // }

            try {
                var idLabel = StaticData.Model.transform.Find("IdLabel");
                idLabel.transform.localPosition = new Vector3(0, StaticData.ModelInformation.Height + .5f, 0);
            }
            catch (Exception) {
                // ignored
            }

            boundingBox = new Bounds(new Vector3(0, StaticData.ModelInformation.Height / 2f, 0),
                new Vector3(StaticData.ModelInformation.Width, StaticData.ModelInformation.Height,
                    StaticData.ModelInformation.Length));
        }

        protected override void UpdatePosition() {
            var nextPositionPointer = new Vector2(deltaS, 0);
            nextPositionPointer.RotateRadians(DynamicData.ActiveSimulationStep.Rotation);

            StaticData.Model.transform.position = new Vector3(DynamicData.ActiveSimulationStep.Position.x + nextPositionPointer.x, 0,
                DynamicData.ActiveSimulationStep.Position.y + nextPositionPointer.y);

            DynamicData.Position3D = StaticData.Model.transform.GetChild(0).position;
            boundingBox.center = DynamicData.Position3D;
        }

        protected override void UpdateRotation() {
            _currentHdg = deltaTMs * ((DynamicData.ActiveSimulationStep.Next.Rotation - DynamicData.ActiveSimulationStep.Rotation) / 100) + DynamicData.ActiveSimulationStep.Rotation;
             StaticData.Model.transform.rotation = Quaternion.Euler(0, -_currentHdg * Mathf.Rad2Deg, 0);

             DynamicData.Rotation = _currentHdg;
        }

        // protected override void UpdateLabel() {
        //     if (OwnLabel == null)
        //         return;
        //     
        //     var avi = previous.AdditionalInformation as AdditionalVehicleInformation;
        //
        //     var modelPosition = Model.transform.position;
        //     OwnLabel.UpdateFloats(modelPosition.x, modelPosition.z, previous.Velocity, previous.Acceleration);
        //     OwnLabel.UpdateStrings(avi.CrossingPhase, avi.ScanAoI, avi.GlanceType);
        //     OwnLabel.UpdatePositions(avi.OtherAgents);
        //     OwnLabel.UpdateIntegers(avi.Brake ? 1 : 0,
        //         avi.IndicatorState == IndicatorState.Left || avi.IndicatorState == IndicatorState.Warn ? 1 : 0,
        //         avi.IndicatorState == IndicatorState.Right || avi.IndicatorState == IndicatorState.Warn ? 1 : 0,
        //         avi.AEBActive ? 1 : 0);
        // }

        protected override Vector3[] GetReferencePointsRenderer() {
            return new[] { transform.position }; // TODO

            // var points = new List<Vector3>();
            //
            // if (renderers.Length == 0) return points.ToArray();
            //
            // var completeBounds = new Bounds();
            // foreach (var r in renderers) {
            //     completeBounds.Encapsulate(r.bounds);
            // }
            //
            // var ext = completeBounds.extents;
            // var tr = Model.transform.localToWorldMatrix;
            //     
            // // adding global points to the list, first the lower 4 points of the box, then the upper 4 points,
            // // ordered counter clockwise
            //
            // points.Add(tr.MultiplyPoint3x4(new Vector3(-ext.x / 2f, -ext.y / 2f, ext.z / 2f)));
            // points.Add(tr.MultiplyPoint3x4(new Vector3(ext.x / 2f, -ext.y / 2f, ext.z / 2f)));
            // points.Add(tr.MultiplyPoint3x4(new Vector3(ext.x / 2f, -ext.y / 2f, -ext.z / 2f)));
            // points.Add(tr.MultiplyPoint3x4(new Vector3(-ext.x / 2f, -ext.y / 2f, -ext.z / 2f)));
            //     
            // points.Add(tr.MultiplyPoint3x4(new Vector3(-ext.x / 2f, ext.y / 2f, ext.z / 2f)));
            // points.Add(tr.MultiplyPoint3x4(new Vector3(ext.x / 2f, ext.y / 2f, ext.z / 2f)));
            // points.Add(tr.MultiplyPoint3x4(new Vector3(ext.x / 2f, ext.y / 2f, -ext.z / 2f)));
            // points.Add(tr.MultiplyPoint3x4(new Vector3(-ext.x / 2f, ext.y / 2f, -ext.z / 2f)));
            //
            // return points.ToArray();
        }

        protected override Vector3[] GetReferencePointsCustom() {
            return new[] { transform.position }; // TODO

            // if (customPoints == null || customPoints.customPoints == null) {
            //     Debug.Log($"Uh-Oh: Agent {Id}");
            // }
            //
            // var toReturn = new Vector3[customPoints.customPoints.Count];
            // var tr2 = Model.transform.localToWorldMatrix;
            // for (var i = 0; i < toReturn.Length; i++) {
            //     var tmp = customPoints.customPoints[i];
            //     toReturn[i] =
            //         tr2.MultiplyPoint3x4(
            //             new Vector3(tmp.x * _chassis.lossyScale.x, tmp.y * _chassis.lossyScale.y,
            //                 tmp.z * _chassis.lossyScale.z) + ModelInformation.Center);
            //
            // }
            //
            // return toReturn;
        }

        public override ElementOrigin ElementOrigin => ElementOrigin.OpenPass;
    }
}