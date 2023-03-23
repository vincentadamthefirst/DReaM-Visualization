using System;
using Scenery;
using UnityEngine;
using Utils;

namespace Visualization.Agents {
    public class PedestrianAgent : Agent {
        private Vector3 _heightOffset;

        public override void Prepare() {
            // coloring the pedestrian
            foreach (var mr in StaticData.Model.transform.GetComponentsInChildren<MeshRenderer>()) {
                mr.material = StaticData.ColorMaterial;
            }

            base.Prepare();

            Debug.Log(
                $"{StaticData.ModelInformation.Length} - {StaticData.ModelInformation.Width} - {StaticData.ModelInformation.Height}");

            // scaling to fit the width / length
            // if (Model.transform.GetChild(1).GetComponent<Camera>() == null)
            //     Model.transform.GetChild(1).localScale =
            //         new Vector3(ModelInformation.Length, ModelInformation.Width, 1);
            // else
            ReScale(StaticData.Model.transform.GetChild(0).gameObject, new Vector3(StaticData.ModelInformation.Width,
                StaticData.ModelInformation.Length,
                StaticData.ModelInformation.Height));
            // Model.transform.GetChild(0).localScale = new Vector3(ModelInformation.Width, ModelInformation.Length,
            //     ModelInformation.Height);

            // preparing the label
            // if (OwnLabel != null) {
            //     OwnLabel.SetStrings(gameObject.name.Split(new[] { " [" }, StringSplitOptions.None)[0]);
            //     OwnLabel.SetFloats(3.2f);
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
                new Vector3(StaticData.ModelInformation.Length, StaticData.ModelInformation.Height,
                    StaticData.ModelInformation.Width));

            _heightOffset = new Vector3(0, StaticData.ModelInformation.Height / 2f, 0);
        }

        public void ReScale(GameObject theGameObject, Vector3 dimensions) {
            var size = theGameObject.GetComponent<Renderer>().bounds.size;
            Debug.Log(size);
            var rescale = theGameObject.transform.localScale;

            rescale.x = dimensions.x * rescale.x / size.z;
            rescale.y = dimensions.y * rescale.y / size.x;
            rescale.z = dimensions.z * rescale.z / size.y;

            theGameObject.transform.localScale = rescale;
        }

        protected override void UpdatePosition() {
            var nextPositionPointer = new Vector2(deltaS, 0);
            nextPositionPointer.RotateRadians(DynamicData.ActiveSimulationStep.Rotation);

            StaticData.Model.transform.position = new Vector3(
                DynamicData.ActiveSimulationStep.Position.x + nextPositionPointer.x, 0,
                DynamicData.ActiveSimulationStep.Position.y + nextPositionPointer.y);

            DynamicData.Position3D = StaticData.Model.transform.position;
            boundingBox.center = DynamicData.Position3D + _heightOffset;
        }

        protected override void UpdateRotation() {
            var currentHdg =
                deltaTMs * ((DynamicData.ActiveSimulationStep.Next.Rotation - DynamicData.ActiveSimulationStep.Rotation) / 100) +
                DynamicData.ActiveSimulationStep.Rotation;
            StaticData.Model.transform.rotation = Quaternion.Euler(0, -currentHdg * Mathf.Rad2Deg, 0);

            DynamicData.Rotation = currentHdg;
        }

        // protected override void UpdateLabel() {
        //     if (OwnLabel == null)
        //         return;
        //     
        //     var avi = DynamicData.PreviousStep.AdditionalInformation as AdditionalPedestrianInformation;
        //     
        //     var modelPosition = Model.transform.position;
        //     OwnLabel.UpdateFloats(modelPosition.x, modelPosition.z, DynamicData.PreviousStep.Velocity, DynamicData.PreviousStep.Acceleration);
        //     OwnLabel.UpdateStrings(DynamicData.PreviousStep.AdditionalInformation.CrossingPhase, DynamicData.PreviousStep.AdditionalInformation.ScanAoI, DynamicData.PreviousStep.AdditionalInformation.GlanceType);
        //     OwnLabel.UpdatePositions(DynamicData.PreviousStep.AdditionalInformation.OtherAgents);
        //     OwnLabel.UpdateIntegers(avi?.Stopping ?? false ? 1 : 0);
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
            // var toReturn = new Vector3[customPoints.customPoints.Count];
            // var tr2 = Model.transform.localToWorldMatrix;
            // for (var i = 0; i < toReturn.Length; i++) {
            //     toReturn[i] = tr2.MultiplyPoint3x4(customPoints.customPoints[i]);
            // }
            //
            // return toReturn;
        }

        public override ElementOrigin ElementOrigin => ElementOrigin.OpenPass;
    }
}