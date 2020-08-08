using System;
using Evaluation;
using Importer.XMLHandlers;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Visualization.OcclusionManagement;

namespace UI.Main_Menu {
    public class MainMenuController : MonoBehaviour {

        // dropdown for selecting the evaluation step
        public TMP_Dropdown evaluationStepDropdown;

        // input field for the tester name
        public TMP_InputField testerInputField;

        // the dropdown for the fps test
        public TMP_Dropdown fpsDropdown;

        // the occlusion management options to be used for evaluating the program
        public OcclusionManagementOptions evaluationOptions;

        private Button _startButton;
        private Button _importButton;
        private Button _settingsButton;
        private Button _evaluationButton;

        private Transform _importWindow;
        private Transform _settingsWindow;
        private Transform _evaluationWindow;

        public FileImportController fileImportController;

        private DataMover _dataMover;

        public void Start() {
            _startButton = transform.Find("Start (Inset)").GetComponent<Button>();
            _importButton = transform.Find("Import (Inset)").GetComponent<Button>();
            _settingsButton = transform.Find("Settings (Inset)").GetComponent<Button>();
            _evaluationButton = transform.Find("Evaluation (Inset)").GetComponent<Button>();

            var parent = transform.parent;
            _importWindow = parent.Find("File Import");
            _settingsWindow = parent.Find("Settings");
            _evaluationWindow = parent.Find("Evaluation");
            
            _startButton.onClick.AddListener(StartButtonClicked);
            _importButton.onClick.AddListener(ImportButtonClicked);
            _settingsButton.onClick.AddListener(SettingsButtonClicked);
            _evaluationButton.onClick.AddListener(EvaluationButtonClicked);
            
            _dataMover = FindObjectOfType<DataMover>();
        }

        private void ImportButtonClicked() {
            _importWindow.gameObject.SetActive(true);
            _settingsWindow.gameObject.SetActive(false);
            _evaluationWindow.gameObject.SetActive(false);
        }

        private void SettingsButtonClicked() {
            _importWindow.gameObject.SetActive(false);
            _settingsWindow.gameObject.SetActive(true);
            _evaluationWindow.gameObject.SetActive(false);
        }

        private void EvaluationButtonClicked() {
            _importWindow.gameObject.SetActive(false);
            _settingsWindow.gameObject.SetActive(false);
            _evaluationWindow.gameObject.SetActive(true);
        }

        private void FpsTest() {
            evaluationOptions.occlusionHandlingMethod = OcclusionHandlingMethod.Transparency;
            
            switch ((FpsTest) fpsDropdown.value) {
                case Evaluation.FpsTest.None:
                    return;
                case Evaluation.FpsTest.Shader:
                    evaluationOptions.occlusionDetectionMethod = OcclusionDetectionMethod.Shader;
                    break;
                case Evaluation.FpsTest.RayCast:
                    evaluationOptions.occlusionDetectionMethod = OcclusionDetectionMethod.RayCast;
                    break;
                case Evaluation.FpsTest.Polygon:
                    evaluationOptions.occlusionDetectionMethod = OcclusionDetectionMethod.Polygon;
                    break;
                case Evaluation.FpsTest.Transparency:
                    evaluationOptions.occlusionDetectionMethod = OcclusionDetectionMethod.RayCast;
                    break;
                case Evaluation.FpsTest.WireFrame:
                    evaluationOptions.occlusionDetectionMethod = OcclusionDetectionMethod.RayCast;
                    evaluationOptions.occlusionHandlingMethod = OcclusionHandlingMethod.WireFrame;
                    break;
                default:
                    return;
            }
            
            // finding the necessary files
            const string basePath = "C:/Bachelor Evaluation/Quantitative/Test/";
            
            var sceneHandler = new SceneryXmlHandler();
            sceneHandler.SetFilePath(basePath + "SceneryConfiguration.xodr");
            var vehicleModelHandler = new VehicleModelsXmlHandler();
            vehicleModelHandler.SetFilePath(basePath + "VehicleModelsCatalog.xosc");
            var pedestrianModelHandler = new PedestrianModelsXmlHandler();
            pedestrianModelHandler.SetFilePath(basePath + "PedestrianModelsCatalog.xosc");
            var outHandler = new SimulationOutputXmlHandler();
            outHandler.SetFilePath(basePath + "simulationOutput.xml");

            // preparing the data for moving
            _dataMover.SceneryXmlHandler = sceneHandler;
            _dataMover.SimulationOutputXmlHandler = outHandler;
            _dataMover.VehicleModelsXmlHandler = vehicleModelHandler;
            _dataMover.PedestrianModelsXmlHandler = pedestrianModelHandler;

            _dataMover.FpsTestType = (FpsTest) fpsDropdown.value;
            
            _dataMover.occlusionManagementOptions = evaluationOptions;

            SceneManager.LoadScene(1);
        }

        private void QualitativeEvaluation() {
            // setting up any settings
            switch ((EvaluationType) evaluationStepDropdown.value) {
                case EvaluationType.OccTransparency:
                    evaluationOptions.occlusionDetectionMethod = OcclusionDetectionMethod.RayCast;
                    evaluationOptions.occlusionHandlingMethod = OcclusionHandlingMethod.Transparency;
                    break;
                case EvaluationType.OccWireFrame:
                    evaluationOptions.occlusionDetectionMethod = OcclusionDetectionMethod.RayCast;
                    evaluationOptions.occlusionHandlingMethod = OcclusionHandlingMethod.WireFrame;
                    break;
                case EvaluationType.OccShader:
                    evaluationOptions.occlusionDetectionMethod = OcclusionDetectionMethod.Shader;
                    break;
            }

            // finding the necessary files
            var basePath = "C:/Bachelor Evaluation/Test " + evaluationStepDropdown.value + "/";
            
            var sceneHandler = new SceneryXmlHandler();
            sceneHandler.SetFilePath(basePath + "SceneryConfiguration.xodr");
            var vehicleModelHandler = new VehicleModelsXmlHandler();
            vehicleModelHandler.SetFilePath(basePath + "VehicleModelsCatalog.xosc");
            var pedestrianModelHandler = new PedestrianModelsXmlHandler();
            pedestrianModelHandler.SetFilePath(basePath + "PedestrianModelsCatalog.xosc");
            var outHandler = new SimulationOutputXmlHandler();
            outHandler.SetFilePath(basePath + "simulationOutput.xml");

            // preparing the data for moving
            _dataMover.SceneryXmlHandler = sceneHandler;
            _dataMover.SimulationOutputXmlHandler = outHandler;
            _dataMover.VehicleModelsXmlHandler = vehicleModelHandler;
            _dataMover.PedestrianModelsXmlHandler = pedestrianModelHandler;

            _dataMover.EvaluationType = (EvaluationType) evaluationStepDropdown.value;
            _dataMover.EvaluationPersonString = testerInputField.text ?? "NULL";

            _dataMover.occlusionManagementOptions = evaluationOptions;

            SceneManager.LoadScene(1);
        }

        private void NormalStart() {
            var sceneryHandler = fileImportController.GetXmlHandler<SceneryXmlHandler>();
            var outputHandler = fileImportController.GetXmlHandler<SimulationOutputXmlHandler>();
            var vehicleHandler = fileImportController.GetXmlHandler<VehicleModelsXmlHandler>();
            var pedestrianHandler = fileImportController.GetXmlHandler<PedestrianModelsXmlHandler>();

            _dataMover.SceneryXmlHandler = sceneryHandler;
            _dataMover.SimulationOutputXmlHandler = outputHandler;
            _dataMover.VehicleModelsXmlHandler = vehicleHandler;
            _dataMover.PedestrianModelsXmlHandler = pedestrianHandler;

            if (sceneryHandler != null && outputHandler != null && vehicleHandler != null &&
                pedestrianHandler != null) {

                // switching to actual visualization
                SceneManager.LoadScene(1);
            }
        }

        private void StartButtonClicked() {
            if (fpsDropdown.value != 0) {
                FpsTest();
                return;
            }
            if (evaluationStepDropdown.value != 0) {
                // evaluation is selected... load necessary files based on the test to be performed
                QualitativeEvaluation();
                return;
            } 
            
            // normal scene load
            NormalStart();
        }
    }
}