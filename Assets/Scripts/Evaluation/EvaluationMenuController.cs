using TMPro;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Visualization.OcclusionManagement;

namespace Evaluation {
    public class EvaluationMenuController : MonoBehaviour {
        
        public QualitativeEvaluationType TestType { get; set; }

        private Transform _currentTestPanel;

        private Transform _endPanel;

        private bool _firstEsc = true;

        private bool _isPaused = true;

        public TMP_InputField endInput;

        public Button endButton;

        private ExecutionMeasurement _roadMeasurement;
        private ExecutionMeasurement _labelMeasurement;
        private ExecutionMeasurement _detectionMeasurement;
        private ExecutionMeasurement _handlingMeasurement;

        private SimpleCameraController _simpleCameraController;
        private PlaybackControl _playbackControl;
        private QualitativeEvaluation _qualitativeEvaluation;

        public void FindAll() {
            _roadMeasurement = FindObjectOfType<RoadOcclusionManager>().ExecutionMeasurement;
            _labelMeasurement = FindObjectOfType<LabelOcclusionManager>().ExecutionMeasurement;
            _detectionMeasurement = FindObjectOfType<AgentOcclusionManager>().DetectionMeasurement;
            _handlingMeasurement = FindObjectOfType<AgentOcclusionManager>().HandlingMeasurement;

            _simpleCameraController = FindObjectOfType<SimpleCameraController>();
            _playbackControl = FindObjectOfType<PlaybackControl>();
            _qualitativeEvaluation = FindObjectOfType<QualitativeEvaluation>();

            _currentTestPanel = transform.GetChild(0).GetChild((int) TestType - 1);
            _endPanel = transform.GetChild(0).GetChild(7);
            
            endButton.onClick.AddListener(ToMenu);
        }

        private void ToMenu() {
            SceneManager.LoadScene(0);
        }

        private void Update() {
            if (!Input.GetKeyDown(KeyCode.Escape)) return;
            _isPaused = !_isPaused;
            PauseTest(_isPaused);

            if (_firstEsc) {
                _currentTestPanel = _endPanel;
                _firstEsc = false;
            }
        }

        public void PauseTest(bool pause) {
            _roadMeasurement.Disable = pause;
            _labelMeasurement.Disable = pause;
            _detectionMeasurement.Disable = pause;
            _handlingMeasurement.Disable = pause;

            _qualitativeEvaluation.Disable = pause;
            transform.GetChild(0).gameObject.SetActive(pause);
            
            _currentTestPanel.gameObject.SetActive(pause);
            _playbackControl.Disable = pause;
            _simpleCameraController.SetSettingsOpen(pause);
        }
    }
}