using UnityEngine;

namespace Scenery.RoadNetwork.RoadSignals {
    public abstract class RoadSignal : VisualizationElement {
        
        public override bool IsDistractor => true;

        /// <summary>
        /// Repeats the object if necessary and generates its Mesh.
        /// </summary>
        public abstract void Show();

        protected override Vector3[] GetReferencePointsRenderer() {
            return new Vector3[0]; // assume scenery is never target, Ignore
        }

        protected override Vector3[] GetReferencePointsCustom() {
            return new Vector3[0]; // assume scenery is never target, Ignore
        }
    }
}