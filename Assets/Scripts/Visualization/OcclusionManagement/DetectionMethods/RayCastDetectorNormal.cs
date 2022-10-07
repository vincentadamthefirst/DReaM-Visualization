namespace Visualization.OcclusionManagement.DetectionMethods {
    public class RayCastDetectorNormal : RayCastDetector {
        public override void Trigger() {
            foreach (var target in Targets) {
                CastRay(target);
            }
        }
    }
}