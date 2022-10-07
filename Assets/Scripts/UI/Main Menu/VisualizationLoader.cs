using System;
using System.Collections;
using Min_Max_Slider;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI.Main_Menu {
    public class VisualizationLoader : MonoBehaviour {
        public GameObject userInput;
        public MinMaxSlider minMaxSlider;

        private bool _enterListener;
        private DataMover _dataMover;

        private int _stepSize;
        
        public void StartSceneLoad(DataMover dataMover) {
            _dataMover = dataMover;
            
            if (PlayerPrefs.GetInt("app_wip_features") > 0) {
                // first perform a check on how many samples there are
                // if it is over a user-defined value, display a range selection

                var sampleInfo = dataMover.SimulationOutputXmlHandler.GetSampleSize();
                if (sampleInfo.Item3 > int.Parse(PlayerPrefs.GetString("app_samples_show_selection"))) {
                    userInput.SetActive(true);
                    _stepSize = sampleInfo.Item4;
                    minMaxSlider.onValueChanged.AddListener(SliderValueChanged);
                    minMaxSlider.SetLimits(sampleInfo.Item1, sampleInfo.Item2);
                    minMaxSlider.SetValues(sampleInfo.Item1, sampleInfo.Item2, false);
                    _enterListener = true;
                    return;
                }
            }
            
            DefaultSceneLoad();
        }

        private int RoundDownToNearestMultiple(int value, int multiple) {
            return value >= 0 ? (value / multiple) * multiple : ((value - multiple + 1) / multiple) * multiple;
        }
        
        private int RoundUpToNearestMultiple(int value, int multiple) {
            return value >= 0 ? ((value + multiple - 1) / multiple) * multiple : (value / multiple) * multiple;
        }

        private void SliderValueChanged(float a, float b) {
            minMaxSlider.SetValues(RoundDownToNearestMultiple((int)a, _stepSize),
                RoundUpToNearestMultiple((int)b, _stepSize), false);
        }

        /**
         * If the sample min max selection is currently active, listen to Enter (Return) key.
         */
        public void Update() {
            if (_enterListener && Input.GetKeyDown(KeyCode.Return)) {
                _enterListener = false;
                SampleInputContinuePressed();
            }
        }

        /**
         * This method gets called whenever the "Continue" button of the sample input is pressed.
         */
        public void SampleInputContinuePressed() {
            var min = (int) minMaxSlider.Values.minValue;
            var max = (int) minMaxSlider.Values.maxValue;
            userInput.SetActive(false);
            
            _dataMover.SimulationOutputXmlHandler.SetSampleTimeLimits(min, max);
            _dataMover.DReaMOutputXmlHandler?.SetSampleTimeLimits(min, max);

            StartCoroutine(AsyncLoader());
        }

        private void DefaultSceneLoad() {
            var sampleInfo = _dataMover.SimulationOutputXmlHandler.GetSampleSize();
            _dataMover.SimulationOutputXmlHandler.SetSampleTimeLimits(sampleInfo.Item1, sampleInfo.Item2);
            _dataMover.DReaMOutputXmlHandler?.SetSampleTimeLimits(sampleInfo.Item1, sampleInfo.Item2);
            
            StartCoroutine(AsyncLoader());
        }

        private IEnumerator AsyncLoader() {
            var asyncLoad = SceneManager.LoadSceneAsync("Visualization");

            while (!asyncLoad.isDone) {
                // TODO loading bar
                yield return null;
            }
        }
    }
}