using System;
using System.Collections.Generic;
using System.Linq;
using Scenery;
using Scenery.RoadNetwork;
using UnityEngine;
using Utils;
using Visualization.Labels;
using Visualization.OcclusionManagement;

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
        
        /// <summary>
        /// The rotation of the agent at a given time (in radians).
        /// </summary>
        public float CurrentRotation { get; protected set; }
        
        /// <summary>
        /// The position of the agent at the current time (world coordinates).
        /// </summary>
        public Vector3 CurrentPosition { get; protected set; }
        
        /// <summary>
        /// The design used in this program run.
        /// </summary>
        public AgentDesigns AgentDesigns { get; set; }

        /// <summary>
        /// The sensor representing the driver view
        /// </summary>
        public AgentSensor DriverView { get; set; }

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

        protected Bounds boundingBox;

        public override Vector3 WorldAnchor => Model.transform.position;

        public override bool IsActive => !deactivated;

        private MeshRenderer[] _modelRenderers;
        
        public override bool IsDistractor => true;

        private bool _targetStatusChanged;

        public override Bounds AxisAlignedBoundingBox => boundingBox;
        
        // the materials for this agents meshes
        private Material[][] _nonOccludedMaterials;
        private Material[][] _occludedMaterials;

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

            _modelRenderers = GetComponentsInChildren<MeshRenderer>();
            
            _nonOccludedMaterials = new Material[_modelRenderers.Length][];

            for (var i = 0; i < _modelRenderers.Length; i++) {
                _nonOccludedMaterials[i] = _modelRenderers[i].materials;
            }
            
            SetupOccludedMaterials();
            
            SetupSensors();
        }

        public override void SetupOccludedMaterials() {
            _occludedMaterials = new Material[_modelRenderers.Length][];
            
            for (var i = 0; i < _modelRenderers.Length; i++) {
                Material[] tmp;

                if (OcclusionManagementOptions.occlusionHandlingMethod == OcclusionHandlingMethod.Transparency) {
                    tmp = new Material[_modelRenderers[i].materials.Length];
                    for (var j = 0; j < _modelRenderers[i].materials.Length; j++) {
                        tmp[j] = new Material(_nonOccludedMaterials[i][j]);
                        tmp[j].ChangeToTransparent(OcclusionManagementOptions.agentTransparencyValue);
                    }
                } else {
                    tmp = new Material[_modelRenderers[i].materials.Length];
                    for (var j = 0; j < _modelRenderers[i].materials.Length; j++) {
                        tmp[j] = OcclusionManagementOptions.wireFrameMaterial;
                    }
                }
                
                _occludedMaterials[i] = tmp;
            }
        }

        /// <summary>
        /// Initializes this agents sensors if necessary. Currently only handles the drivers view.
        /// </summary>
        private void SetupSensors() {
            // TODO extend by adding support for >1 sensor
            
            // step zero: initializing (for now only one) AgentSensor
            var sensorScript = Instantiate(AgentDesigns.sensorPrefab, transform.parent);
            DriverView = sensorScript;
            DriverView.FindAll();
            
            // step one: is a driver sensor needed?
            var driverSensorNeeded = SimulationSteps.Values.Any(step => step.SensorInformation.Count > 0);
            if (!driverSensorNeeded) return; // no sensor, stop execution
            
            // step two: filling holes in the simulation steps
            foreach (var step in SimulationSteps.Values) {
                if (step.SensorInformation.Count != 0) continue;
                // this steps information is empty
                var dummyData = new SensorInformation {Distance = 0, Heading = 0, OpeningAngle = 0};
                step.SensorInformation.Add(dummyData);
            }
            
            // ordered list of SimulationSteps
            var ordered = SimulationSteps.Values.OrderBy(s => s.Time).ToArray();
            
            // step three: check if the sensor data changes between SimulationSteps
            for (var i = 0; i < ordered.Length - 1; i++) {
                var a = ordered[i];
                var b = ordered[i + 1];

                for (var j = 0; j < a.SensorInformation.Count; j++) {
                    if (Math.Abs(a.SensorInformation[j].OpeningAngle - b.SensorInformation[j].OpeningAngle) >
                        Tolerance) { // opening angles not the same
                        a.SensorInformation[j].OpeningChangedTowardsNext = true;
                        b.SensorInformation[j].OpeningChangedTowardsPrevious = true;
                    }
                }
            }
            
            // step four: set first and last Sensor data in SimulationSteps
            ordered[0].SensorInformation.ForEach(x => x.OpeningChangedTowardsPrevious = true);
            ordered[ordered.Length - 1].SensorInformation.ForEach(x => x.OpeningChangedTowardsNext = true);
            
            // step five: setting the material
            var sensorMat = new Material(AgentDesigns.sensorBase) {
                color = new Color(ColorMaterial.color.r, ColorMaterial.color.g, ColorMaterial.color.b,
                    AgentDesigns.sensorBase.color.a)
            };
            DriverView.SetMeshMaterial(sensorMat);
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
            
            // updating the driver view (if there is one)
            if (previous.SensorInformation.Count != 0) {
                var driverSensorInfo = previous.SensorInformation[0];
                if (backwards && driverSensorInfo.OpeningChangedTowardsNext ||
                    !backwards && driverSensorInfo.OpeningChangedTowardsPrevious || _targetStatusChanged) {
                    DriverView.UpdateOpeningAngle(driverSensorInfo.OpeningAngle, driverSensorInfo.Distance);
                }

                DriverView.UpdatePositionAndRotation(CurrentPosition + new Vector3(0, 1f, 0), driverSensorInfo.Heading);
            }
        }

        protected abstract void UpdatePosition();

        protected abstract void UpdateRotation();
        
        /// <summary>
        /// The anchor point for this agents labelV
        /// </summary>
        public abstract Vector3 GetAnchorPoint();

        /// <summary>
        /// Get called if the agent is a target. Update the Label of this Agent with the necessary data.
        /// </summary>
        protected abstract void UpdateLabel();

        private void Deactivate() {
            deactivated = true;
            Model.SetActive(false);
            DriverView.SetActive(false);
        }

        private void Activate() {
            deactivated = false;
            Model.SetActive(true);
            DriverView.SetActive(true);
        }

        public override void SetIsTarget(bool target) {
            base.SetIsTarget(target);
            Model.SetLayerRecursive(target && OcclusionManagementOptions.occlusionDetectionMethod ==
                                    OcclusionDetectionMethod.Shader
                ? 14
                : 15);
            DriverView.SetActive(target);
            _targetStatusChanged = true;

            if (OwnLabel.GetType().IsSubclassOf(typeof(SceneLabel))) {
                if (target) OwnLabel.Activate();
                else OwnLabel.Deactivate();
            }
        }
        
        public override void HandleHit() {
            for (var i = 0; i < _modelRenderers.Length; i++) {
                _modelRenderers[i].materials = _occludedMaterials[i];
            }
        }

        public override void HandleNonHit() {
            for (var i = 0; i < _modelRenderers.Length; i++) {
                _modelRenderers[i].materials = _nonOccludedMaterials[i];
            }
        }
    }
}