using UnityEngine;

namespace Utils.SimulationOutputs {
    public abstract class SimulationEvent {
        private static int _timeStamp;

        protected SimulationEvent(int timeStamp) {
            _timeStamp = timeStamp;
        }
    }
}