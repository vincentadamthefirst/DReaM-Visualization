using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Scenery;
using Scenery.RoadNetwork;
using UnityEngine;
using Utils;

namespace Visualization.Agents {
    public class DynamicData {
        public float Rotation { get; set; }

        public Vector3 Position3D { get; set; }
        public Vector2 Position2D => new Vector2(Position3D.x, Position3D.z);

        public bool Active { get; set; }

        public int CurrentTime { get; set; }

        public SimulationStep ActiveSimulationStep { get; set; }
    }

    public class StaticData {
        public AgentTypeDetail AgentTypeDetail;
        public GameObject Model { get; set; }
        public ModelInformation ModelInformation { get; set; }
        public Material ColorMaterial { get; set; }
        
        public Dictionary<string, SensorSetup> UniqueSensors { get; set; }

        public int MaxTimeStep { get; set; }
        public int MinTimeStep { get; set; }
    }

    public abstract class Agent : TargetableElement {

        public VisualizationMaster Master { get; set; }
        
        public DynamicData DynamicData { get; } = new();
        public StaticData StaticData { get; } = new();

        // invoked whenever the agent is deactivated
        public event EventHandler OnAgentDeactivation;

        public event EventHandler AgentUpdated;

        /// <summary>
        /// The current SimulationStep Object
        /// </summary>
        public Dictionary<int, SimulationStep> SimulationSteps { get; set; } = new();

        // the delta time to the previous sample
        protected int deltaTMs;

        // the distance covered to the previous sample
        protected float deltaS;

        protected Bounds boundingBox;

        private MeshRenderer[] _modelRenderers;

        [CanBeNull]
        public SensorInformation GetSensorData(string sensorName) {
            var success = DynamicData.ActiveSimulationStep.SensorInformation.TryGetValue(sensorName, out var info);
            return success ? info : null;
        }

        public SensorSetup GetSensorSetup(string sensorName) {
            // TODO
            throw new NotImplementedException();
        }

        /// <summary>
        /// Prepares this agent
        /// </summary>
        public virtual void Prepare() {
            if (SimulationSteps.Count != 0) {
                StaticData.MaxTimeStep = SimulationSteps.Max(e => e.Key);
                StaticData.MinTimeStep = SimulationSteps.Min(e => e.Key);
            } else {
                StaticData.MaxTimeStep = -1;
                StaticData.MinTimeStep = -1;
            }

            var ordered = SimulationSteps.Values.OrderBy(s => s.Time).ToArray();
            for (var i = 0; i < SimulationSteps.Values.Count - 1; i++) {
                // setting the next simulation step
                ordered[i].Next = ordered[i + 1];
                ordered[i + 1].Previous = ordered[i];
            }

            _modelRenderers = GetComponentsInChildren<MeshRenderer>();

            SetupSensors();
        }

        /// <summary>
        /// Initializes this agents sensors if necessary. Currently only handles the drivers view.
        /// </summary>
        private void SetupSensors() {
            // find the amount of sensors this agent needs
            // foreach (var sensorInfo in SimulationSteps.Values.SelectMany(step =>
            //              step.SensorInformation.Where(sensorInfo => !uniqueSensorNames.Contains(sensorInfo.Key)))) {
            //     StaticData.UniqueSensors.Add(sensorInfo.Key);
            // }

            // var sensorColors = new[] {
            //     new Color(.26f, .83f, .76f, AgentDesigns.sensorBase.color.a),
            //     new Color(.94f, .62f, .25f, AgentDesigns.sensorBase.color.a),
            //     new Color(.4f, .85f, .38f, AgentDesigns.sensorBase.color.a),
            // };

            // // initializing all sensors for this agent
            // for (var i = 0; i < uniqueSensorNames.Count; i++) {
            //     var uniqueSensor = uniqueSensorNames[i];
            //     var sensorScript = Instantiate(AgentDesigns.sensorPrefab, transform.parent);
            //     sensorScript.name = /*name + " - Sensor " +*/ uniqueSensor;
            //     _agentSensors.Add(uniqueSensor, sensorScript);
            //     sensorScript.FindAll();
            //
            //     var sensorMat = new Material(AgentDesigns.sensorBase) {
            //         color = i < 3 ? sensorColors[i] : ColorMaterial.color.WithAlpha(AgentDesigns.sensorBase.color.a)
            //         //color = ColorMaterial.color.WithAlpha(AgentDesigns.sensorBase.color.a)
            //     };
            //     sensorScript.SetMeshMaterial(sensorMat);
            //     if (OwnLabel != null) OwnLabel.AddSensor(sensorScript);
            // }

            // ordering the simulation steps
            var ordered = SimulationSteps.Values.OrderBy(s => s.Time).ToArray();

            // ensure that the first step contains all sensors
            // foreach (var uniqueSensor in uniqueSensorNames.Where(uniqueSensor =>
            //              !ordered[0].SensorInformation.ContainsKey(uniqueSensor))) {
            //     ordered[0].SensorInformation.Add(uniqueSensor, new SensorInformation {
            //         Distance = 0, Heading = 0, OpeningAngle = 0
            //     });
            // }

            // filling missing data in the simulation steps & mark if data changes
            for (var i = 0; i < ordered.Length - 1; i++) {
                var a = ordered[i];
                var b = ordered[i + 1];

                // filling missing data
                var diff = a.SensorInformation.Keys.Except(b.SensorInformation.Keys);
                foreach (var diffSensor in diff) {
                    b.SensorInformation.Add(diffSensor, new SensorInformation {
                        Distance = 0, Heading = 0, OpeningAngle = 0
                    });
                }

                foreach (var info in a.SensorInformation) {
                    var curr = info.Value;
                    var next = b.SensorInformation[info.Key];
                    if (Math.Abs(curr.OpeningAngle - next.OpeningAngle) > 0.00001f) {
                        curr.OpeningChangedTowardsNext = true;
                        next.OpeningChangedTowardsPrevious = true;
                    }
                }
            }

            // set first and last Sensor data in SimulationSteps
            if (ordered.Length != 0) {
                ordered[0].SensorInformation.Values.ToList().ForEach(x => x.OpeningChangedTowardsPrevious = true);
                ordered[ordered.Length - 1].SensorInformation.Values.ToList()
                    .ForEach(x => x.OpeningChangedTowardsNext = true);
            }
        }

        /// <summary>
        /// Updates this agents position and rotation based on the provided TimeStep, its Acceleration, Velocity and
        /// former Position.
        /// </summary>
        /// <param name="timeStep">The new time step</param>
        /// <param name="backwards">If the playback is currently backwards</param>
        public void UpdateForTimeStep(int timeStep, bool backwards) {
            DynamicData.CurrentTime = timeStep;

            if (timeStep > StaticData.MaxTimeStep || timeStep < StaticData.MinTimeStep) {
                // time step exceeds the range for this agent
                if (DynamicData.Active)
                    SetActive(false);
            } else {
                if (!DynamicData.Active)
                    SetActive(true);
            }

            if (!DynamicData.Active) return;

            DynamicData.ActiveSimulationStep = SimulationSteps[timeStep.RoundDownToMultipleOf(Master.SampleStep)];

            if (DynamicData.ActiveSimulationStep.Next == null) return;

            deltaTMs = timeStep - DynamicData.ActiveSimulationStep.Time;

            if (Mathf.Abs(DynamicData.ActiveSimulationStep.Acceleration) < 0.00001f) {
                deltaS = DynamicData.ActiveSimulationStep.Velocity / 1000f * deltaTMs;
            } else {
                var avgAcceleration =
                    (DynamicData.ActiveSimulationStep.Acceleration + DynamicData.ActiveSimulationStep.Next.Acceleration) / 2f;
                deltaS = DynamicData.ActiveSimulationStep.Velocity / 1000f * deltaTMs +
                         .5f * (avgAcceleration / 1000000f) * Mathf.Pow(deltaTMs, 2f);
            }

            UpdatePosition();
            UpdateRotation();
            
            AgentUpdated?.Invoke(this, EventArgs.Empty);

            // if (!isTarget) return;
            //
            // // update label
            // UpdateLabel();
            //
            // // updating the sensors
            // foreach (var info in previous.SensorInformation) {
            //     var sensor = _agentSensors[info.Key];
            //     if (backwards && info.Value.OpeningChangedTowardsNext ||
            //         !backwards && info.Value.OpeningChangedTowardsPrevious || _targetStatusChanged) {
            //         sensor.UpdateOpeningAngle(info.Value.OpeningAngle, info.Value.Distance);
            //         
            //         if (info.Key == "aeb")
            //             Debug.Log(info.Value.OpeningAngle + " ... " + info.Value.Distance);
            //     }
            //     
            //     
            //     sensor.UpdatePositionAndRotation(CurrentPosition + new Vector3(0, 1f, 0), info.Value.Heading);
            // }
        }

        protected abstract void UpdatePosition();

        protected abstract void UpdateRotation();

        private void SetActive(bool status) {
            DynamicData.Active = status;
            StaticData.Model.SetActive(status);
            if (!status) {
                OnAgentDeactivation?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}