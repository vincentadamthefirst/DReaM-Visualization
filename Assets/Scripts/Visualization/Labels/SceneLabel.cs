using System;
using TMPro;
using UnityEngine;

namespace Visualization.Labels {
    public class SceneLabel : Label {

        private Camera _mainCamera;

        protected Transform labelMainObject;

        private TextMeshPro _position;
        private TextMeshPro _velocity;
        private TextMeshPro _acceleration;

        private TextMeshPro _crossingPhase;
        private TextMeshPro _scanAoi;
        private TextMeshPro _gazeType;

        private float _height = 3f;

        /// <summary>
        /// Called on program start, retrieves the necessary objects to display information
        /// </summary>
        private void Start() {
            _mainCamera = Camera.main;

            labelMainObject = transform.GetChild(0);
            
            FindLabels();
        }

        /// <summary>
        /// Called every frame, makes the label always face the camera
        /// </summary>
        private void Update() {
            transform.LookAt(2 * transform.position - _mainCamera.transform.position);
        }

        /// <summary>
        /// Retrieves the objects for displaying the data
        /// </summary>
        protected virtual void FindLabels() {
            _gazeType = labelMainObject.GetChild(0).GetChild(0).GetChild(1).GetComponent<TextMeshPro>();
            _scanAoi = labelMainObject.GetChild(0).GetChild(1).GetChild(1).GetComponent<TextMeshPro>();
            _crossingPhase = labelMainObject.GetChild(0).GetChild(2).GetChild(1).GetComponent<TextMeshPro>();

            // TODO add minimap
            
            _position = labelMainObject.GetChild(2).GetChild(0).GetChild(1).GetComponent<TextMeshPro>();
            _velocity = labelMainObject.GetChild(2).GetChild(1).GetChild(1).GetComponent<TextMeshPro>();
            _acceleration = labelMainObject.GetChild(2).GetChild(2).GetChild(1).GetComponent<TextMeshPro>();
            
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
            
            transform.position = new Vector3(posX, _height, posY);
        }

        public override void UpdatePositions(params Tuple<Vector2, float>[] parameters) {
            // TODO implement
        }

        public override void SetColors(params Color[] parameters) {
            // TODO implement
        }

        public override void SetStrings(params string[] parameters) {
            transform.GetChild(0).GetChild(1).GetChild(4).GetChild(1).GetComponent<TextMeshPro>().text = parameters[0];
        }

        public override void SetFloats(params float[] parameters) {
            _height = parameters[0];
        }

        public override void Activate() {
            labelMainObject.gameObject.SetActive(true);
        }

        public override void Deactivate() {
            labelMainObject.gameObject.SetActive(false);
        }
    }
}