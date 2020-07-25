using UnityEngine;

namespace Scenery.RoadNetwork.RoadSignals {
    public class RoadSignal : VisualizationElement {
        
        public override bool IsDistractor => true;
        
        public override void HandleHit() {
            throw new System.NotImplementedException();
        }

        public override void HandleNonHit() {
            throw new System.NotImplementedException();
        }

        protected override Vector3[] GetReferencePointsRenderer() {
            return new Vector3[0]; // assume scenery is never target, Ignore
        }

        protected override Vector3[] GetReferencePointsCustom() {
            return new Vector3[0]; // assume scenery is never target, Ignore
        }
    }
}