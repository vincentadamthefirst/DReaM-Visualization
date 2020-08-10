using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Evaluation {

    /// <summary>
    /// This class is used to log the user input data for a simulation run.
    /// </summary>
    public class QualitativeEvaluation : MonoBehaviour {

        public string TestPersonId { get; set; }= "UNKNOWN";
        
        private readonly List<int> _frameRateOverTime = new List<int>();

        private int _totalKeyStrokes;

        private Stopwatch _stopwatch;

        private DateTime _startTime;

        private SimpleCameraController _simpleCameraController;

        private EvaluationMenuController _evaluationMenuController;

        public EvaluationType EvaluationType { get; set; }

        public bool Disable { get; set; }

        private void Start() {
            _stopwatch = Stopwatch.StartNew();
            _startTime = DateTime.Now;
            _simpleCameraController = FindObjectOfType<SimpleCameraController>();
            _evaluationMenuController = FindObjectOfType<EvaluationMenuController>();
        }

        private void Update() {
            if (Disable) return;
            
            // add current frames per second
            _frameRateOverTime.Add(Mathf.RoundToInt(1.0f / Time.deltaTime));

            if (Input.anyKeyDown) {
                _totalKeyStrokes++;
            }
        }

        /// <summary>
        /// Dumping all data in a file.
        /// </summary>
        private void OnDestroy() {
            _stopwatch.Stop();

            long total = 0;
            var lowestFps = int.MaxValue;
            var highestFps = int.MinValue;
            foreach (var frameRate in _frameRateOverTime) {
                total += frameRate;
                if (frameRate < lowestFps) lowestFps = frameRate;
                if (frameRate > highestFps) highestFps = frameRate;
            }

            var averageFps = total / (float) _frameRateOverTime.Count;

            var endTime = DateTime.Now.ToString("yy-MMM-dd_HH-mm-ss");
            var startTime = _startTime.ToString("yy-MMM-dd_HH-mm-ss");
            var path = "C:/Bachelor Evaluation/Results/" + TestPersonId + "_Test " + (int) EvaluationType + ".txt";
            
            File.Create(path).Dispose();

            var infoText = "Evaluation Results \n \n" +
                           "Tester:     " + TestPersonId + "\n" +
                           "Start:      " + startTime + "\n" +
                           "End:        " + endTime + "\n" +
                           "Total Time: " + _stopwatch.ElapsedMilliseconds + "ms = " +
                                            _stopwatch.ElapsedMilliseconds / 1000 + "s" + "\n \n" +
                           "Average FPS: " + Math.Round(averageFps, 3) + "\n" +
                           "Lowest  FPS: " + lowestFps + "\n" +
                           "Highest FPS: " + highestFps + "\n \n" +
                           "Total Key Strokes: " + _totalKeyStrokes + "\n" +
                           "Distance Moved:    " + _simpleCameraController.TravelledDistance + "\n" +
                           "Total Rotation:    " + _simpleCameraController.TotalRotation + "\n \n" +
                           "\n";
            
            File.AppendAllText(path, infoText);

            File.AppendAllText(path, "\nAdditional Information by Tester:\n" + _evaluationMenuController.endInput.text);
        }
    }
}