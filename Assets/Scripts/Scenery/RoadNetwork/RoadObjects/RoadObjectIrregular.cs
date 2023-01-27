namespace Scenery.RoadNetwork.RoadObjects {
    
    /// <summary>
    /// Not yet implemented class for irregular OpenDrive objects.
    /// </summary>
    public class RoadObjectIrregular : RoadObject {

        private void Repeat() {
            throw new System.NotImplementedException();
        }

        public override void Show() {
            Repeat();  // repeat if these parameters are set
        }

        public override bool MaybeDelete() {
            throw new System.NotImplementedException();
        }

        public override ElementOrigin ElementOrigin => ElementOrigin.OpenDrive;
        
        public override void OcclusionStart() {
            throw new System.NotImplementedException();
        }

        public override void OcclusionEnd() {
            throw new System.NotImplementedException();
        }

        public override void SetupOccludedMaterials() {
            throw new System.NotImplementedException();
        }
    }
}