using System;
using System.Collections.Generic;
using System.Linq;
using Scenery;
using UI.Main_Menu.Utils;
using UI.SidePanel;
using UnityEngine;
using Utils;
using Visualization.Labels;

namespace Visualization.Agents {
    public abstract class Agent : VisualizationElement {
        /// <summary>
        /// The current SimulationStep Object
        /// </summary>
        public Dictionary<int, SimulationStep> SimulationSteps { get; set; } = new Dictionary<int, SimulationStep>();

        /// <summary>
        /// The model of this agent, this is the object that gets moved.
        /// </summary>
        public GameObject Model { get; set; }
        
        /// <summary>
        /// The id of this agent (in the simulation)
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The Model Information for this Agent
        /// </summary>
        public ModelInformation ModelInformation { get; set; }

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
        
        // The sensors for this agent
        private readonly Dictionary<string, AgentSensor> _agentSensors = new Dictionary<string, AgentSensor>();

        // if the agent is deactivated
        protected bool deactivated;

        /// The global current time since start of the visualization in ms
        protected int globalTimeMs;

        // the maximum time step for this agent
        public int MaxTimeStep { get; private set; }
        
        // the minimum time step for this agent
        public int MinTimeStep { get; private set; }
        
        public int TimeStepSize { get; set; }

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

        public bool WriteToSidePanel { get; set; }

        private SidePanel _sidePanel;
        
        // the materials for this agents meshes
        private Material[][] _nonOccludedMaterials;
        private Material[][] _occludedMaterials;

        /// <summary>
        /// Finds necessary components.
        /// </summary>
        private void Start() {
            renderers = GetComponentsInChildren<Renderer>();
            customPoints = GetComponentInChildren<CustomPoints>();
            _sidePanel = FindObjectOfType<SidePanel>();
        }

        /// <summary>
        /// Prepares this agent
        /// </summary>
        public virtual void Prepare() {
            if (SimulationSteps.Count != 0) {
                MaxTimeStep = SimulationSteps.Max(e => e.Key);
                MinTimeStep = SimulationSteps.Min(e => e.Key);
            } else {
                MaxTimeStep = -1;
                MinTimeStep = -1;
            }

            var ordered = SimulationSteps.Values.OrderBy(s => s.Time).ToArray();
            for (var i = 0; i < SimulationSteps.Values.Count - 1; i++) {
                // setting the next simulation step
                ordered[i].Next = ordered[i + 1];
                ordered[i + 1].Previous = ordered[i];
            }

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
                
                tmp = new Material[_modelRenderers[i].materials.Length];
                for (var j = 0; j < _modelRenderers[i].materials.Length; j++) {
                    tmp[j] = new Material(_nonOccludedMaterials[i][j]);
                    tmp[j].ChangeToTransparent(settings.minimumAgentOpacity);
                }

                _occludedMaterials[i] = tmp;
            }
        }

        /// <summary>
        /// Initializes this agents sensors if necessary. Currently only handles the drivers view.
        /// </summary>
        private void SetupSensors() {
            // find the amount of sensors this agent needs
            var uniqueSensorNames = new List<string>();
            foreach (var sensorInfo in SimulationSteps.Values.SelectMany(step =>
                step.SensorInformation.Where(sensorInfo => !uniqueSensorNames.Contains(sensorInfo.Key)))) {
                uniqueSensorNames.Add(sensorInfo.Key);
            }

            var sensorColors = new[] {
                new Color(.26f, .83f, .76f, AgentDesigns.sensorBase.color.a),
                new Color(.94f, .62f, .25f, AgentDesigns.sensorBase.color.a),
                new Color(.4f, .85f, .38f, AgentDesigns.sensorBase.color.a),
            };

            // initializing all sensors for this agent
            for (var i = 0; i < uniqueSensorNames.Count; i++) {
                var uniqueSensor = uniqueSensorNames[i];
                var sensorScript = Instantiate(AgentDesigns.sensorPrefab, transform.parent);
                sensorScript.name = /*name + " - Sensor " +*/ uniqueSensor;
                _agentSensors.Add(uniqueSensor, sensorScript);
                sensorScript.FindAll();

                var sensorMat = new Material(AgentDesigns.sensorBase) {
                    color = i < 3 ? sensorColors[i] : ColorMaterial.color.WithAlpha(AgentDesigns.sensorBase.color.a)
                    //color = ColorMaterial.color.WithAlpha(AgentDesigns.sensorBase.color.a)
                };
                sensorScript.SetMeshMaterial(sensorMat);
                if (OwnLabel != null) OwnLabel.AddSensor(sensorScript);
            }

            // ordering the simulation steps
            var ordered = SimulationSteps.Values.OrderBy(s => s.Time).ToArray();
            
            // ensure that the first step contains all sensors
            foreach (var uniqueSensor in uniqueSensorNames.Where(uniqueSensor =>
                !ordered[0].SensorInformation.ContainsKey(uniqueSensor))) {
                ordered[0].SensorInformation.Add(uniqueSensor, new SensorInformation {
                    Distance = 0, Heading = 0, OpeningAngle = 0
                });
            }

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
                    if (Math.Abs(curr.OpeningAngle - next.OpeningAngle) > Tolerance) {
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
            globalTimeMs = timeStep;

            if (timeStep > MaxTimeStep || timeStep < MinTimeStep) {
                if (!deactivated) {
                    Deactivate();
                    if (OwnLabel != null) OwnLabel.Deactivate();
                }
            } else {
                if (deactivated) {
                    Activate();
                    if (OwnLabel != null) OwnLabel.Activate();
                }
            }

            if (deactivated) return;
            
            // Debug.Log($"Agent {Id}: {TimeStepSize}");
            
            previous = SimulationSteps[timeStep.RoundDownToMultipleOf(TimeStepSize)];

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
            
            // update label
            UpdateLabel();
            
            // updating the sensors
            foreach (var info in previous.SensorInformation) {
                var sensor = _agentSensors[info.Key];
                if (backwards && info.Value.OpeningChangedTowardsNext ||
                    !backwards && info.Value.OpeningChangedTowardsPrevious || _targetStatusChanged) {
                    sensor.UpdateOpeningAngle(info.Value.OpeningAngle, info.Value.Distance);
                    
                    if (info.Key == "aeb")
                        Debug.Log(info.Value.OpeningAngle + " ... " + info.Value.Distance);
                }
                
                
                sensor.UpdatePositionAndRotation(CurrentPosition + new Vector3(0, 1f, 0), info.Value.Heading);
            }
            
            // update side panel if needed
            if (WriteToSidePanel)
                _sidePanel.UpdateTexts(this, previous.UnknownInformation.ToArray());
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
            foreach (var sensor in _agentSensors.Values) {
                sensor.SetActive(false);
            }
        }

        private void Activate() {
            deactivated = false;
            Model.SetActive(true);
            foreach (var sensor in _agentSensors.Values) {
               sensor.SetActive(true);
            }
        }

        public override void SetIsTarget(bool target) {
            base.SetIsTarget(target);
            foreach (var sensor in _agentSensors.Values) {
                sensor.SetActive(target);
            }
            _targetStatusChanged = true;
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