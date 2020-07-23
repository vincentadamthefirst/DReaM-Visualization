namespace Visualization.OcclusionManagement.DetectionMethods {
    public class RayCastNormal : RayCastDetector {
        public override void Trigger() {
            foreach (var target in Targets) {
                CastRay(target);
            }
        }
    }
}