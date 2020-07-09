using System.Transactions;
using UnityEngine;
using Utils;

namespace Visualization.Agents {
    public class PedestrianAgent : Agent {

        public override void Prepare() {
            base.Prepare();
            
            Model.transform.ScaleToValue(0.5f,  0.5f, 1.8f);
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

        public override void Pause() {
            // no implementation yet
        }
    }
}