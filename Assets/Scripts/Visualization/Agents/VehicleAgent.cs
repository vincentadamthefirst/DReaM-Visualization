using UnityEngine;
using Utils;

namespace Visualization.Agents {
    public class VehicleAgent : Agent {
        
        protected override void UpdatePosition() {
            var nextPositionPointer = new Vector2(deltaS + 1.35f, 0);
            nextPositionPointer.RotateRadians(previous.Rotation);

            Model.transform.position = new Vector3(previous.Position.x + nextPositionPointer.x, 0,
                previous.Position.y + nextPositionPointer.y);
        }

        protected override void UpdateRotation() {
            var hdg = deltaTMs * ((previous.Next.Rotation - previous.Rotation) / 100) + previous.Rotation;
            Model.transform.rotation = Quaternion.Euler(0, -hdg * Mathf.Rad2Deg, 0);
        }

        public override void Pause() { }
    }
}