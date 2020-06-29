using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;

namespace Visualization.Agents {
    public abstract class Agent : MonoBehaviour {
        /// <summary>
        /// The current SimulationStep Object
        /// </summary>
        public Dictionary<int, SimulationStep> SimulationSteps { get; private set; }

        /// <summary>
        /// The model of this agent, this is the object that gets moved.
        /// It needs to follow these rules:
        /// 1. Empty GameObject (used for Offset of Center Point)
        ///     2. The actual model
        /// </summary>
        public GameObject Model { get; set; }

        /// <summary>
        /// The Model Information for this Agent
        /// </summary>
        public ModelInformation ModelInformation { get; set; }

        // TODO add Label

        // if the agent is deactivated
        protected bool deactivated;
        
        /// The step size of one sample time to the next
        protected int timeStepSize;
        
        /// The global current time since start of the visualization in ms
        protected int globalTimeMs;

        // the maximum time step for this agent
        protected int maxTimeStep;
        
        // the minimum time step for this agent
        protected int minTimeStep;

        // the delta time to the previous sample
        protected int deltaTMs;

        // the distance covered to the previous sample
        protected float deltaS;

        // the previous simulation step for this agent
        protected SimulationStep previous;

        public Agent() {
            SimulationSteps = new Dictionary<int, SimulationStep>();
        }

        /// <summary>
        /// Prepares this agent
        /// </summary>
        public void Prepare() {
            maxTimeStep = SimulationSteps.Max(e => e.Key);
            minTimeStep = SimulationSteps.Min(e => e.Key);

            var ordered = SimulationSteps.Values.OrderBy(s => s.Time).ToArray();
            for (var i = 0; i < SimulationSteps.Values.Count - 1; i++) {
                ordered[i].Next = ordered[i + 1];
            }

            timeStepSize = SimulationSteps.Values.ToArray()[1].Time - SimulationSteps.Values.ToArray()[0].Time;
            
            Model.transform.GetChild(0).transform.localPosition -= new Vector3(1.285f, 0, 0);

            // TODO offset using ModelInformation
        }

        /// <summary>
        /// Updates this agents position and rotation based on the provided TimeStep, its Acceleration, Velocity and
        /// former Position.
        /// </summary>
        /// <param name="timeStep">The new time step</param>
        public void UpdateForTimeStep(int timeStep) {
            globalTimeMs = timeStep;

            if (timeStep > maxTimeStep || timeStep < minTimeStep) {
                if (!deactivated) Deactivate();
            } else {
                if (deactivated) Activate();
            }

            if (deactivated) return;
            
            previous = SimulationSteps[timeStep.RoundDownToMultipleOf(timeStepSize)];

            deltaTMs = timeStep - previous.Time;
            if (Mathf.Abs(previous.Acceleration) < 0.00001f) {
                deltaS = previous.Velocity / 1000f * deltaTMs;
            } else {
                var avgAcceleration = (previous.Acceleration + previous.Next.Acceleration) / 2f;
                deltaS = previous.Velocity / 1000f * deltaTMs +
                          .5f * (avgAcceleration / 1000000f) * Mathf.Pow(deltaTMs, 2f);
            }

            UpdatePosition();
            UpdateRotation();
        }

        protected abstract void UpdatePosition();

        protected abstract void UpdateRotation();

        public abstract void Pause();

        protected void Deactivate() {
            deactivated = true;
            Model.SetActive(false);
        }

        protected void Activate() {
            deactivated = false;
            Model.SetActive(true);
        }
    }
}