using System;
using System.Diagnostics.CodeAnalysis;
using TMPro;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.UI;
using Visualization;
using Button = UnityEngine.UI.Button;
using Slider = UnityEngine.UI.Slider;

namespace UI.Visualization {

    [Serializable]
    public struct PlaybackButtonData {
        public Sprite pauseSymbol;
        public Sprite playSymbol;
        public Sprite forwardBackwardSymbol;
    }
    
    [SuppressMessage("ReSharper", "Unity.PerformanceCriticalCodeInvocation")]
    public class PlaybackControl : MonoBehaviour {
        [Header("Text Elements")]
        public TextMeshProUGUI totalTime;
        public TMP_InputField currentTime;

        [Header("Button Elements")]
        public Slider timeSlider;
        public Button playPause;
        public Button playDirection;
        
        [Header("Button Info")] 
        public PlaybackButtonData buttonData;
        
        private VisualizationMaster _visualizationMaster;
        private RectTransform _selfTransform;
        
        public event EventHandler<bool> PauseStatusChanged;
        public event EventHandler<bool> PlayBackwardsStatusChanged;

        public bool Disable { get; set; }

        public void FindAll() {
            _visualizationMaster = FindObjectOfType<VisualizationMaster>();

            currentTime.onValueChanged.AddListener(TimeInput);
            playPause.onClick.AddListener(PlayPauseChanged);
            playDirection.onClick.AddListener(PlayDirectionChanged);
            timeSlider.onValueChanged.AddListener(TimeSliderValueChanged);

            _selfTransform = GetComponent<RectTransform>();
        }

        private void TimeInput(string input) {
            _visualizationMaster.Pause = true;
            int newTime;
            try {
                newTime = int.Parse(input);
            } catch (Exception) {
                return;
            }

            if (newTime < _visualizationMaster.MinSampleTime) {
                currentTime.SetTextWithoutNotify(_visualizationMaster.MinSampleTime + "");
                newTime = _visualizationMaster.MinSampleTime;
            }

            if (newTime > _visualizationMaster.MaxSampleTime) {
                currentTime.SetTextWithoutNotify(_visualizationMaster.MaxSampleTime + "");
                newTime = _visualizationMaster.MaxSampleTime;
            }

            _visualizationMaster.CurrentTime = newTime;
            if (_visualizationMaster.Pause) {
                _visualizationMaster.SmallUpdate();
            }
        }

        private void Update() {
            if (Disable) return;
            
            if (Input.GetKeyUp(KeyCode.Space)) {
                PlayPauseChanged();
            }

            if (Input.GetKeyUp(KeyCode.R)) {
                PlayDirectionChanged();
            }
        }

        public void UpdateCurrentTime(int current) {
            currentTime.SetTextWithoutNotify(current + "");
            LayoutRebuilder.ForceRebuildLayoutImmediate(_selfTransform);
            timeSlider.SetValueWithoutNotify(current);
        }

        public void SetTotalTime(int min, int max) {
            totalTime.text = max + "";
            timeSlider.maxValue = max;
            timeSlider.minValue = min;
        }

        private void PlayPauseChanged() {
            _visualizationMaster.Pause = !_visualizationMaster.Pause;
            playPause.GetComponentInChildren<SVGImage>().sprite = _visualizationMaster.Pause
                ? buttonData.playSymbol
                : buttonData.pauseSymbol;
            PauseStatusChanged?.Invoke(this, _visualizationMaster.Pause);
        }

        private void PlayDirectionChanged() {
            _visualizationMaster.PlayBackwards = !_visualizationMaster.PlayBackwards;
            playDirection.transform.SetLocalPositionAndRotation(playDirection.transform.localPosition, Quaternion.Euler(0, 0, _visualizationMaster.PlayBackwards ? 0 : 180));
            PlayBackwardsStatusChanged?.Invoke(this, _visualizationMaster.PlayBackwards);
        }

        private void TimeSliderValueChanged(float current) {
            _visualizationMaster.CurrentTime = Mathf.RoundToInt(current);
            if (_visualizationMaster.Pause) {
                _visualizationMaster.SmallUpdate();
            }
        }
    }
}