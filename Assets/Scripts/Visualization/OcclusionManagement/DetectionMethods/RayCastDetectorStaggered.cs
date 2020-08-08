namespace Visualization.OcclusionManagement.DetectionMethods {
    public class RayCastDetectorStaggered : RayCastDetector {

        private int _lastTarget;

        public override void Trigger() {
            if (Targets.Count == 0) return;
            if (_lastTarget > Targets.Count - 1) _lastTarget = 0;

            CastRay(Targets[_lastTarget]);

            _lastTarget++;
            
            // ending the measurement, gets started inside of 'CastRay()'
            DetectionMeasurement.EndMeasurement();
            HandlingMeasurement.EndMeasurement();
        }
    }
}