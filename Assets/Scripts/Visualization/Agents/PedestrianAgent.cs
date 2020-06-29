using Scenery;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;
using Utils;

namespace Visualization.Agents {
    public class PedestrianAgent : Agent {

        public ThirdPersonCharacter thirdPersonCharacter;
        
        protected override void UpdatePosition() {
            var nextPositionPointer = new Vector2(deltaS + 1.35f, 0);
            nextPositionPointer.RotateRadians(previous.Rotation);

            thirdPersonCharacter.transform.position = new Vector3(previous.Position.x + nextPositionPointer.x, 0, 
                previous.Position.y + nextPositionPointer.y);

            var move = new Vector3(nextPositionPointer.x, 0, nextPositionPointer.y);
            thirdPersonCharacter.Move(move);
        }

        protected override void UpdateRotation() {
            // rotate using ThirdPersonCharacter
        }

        public override void Pause() {
            thirdPersonCharacter.Move(Vector3.zero);
        }
    }
}