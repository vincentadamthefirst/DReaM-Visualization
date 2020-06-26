namespace Scenery.RoadNetwork.RoadObjects {
    public class RoadObjectIrregular : RoadObject {
        protected override void Repeat() {
            throw new System.NotImplementedException();
        }

        public override void Show() {
            Repeat();  // repeat if these parameters are set
        }
    }
}