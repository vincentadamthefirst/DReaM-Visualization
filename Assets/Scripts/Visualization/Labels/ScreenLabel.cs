using System;
using TMPro;
using UnityEngine;
using Visualization.Agents;

namespace Visualization.Labels {
    public class ScreenLabel : Label {

        public RectTransform LabelMainObject;
        
        public Agent Agent { get; set; }
        
        public Vector2 AnchorScreenPosition { get; set; }
        
        public LabelOcclusionManager LabelOcclusionManager { get; set; }
        
        public LineRenderer LineRenderer { get; private set; }
        
        private TextMeshProUGUI _position;
        private TextMeshProUGUI _velocity;
        private TextMeshProUGUI _acceleration;

        private TextMeshProUGUI _crossingPhase;
        private TextMeshProUGUI _scanAoi;
        private TextMeshProUGUI _gazeType;

        /// <summary>
        /// Called on program start, retrieves the necessary objects to display information
        /// </summary>
        private void Start() {
            LabelMainObject = transform.GetChild(0).GetComponent<RectTransform>();
            LineRenderer = LabelMainObject.GetComponent<LineRenderer>();
            LineRenderer.positionCount = 2;
            LineRenderer.useWorldSpace = false;
            LineRenderer.SetPosition(0, Vector3.zero);
            LineRenderer.SetPosition(1, new Vector3(1, 1, 0));
            
            FindLabels();
        }

        private void Update() {
            AnchorScreenPosition = LabelOcclusionManager.WorldToScreenPoint(Agent.GetAnchorPoint());
        }

        /// <summary>
        /// Retrieves the objects for displaying the data
        /// </summary>
        protected virtual void FindLabels() {
            _gazeType = LabelMainObject.GetChild(0).GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();
            _scanAoi = LabelMainObject.GetChild(0).GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>();
            _crossingPhase = LabelMainObject.GetChild(0).GetChild(2).GetChild(1).GetComponent<TextMeshProUGUI>();

            // TODO add minimap
            
            _position = LabelMainObject.GetChild(2).GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();
            _velocity = LabelMainObject.GetChild(2).GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>();
            _acceleration = LabelMainObject.GetChild(2).GetChild(2).GetChild(1).GetComponent<TextMeshProUGUI>();
            
            // TODO add sensors
        }
        
        public override void UpdateStrings(params string[] parameters) {
            _crossingPhase.text = parameters[0];
            _scanAoi.text = parameters[1];
            _gazeType.text = parameters[2];
        }

        public override void UpdateIntegers(params int[] parameters) {
            // nothing to be updated here (yet)
        }

        public override void UpdateFloats(params float[] parameters) {
            var posX = parameters[0];
            var posY = parameters[1];

            var vel = parameters[2];
            var acc = parameters[3];

            _position.text = Math.Round(posX, 1) + "/" + Math.Round(posY, 1);
            _velocity.text = Math.Round(vel, 2) + "m/s";
            _acceleration.text = Math.Round(acc, 2) + "m/s²";
        }

        public override void SetColors(params Color[] parameters) {
            // TODO implement
        }

        public override void SetStrings(params string[] parameters) {
            LabelMainObject.GetChild(1).GetChild(5).GetComponent<TextMeshProUGUI>().text = parameters[0];
        }

        public override void SetFloats(params float[] parameters) {
            // nothing to be set here (yet)
        }

        public override void Activate() {
            LabelMainObject.gameObject.SetActive(true);
        }

        public override void Deactivate() {
            LabelMainObject.gameObject.SetActive(false);
        }
    }
}