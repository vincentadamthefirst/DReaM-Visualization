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
        
        private RectTransform _selfTransform;
        
        public event EventHandler<bool> PauseStatusChanged;
        public event EventHandler<bool> PlayBackwardsStatusChanged;

        public bool Disable { get; set; }

        public void FindAll() {
            currentTime.onValueChanged.AddListener(TimeInput);
            playPause.onClick.AddListener(PlayPauseChanged);
            playDirection.onClick.AddListener(PlayDirectionChanged);
            timeSlider.onValueChanged.AddListener(TimeSliderValueChanged);

            _selfTransform = GetComponent<RectTransform>();
        }

        private void TimeInput(string input) {
            VisualizationMaster.Instance.Pause = true;
            int newTime;
            try {
                newTime = int.Parse(input);
            } catch (Exception) {
                return;
            }

            if (newTime < VisualizationMaster.Instance.MinSampleTime) {
                currentTime.SetTextWithoutNotify(VisualizationMaster.Instance.MinSampleTime + "");
                newTime = VisualizationMaster.Instance.MinSampleTime;
            }

            if (newTime > VisualizationMaster.Instance.MaxSampleTime) {
                currentTime.SetTextWithoutNotify(VisualizationMaster.Instance.MaxSampleTime + "");
                newTime = VisualizationMaster.Instance.MaxSampleTime;
            }

            VisualizationMaster.Instance.CurrentTime = newTime;
            if (VisualizationMaster.Instance.Pause) {
                VisualizationMaster.Instance.SmallUpdate();
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
            VisualizationMaster.Instance.Pause = !VisualizationMaster.Instance.Pause;
            playPause.GetComponentInChildren<SVGImage>().sprite = VisualizationMaster.Instance.Pause
                ? buttonData.playSymbol
                : buttonData.pauseSymbol;
            PauseStatusChanged?.Invoke(this, VisualizationMaster.Instance.Pause);
        }

        private void PlayDirectionChanged() {
            VisualizationMaster.Instance.PlayBackwards = !VisualizationMaster.Instance.PlayBackwards;
            playDirection.transform.SetLocalPositionAndRotation(playDirection.transform.localPosition, Quaternion.Euler(0, 0, VisualizationMaster.Instance.PlayBackwards ? 0 : 180));
            PlayBackwardsStatusChanged?.Invoke(this, VisualizationMaster.Instance.PlayBackwards);
        }

        private void TimeSliderValueChanged(float current) {
            VisualizationMaster.Instance.CurrentTime = Mathf.RoundToInt(current);
            if (VisualizationMaster.Instance.Pause) {
                VisualizationMaster.Instance.SmallUpdate();
            }
        }
    }
}