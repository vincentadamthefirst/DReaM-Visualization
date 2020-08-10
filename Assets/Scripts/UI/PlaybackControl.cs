using System;
using System.Collections.Generic;
using TMPro;
using UI.MagicUI;
using UnityEngine;
using UnityEngine.UI;
using Visualization;
using Visualization.SimulationEvents;

namespace UI {
    public class PlaybackControl : MonoBehaviour {
        public TextMeshProUGUI totalTime;
        public TextMeshProUGUI currentTime;

        private VisualizationMaster _visualizationMaster;

        private MagicUiButtonSymbol _playPause;
        private MagicUiButtonSymbol _forwardBackward;
        private Transform _simulationEventPanel;
        private Slider _timeSlider;

        private int _totalTimeInt;

        public bool Disable { get; set; }

        /// <summary>
        /// The VisualizationDesign to be used
        /// </summary>
        public VisualizationDesign VisualizationDesign { get; set; }
        
        /// <summary>
        /// List containing all the simulation events, these will be placed at start
        /// </summary>
        public List<SimulationEvent> SimulationEvents { get; } = new List<SimulationEvent>();

        public void FindAll() {
            _visualizationMaster = FindObjectOfType<VisualizationMaster>();
            _playPause = transform.GetChild(2).GetChild(0).GetComponent<MagicUiButtonSymbol>();
            _forwardBackward = transform.GetChild(2).GetChild(1).GetComponent<MagicUiButtonSymbol>();
            _simulationEventPanel = transform.GetChild(0);

            _timeSlider = transform.Find("TimeSlider").GetComponent<Slider>();
            
            PlaceEventMarkers();
        }

        private void Update() {
            if (Disable) return;
            
            if (Input.GetKeyUp(KeyCode.Space)) {
                HandlePlayPause();
            }

            if (Input.GetKeyUp(KeyCode.Backspace)) {
                HandlePlayBackwards();
            }
        }

        /// <summary>
        /// Places any EventMarkers based on the time that they occur
        /// </summary>
        private void PlaceEventMarkers() {
            foreach (var se in SimulationEvents) {
                var c = Color.white;

                if (!VisualizationDesign.GetEventColor(se.Name, ref c)) continue;

                var instantiated = Instantiate(VisualizationDesign.eventMarkerPrefab, _simulationEventPanel, true);
                instantiated.transform.localPosition =
                    new Vector3(Mathf.Lerp(-550f, 550f, se.TimeStep / (float) _totalTimeInt), 0, 0);
            }
        }

        public void UpdateCurrentTime(int current) {
            currentTime.text = current + " ms";
            _timeSlider.SetValueWithoutNotify(current);
        }

        public void SetTotalTime(int total) {
            totalTime.text = total + " ms";
            _totalTimeInt = total;

            _timeSlider.maxValue = total;
            _timeSlider.minValue = 0;
        }

        public void HandlePlayPause() {
            _visualizationMaster.Pause = !_visualizationMaster.Pause;
            _playPause.ChangeSymbol();
        }

        public void HandlePlayBackwards() {
            _visualizationMaster.PlayBackwards = !_visualizationMaster.PlayBackwards;
            _forwardBackward.ChangeSymbol();
        }

        public void TimeSliderValueChanged() {
            _visualizationMaster.CurrentTime = Mathf.RoundToInt(_timeSlider.value);
            if (_visualizationMaster.Pause) {
                _visualizationMaster.SmallUpdate();
            }
        }
    }
}