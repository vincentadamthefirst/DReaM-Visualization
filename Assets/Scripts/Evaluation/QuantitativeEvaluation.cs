using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Visualization.OcclusionManagement;

namespace Evaluation {
    public class QuantitativeEvaluation : MonoBehaviour {
        
        public QuantitativeEvaluationType QuantitativeEvaluationTypeType { get; set; }
        
        public ExecutionMeasurement LabelPlacementMeasurement { get; set; }
        
        public ExecutionMeasurement RoadOcclusionMeasurement { get; set; }
        
        public ExecutionMeasurement HandlingMeasurement { get; set; }
        
        public ExecutionMeasurement DetectionMeasurement { get; set; }
        
        private readonly List<int> _fpsList = new List<int>();

        private OcclusionManagementOptions _occlusionManagementOptions;

        private void Start() {
            _occlusionManagementOptions = FindObjectOfType<AgentOcclusionManager>().OcclusionManagementOptions;
        }

        private void Update() {
            _fpsList.Add(Mathf.RoundToInt(1.0f / Time.deltaTime));
        }

        private void OnDestroy() {
            var fileName = "C:/Bachelor Evaluation/Quantitative/Results/" + QuantitativeEvaluationTypeType +
                           (_occlusionManagementOptions.staggeredCheck ? "_staggered" : "_normal") + ".txt";

            if (File.Exists(fileName))
                File.Delete(fileName);
            File.Create(fileName).Dispose();

            var averageFps = _fpsList.Sum() / (float) _fpsList.Count;
            var averageExecLabel = LabelPlacementMeasurement.ElapsedTimeMs.Sum() /
                                   LabelPlacementMeasurement.ElapsedTimeMs.Count;
            var averageExecRoad = RoadOcclusionMeasurement.ElapsedTimeMs.Sum() /
                                  RoadOcclusionMeasurement.ElapsedTimeMs.Count;
            var averageExecDetection = DetectionMeasurement.ElapsedTimeMs.Sum() /
                                       DetectionMeasurement.ElapsedTimeMs.Count;
            var averageExecHandle = HandlingMeasurement.ElapsedTimeMs.Sum() /
                                    HandlingMeasurement.ElapsedTimeMs.Count;

            File.AppendAllText(fileName, "Evaluation Type: " + QuantitativeEvaluationTypeType + "\n");
            File.AppendAllText(fileName, "Averages:\n" +
                                         "\tFPS: " + averageFps + "\n" +
                                         "\tLabels: " + averageExecLabel + "\n" +
                                         "\tRoads: " + averageExecRoad + "\n" +
                                         "\tDetection: " + averageExecDetection + "\n" +
                                         "\tHandling: " + averageExecHandle + "\n");
        }
    }
}