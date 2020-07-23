using System;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Visualization.Agents {
    public class PedestrianAgent : Agent {

        public override void Prepare() {
            base.Prepare();
            
            // coloring the pedestrian
            Model.transform.GetChild(0).GetComponent<MeshRenderer>().material = ColorMaterial;
            Model.transform.GetChild(1).GetComponent<MeshRenderer>().material = ColorMaterial;
            
            // scaling to fit the width / length
            Model.transform.GetChild(1).localScale = new Vector3(ModelInformation.Length, ModelInformation.Width, 1);

            // preparing the label
            OwnLabel.SetStrings(gameObject.name);
            OwnLabel.SetFloats(3.2f);
        }

        protected override void UpdatePosition() {
            var nextPositionPointer = new Vector2(deltaS, 0);
            nextPositionPointer.RotateRadians(previous.Rotation);

            Model.transform.position = new Vector3(previous.Position.x + nextPositionPointer.x, 0, 
                previous.Position.y + nextPositionPointer.y);
        }

        protected override void UpdateRotation() {
            var currentHdg = deltaTMs * ((previous.Next.Rotation - previous.Rotation) / 100) + previous.Rotation;
            Model.transform.rotation = Quaternion.Euler(0, -currentHdg * Mathf.Rad2Deg, 0);
        }

        public override Vector3 GetAnchorPoint() {
            var position = Model.transform.position;
            return new Vector3(position.x, 2f, position.z);
        }

        protected override void UpdateLabel() {
            var modelPosition = Model.transform.position;
            OwnLabel.UpdateFloats(modelPosition.x, modelPosition.z, previous.Velocity, previous.Acceleration);
            OwnLabel.UpdateStrings(previous.AdditionalInformation.CrossingPhase, previous.AdditionalInformation.ScanAoI, previous.AdditionalInformation.GlanceType);
            OwnLabel.UpdatePositions(previous.AdditionalInformation.OtherAgents);
        }

        public override void HandleHit() {
            if (isTarget) return; // targets will not be handled on occlusion
        }

        public override void HandleNonHit() {
            // TODO implement
        }
        
        public override Vector3[] GetReferencePointsRenderer() {
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
        
        public override Vector3[] GetReferencePointsCustom() {
            var toReturn = new Vector3[customPoints.customPoints.Count];
            var tr2 = Model.transform.localToWorldMatrix;
            for (var i = 0; i < toReturn.Length; i++) {
                toReturn[i] = tr2.MultiplyPoint3x4(customPoints.customPoints[i]);
            }

            return toReturn;
        }
    }
}