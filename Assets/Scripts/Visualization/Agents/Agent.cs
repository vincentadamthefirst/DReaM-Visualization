using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Scenery;
using Scenery.RoadNetwork;
using UnityEngine;
using Utils;
using Visualization.Labels;
using Visualization.OcclusionManagement;
using Visualization.RoadOcclusion;

namespace Visualization.Agents {
    public abstract class Agent : VisualizationElement {
        /// <summary>
        /// The current SimulationStep Object
        /// </summary>
        public Dictionary<int, SimulationStep> SimulationSteps { get; } = new Dictionary<int, SimulationStep>();

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
        /// The RoadNetworkHolder to Perform layer change for certain roads
        /// </summary>
        public RoadNetworkHolder RoadNetworkHolder { get; set; }
        
        /// <summary>
        /// The occlusion controller for roads to perform layer change on roads
        /// </summary>
        public RoadOcclusionManager RoadOcclusionManager { get; set; }

        /// <summary>
        /// The label of this agent
        /// </summary>
        public Label OwnLabel { get; set; }
        
        /// <summary>
        /// The assigned color for this agent
        /// </summary>
        public Material ColorMaterial { get; set; }

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

        // array of all renderers for this GameObject (all Renderers in children)
        protected Renderer[] renderers;

        // custom points that are defined for this GameObject
        protected CustomPoints customPoints;

        public override Vector3 WorldAnchor => Model.transform.position;

        /// <summary>
        /// Finds necessary components.
        /// </summary>
        private void Start() {
            renderers = GetComponentsInChildren<Renderer>();
            customPoints = GetComponentInChildren<CustomPoints>();
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
            }

            foreach (var simulationStep in SimulationSteps.Values) {
                if (simulationStep.OnId != "" && RoadNetworkHolder.Roads.ContainsKey(simulationStep.OnId)) {
                    var road = RoadNetworkHolder.Roads[simulationStep.OnId];

                    simulationStep.OnElement = road.OnJunction ? road.ParentJunction : (VisualizationElement) road;
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

            if (!isTarget) return;
            RoadOcclusionManager.AddOnElement(previous.OnElement);
            UpdateLabel();
        }

        protected abstract void UpdatePosition();

        protected abstract void UpdateRotation();
        
        /// <summary>
        /// The anchor point for this agents label
        /// </summary>
        public abstract Vector3 GetAnchorPoint();

        /// <summary>
        /// Get called if the agent is a target. Update the Label of this Agent with the necessary data.
        /// </summary>
        protected abstract void UpdateLabel();

        private void Deactivate() {
            deactivated = true;
            Model.SetActive(false);
        }

        private void Activate() {
            deactivated = false;
            Model.SetActive(true);
        }

        public override void SetIsTarget(bool target) {
            base.SetIsTarget(target);
            Model.SetLayerRecursive(target ? 14 : 15);
        }
    }
}