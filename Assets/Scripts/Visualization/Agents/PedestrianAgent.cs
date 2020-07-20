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
    }
}