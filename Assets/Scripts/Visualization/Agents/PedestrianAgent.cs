using System.Transactions;
using UnityEngine;
using Utils;

namespace Visualization.Agents {
    public class PedestrianAgent : Agent {

        public override void Prepare() {
            base.Prepare();
            
            Model.transform.ScaleToValue(0.5f,  0.5f, 1.8f);
            
            // preparing the label
            //OwnLabel.SetStrings(gameObject.name);
            //OwnLabel.SetFloats(1.8f + 0.5f); // TODO non static
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

        protected override void UpdateLabel() {
            var modelPosition = Model.transform.position;
            OwnLabel.UpdateFloats(modelPosition.x, modelPosition.z, previous.Velocity, previous.Acceleration);
            OwnLabel.transform.position = modelPosition + new Vector3(0, 1.8f + 1.4f, 0); // TODO non static values
        }
    }
}