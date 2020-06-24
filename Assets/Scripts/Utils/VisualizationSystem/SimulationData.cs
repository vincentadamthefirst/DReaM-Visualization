using System.Collections.Generic;
using Utils.SimulationOutputs;

namespace Utils.VisualizationSystem {
    public class SimulationData {
        private Dictionary<int, SimulationTimeStep> _timeSteps;
        private int _timeStepSize = 100;
        
        public SimulationData() {
            _timeSteps = new Dictionary<int, SimulationTimeStep>();
        }

        public void Reset() {
            _timeSteps.Clear();
            _timeStepSize = 100;
        }

        public void AddTimeStep(SimulationTimeStep timeStep, int time) {
            _timeSteps[time] = timeStep;
        }

        public SimulationTimeStep GetTimeStep(int time) {
            return _timeSteps[time];
        }
    }
}