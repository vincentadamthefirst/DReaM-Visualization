using System.Transactions;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Visualization.Agents {
    public class PedestrianAgent : Agent {

        public override void Prepare() {
            base.Prepare();
            
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
            // no implementation needed
        }

        public override Vector3 GetAnchorPoint() {
            var position = Model.transform.position;
            return new Vector3(position.x, 2f, position.z);
        }

        protected override void UpdateLabel() {
            var modelPosition = Model.transform.position;
            OwnLabel.UpdateFloats(modelPosition.x, modelPosition.z, previous.Velocity, previous.Acceleration);
        }
    }
}