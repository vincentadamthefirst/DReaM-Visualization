using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Evaluation {
    public class EvaluationCameraMover : MonoBehaviour {
        
        // the list containing all camera positions
        public List<CameraPosition> cameraPositions = new List<CameraPosition>();

        // the index of the current position
        private int _currentPosition;

        // the camera controller of the main camera
        private SimpleCameraController _simpleCameraController;
        
        // percentage of the time (to the next CameraPosition) that has passed since the current CameraPosition was
        // reached, used for lerping between positions / rotations
        private float _t;

        private void Start() {
            _simpleCameraController = FindObjectOfType<SimpleCameraController>();
            _simpleCameraController.AutomaticMovement = true;
            
            // set camera position to start position
            var cameraTransform = _simpleCameraController.transform;
            cameraTransform.position = cameraPositions[0].position;
            cameraTransform.eulerAngles = cameraPositions[0].rotation;
        }

        private void Update() {
            if (_currentPosition > cameraPositions.Count - 2) {
                // at the end of the movement
                SceneManager.LoadScene(0); // return to main menu
                return;
            }
            
            _t += Time.deltaTime / cameraPositions[_currentPosition].activeS;
            var currPos = cameraPositions[_currentPosition];
            var nextPos = cameraPositions[_currentPosition + 1];
            
            _simpleCameraController.transform.position = Vector3.Lerp(currPos.position, nextPos.position, _t);
            _simpleCameraController.transform.rotation = Quaternion.Lerp(Quaternion.Euler(currPos.rotation),
                Quaternion.Euler(nextPos.rotation), _t);

            if (!(_t > 1)) return;
            _currentPosition++;
            _t = 0;
        }

        private void OnDestroy() {
            if (_simpleCameraController == null) return;
            _simpleCameraController.AutomaticMovement = false;
        }
    }
    
    [Serializable]
    public class CameraPosition {
        public Vector3 position;
        public Vector3 rotation;
        public float activeS;
    }
}