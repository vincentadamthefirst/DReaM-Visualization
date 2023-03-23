namespace Scenery.RoadNetwork.RoadSignals {
    public abstract class RoadSignal : OccluderElement {
        /// <summary>
        /// Repeats the object if necessary and generates its Mesh.
        /// </summary>
        public abstract void Show();
    }
}