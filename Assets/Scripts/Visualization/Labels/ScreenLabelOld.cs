using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Visualization.Agents;
using Visualization.OcclusionManagement;
using Color = UnityEngine.Color;

namespace Visualization.Labels {
    public class ScreenLabelOld : LabelOld {
        public RectTransform LabelMainObject { get; private set; }
        
        public Agent Agent { get; set; }
        
        public Image Pointer { get; set; }
        
        public Vector2 AnchorScreenPosition { get; set; }
        
        public LabelOcclusionManager LabelOcclusionManager { get; set; }

        private RectTransform _cognitiveMap;

        private readonly List<RectTransform> _otherAgents = new List<RectTransform>();
        
        private TextMeshProUGUI _position;
        private TextMeshProUGUI _velocity;
        private TextMeshProUGUI _acceleration;

        private TextMeshProUGUI _crossingPhase;
        private TextMeshProUGUI _scanAoi;
        private TextMeshProUGUI _gazeType;

        private RectTransform _sensors;
        private List<AgentSensor> _sensorList = new();

        /// <summary>
        /// Called on program start, retrieves the necessary objects to display information
        /// </summary>
        private void Start() {
            LabelMainObject = transform.GetChild(0).GetComponent<RectTransform>();

            FindLabels();
            
            // setting the camera parameters
            var targetTexture = new RenderTexture(250, 250, 1);
            _cognitiveMap.GetComponent<RawImage>().texture = targetTexture;
            AgentCamera.aspect = 1f;
            AgentCamera.orthographicSize = 100f;
            AgentCamera.targetTexture = targetTexture;
        }

        /// <summary>
        /// Retrieves the objects for displaying the data
        /// </summary>
        protected virtual void FindLabels() {
            _gazeType = LabelMainObject.GetChild(0).GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();
            _scanAoi = LabelMainObject.GetChild(0).GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>();
            _crossingPhase = LabelMainObject.GetChild(0).GetChild(2).GetChild(1).GetComponent<TextMeshProUGUI>();

            _cognitiveMap = LabelMainObject.GetChild(1).GetChild(1).GetChild(0).GetComponent<RectTransform>();
            
            _position = LabelMainObject.GetChild(2).GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();
            _velocity = LabelMainObject.GetChild(2).GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>();
            _acceleration = LabelMainObject.GetChild(2).GetChild(2).GetChild(1).GetComponent<TextMeshProUGUI>();

            _sensors = LabelMainObject.GetChild(2).GetChild(3).GetComponent<RectTransform>();
            
            if (_sensorList.Count == 0)
                _sensors.gameObject.SetActive(false);
        }
        
        public override void UpdateStrings(params string[] parameters) {
            if (LabelOcclusionManager.Disable) return;
            _crossingPhase.text = parameters[0];
            _scanAoi.text = parameters[1];
            _gazeType.text = parameters[2];
        }

        public override void UpdateIntegers(params int[] parameters) {
            if (LabelOcclusionManager.Disable) return;
            // nothing to be updated here (yet) TODO add
        }

        public override void UpdateFloats(params float[] parameters) {
            if (LabelOcclusionManager.Disable) return;
            var posX = parameters[0];
            var posY = parameters[1];

            var vel = parameters[2];
            var acc = parameters[3];

            _position.text = Math.Round(posX, 1) + "/" + Math.Round(posY, 1);
            _velocity.text = Math.Round(vel, 2) + "m/s";
            _acceleration.text = Math.Round(acc, 2) + "m/s²";
        }
        
        public override void UpdatePositions(params Tuple<Vector2, float>[] parameters) {
            if (LabelOcclusionManager.Disable) return;
            if (parameters == null) return;
            var difference = parameters.Length - _otherAgents.Count;

            if (difference > 0) {
                var go = new GameObject("agent pointer");
                var img = go.AddComponent<Image>();
                img.rectTransform.sizeDelta = new Vector2(7f, 7f);
                img.color = Color.cyan;
                go.transform.parent = _cognitiveMap;
                _otherAgents.Add(img.rectTransform);
            }
            
            for (var i = 0; i < _otherAgents.Count; i++) {
                if (i >= parameters.Length) {
                    _otherAgents[i].localPosition = Vector2.one * 500f;
                    continue;
                }
                
                var localPosition =
                    AgentCamera.WorldToViewportPoint(new Vector3(parameters[i].Item1.x, 0, parameters[i].Item1.y));

                var actualPosition = new Vector2((localPosition.x - 0.5f) * 250, (localPosition.y - 0.5f) * 250);
                _otherAgents[i].localPosition = actualPosition;
                _otherAgents[i].localRotation = Quaternion.identity;
            }
        }
        
        public override void SetColors(params Color[] parameters) {
            if (LabelOcclusionManager.Disable) return;
            Pointer.color = parameters[0];
            transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<Image>().color = parameters[0];
        }

        public override void SetStrings(params string[] parameters) {
        if (LabelOcclusionManager.Disable) return;
            transform.GetChild(0).GetChild(1).GetChild(5).GetComponent<TextMeshProUGUI>().text = parameters[0];
        }

        public override void SetFloats(params float[] parameters) {
            if (LabelOcclusionManager.Disable) return;
            // nothing to be set here (yet)
        }

        public override void Activate() {
            if (!LabelOcclusionManager.Disable) LabelMainObject.gameObject.SetActive(true);
            Pointer.gameObject.SetActive(true);
            AgentCamera.gameObject.SetActive(true);
        }

        public override void Deactivate() {
            if (!LabelOcclusionManager.Disable) LabelMainObject.gameObject.SetActive(false);
            Pointer.gameObject.SetActive(false);
            AgentCamera.gameObject.SetActive(false);
        }

        public void UpdateLabel() {
            AnchorScreenPosition = LabelOcclusionManager.WorldToScreenPoint(Agent.DynamicData.Position3D);
        }

        public override void AddSensor(AgentSensor sensor) {
            _sensors = transform.GetChild(0).GetChild(2).GetChild(3).GetComponent<RectTransform>();
            
            _sensorList.Add(sensor);
            var sensorToggle = Resources.Load<Toggle>("Prefabs/UI/Visualization/Labels/SensorToggle");
            var newSensorToggle = Instantiate(sensorToggle, _sensors, false);
            newSensorToggle.transform.GetChild(1).GetComponent<Text>().text = sensor.name;
            newSensorToggle.onValueChanged.AddListener(sensor.SetOn);
            newSensorToggle.SetIsOnWithoutNotify(true);
        }
    }
}