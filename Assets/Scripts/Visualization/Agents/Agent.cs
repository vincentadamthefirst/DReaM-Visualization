using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Scenery;
using Scenery.RoadNetwork;
using UnityEngine;
using Utils;
using Visualization.Labels;

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
        
        /// <summary>
        /// The RoadNetworkHolder to Perfom layer change for certain roads
        /// </summary>
        public RoadNetworkHolder RoadNetworkHolder { get; set; }

        /// <summary>
        /// The label of this agent
        /// </summary>
        public Label OwnLabel { get; set; }
        
        /// <summary>
        /// The assigned color for this agent
        /// </summary>
        public Material ColorMaterial { get; set; }
        
        // if the agent is a target object
        private bool _isTarget;

        // if the agent is deactivated
        protected bool deactivated;
        
        /// The step size of one sample time to the next
        protected int timeStepSize;
        
        /// The global current time since start of the visualization in ms
        protected int globalTimeMs;

        // the maximum time step for this agent
        public int MaxTimeStep { get; private set; }
        
        // the minimum time step for this agent
        public int MinTimeStep { get; private set; }

        // the delta time to the previous sample
        protected int deltaTMs;

        // the distance covered to the previous sample
        protected float deltaS;

        // the previous simulation step for this agent
        protected SimulationStep previous;

        protected Agent() {
            SimulationSteps = new Dictionary<int, SimulationStep>();
        }

        /// <summary>
        /// Prepares this agent
        /// </summary>
        public virtual void Prepare() {
            MaxTimeStep = SimulationSteps.Max(e => e.Key);
            MinTimeStep = SimulationSteps.Min(e => e.Key);

            var ordered = SimulationSteps.Values.OrderBy(s => s.Time).ToArray();
            for (var i = 0; i < SimulationSteps.Values.Count - 1; i++) {
                // setting the next simulation step
                ordered[i].Next = ordered[i + 1];
                ordered[i + 1].Previous = ordered[i];

                // setting the bools to check if there was a road change
                if (ordered[i].OnId == ordered[i + 1].OnId) continue;
                ordered[i].OnIdChangedTowardsNext = true;
                ordered[i + 1].OnIdChangedTowardsPrevious = true;
            }

            ordered[0].OnIdChangedTowardsPrevious = true;
            ordered[ordered.Length - 1].OnIdChangedTowardsNext = true;

            foreach (var simulationStep in SimulationSteps.Values) {
                if (simulationStep.OnId != "" && RoadNetworkHolder.Roads.ContainsKey(simulationStep.OnId)) {
                    var road = RoadNetworkHolder.Roads[simulationStep.OnId];

                    simulationStep.OnElement = road.OnJunction ? road.ParentJunction.gameObject : road.gameObject;
                    simulationStep.OnJunction = road.OnJunction;
                }
            }

            timeStepSize = SimulationSteps.Values.ToArray()[1].Time - SimulationSteps.Values.ToArray()[0].Time;
        }

        /// <summary>
        /// Updates this agents position and rotation based on the provided TimeStep, its Acceleration, Velocity and
        /// former Position.
        /// </summary>
        /// <param name="timeStep">The new time step</param>
        /// <param name="backwards">If the playback is currently backwards</param>
        public void UpdateForTimeStep(int timeStep, bool backwards) {
            globalTimeMs = timeStep;

            if (timeStep > MaxTimeStep || timeStep < MinTimeStep) {
                if (!deactivated) {
                    Deactivate();
                    OwnLabel.Deactivate();
                }
            } else {
                if (deactivated) {
                    Activate();
                    OwnLabel.Activate();
                }
            }

            if (deactivated) return;
            
            previous = SimulationSteps[timeStep.RoundDownToMultipleOf(timeStepSize)];

            if (previous.Next == null) return;

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

            if (!_isTarget) return;
            UpdateRoadLayers(backwards);
            UpdateLabel();
        }

        protected abstract void UpdatePosition();

        protected abstract void UpdateRotation();

        /// <summary>
        /// Get called if the agent is a target. Update the Label of this Agent with the necessary data.
        /// </summary>
        protected abstract void UpdateLabel();

        private void UpdateRoadLayers(bool backwards) {
            if (backwards) {
                if (!previous.OnIdChangedTowardsNext) return;
                previous.OnElement.SetLayerRecursive(14);
                try {
                    previous.Next.OnElement.SetLayerRecursive(17);
                } catch (Exception e) {
                    // ignored
                }
            } else {
                if (!previous.OnIdChangedTowardsPrevious) return;
                previous.OnElement.SetLayerRecursive(14);
                try {
                    previous.Previous.OnElement.SetLayerRecursive(17);
                } catch (Exception e) {
                    // ignored
                }
            }
        }

        private void Deactivate() {
            deactivated = true;
            Model.SetActive(false);
        }

        private void Activate() {
            deactivated = false;
            Model.SetActive(true);
        }

        public void SetIsTarget(bool target) {
            Model.SetLayerRecursive(target ? 14 : 15);
            _isTarget = target;
        }
    }
}