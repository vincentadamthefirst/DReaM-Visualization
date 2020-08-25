using System;
using System.Collections;
using System.IO;
using Evaluation;
using Importer.XMLHandlers;
using SimpleFileBrowser;
using TMPro;
using UI.Main_Menu.Notifications;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Visualization.OcclusionManagement;

namespace UI.Main_Menu {
    public class MainMenuController : MonoBehaviour {

        // the notification manager
        public NotificationManager notificationManager;

        // dropdown for selecting the evaluation step
        public TMP_Dropdown qualitativeEvaluationDropdown;

        // input field for the tester name
        public TMP_InputField testerInputField;

        // the dropdown for the fps test
        public TMP_Dropdown quantitativeEvaluationDropdown;

        // the toggle for staggered check in the quantitative evaluation
        public Toggle quantitativeStaggeredToggle;

        // the occlusion management options to be used for evaluating the program
        public OcclusionManagementOptions evaluationOptions;

        public MainMenuSettingsController mainMenuSettingsController;

        public TMP_InputField evaluationFolderInput;

        public Button evaluationFolderSelect;

        private Button _startButton;
        private Button _importButton;
        private Button _settingsButton;
        private Button _evaluationButton;
        private Button _exitButton;

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
            _exitButton = transform.Find("Exit").GetComponent<Button>();

            var parent = transform.parent;
            _importWindow = parent.Find("File Import");
            _settingsWindow = parent.Find("Settings");
            _evaluationWindow = parent.Find("Evaluation");
            
            _startButton.onClick.AddListener(StartButtonClicked);
            _importButton.onClick.AddListener(ImportButtonClicked);
            _settingsButton.onClick.AddListener(SettingsButtonClicked);
            _evaluationButton.onClick.AddListener(EvaluationButtonClicked);
            _exitButton.onClick.AddListener(ExitButtonClicked);
            
            evaluationFolderSelect.onClick.AddListener(OpenFileBrowser);
            
            _dataMover = FindObjectOfType<DataMover>();

            if (PlayerPrefs.HasKey("evalFolder")) {
                evaluationFolderInput.SetTextWithoutNotify(PlayerPrefs.GetString("evalFolder"));
            }
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

        private void ExitButtonClicked() {
            Application.Quit();
        }
        
        private IEnumerator ShowLoadDialogCoroutine() {
            yield return FileBrowser.WaitForLoadDialog(true, false,
                evaluationFolderInput.text.Replace(" ", "") == "" ? null : evaluationFolderInput.text,
                "Select Base Folder");

            if (!FileBrowser.Success) yield break;
            evaluationFolderInput.SetTextWithoutNotify(FileBrowser.Result.Length > 0 ? FileBrowser.Result[0] : "");
        }

        private void OpenFileBrowser() {
            StartCoroutine(ShowLoadDialogCoroutine());
        }

        private void FpsTest() {
            if (!Directory.Exists(evaluationFolderInput.text)) {
                notificationManager.ShowNotification(NotificationType.Error,
                    "[Quantitative Evaluation] The Base Folder does not exist at the specified location!");
                return;
            }
            
            evaluationOptions.occlusionHandlingMethod = OcclusionHandlingMethod.Transparency;
            evaluationOptions.labelLocation = LabelLocation.Screen;
            
            switch ((QuantitativeEvaluationType) quantitativeEvaluationDropdown.value) {
                case QuantitativeEvaluationType.None:
                    return;
                case QuantitativeEvaluationType.Shader:
                    evaluationOptions.occlusionDetectionMethod = OcclusionDetectionMethod.Shader;
                    break;
                case QuantitativeEvaluationType.RayCast:
                    evaluationOptions.occlusionDetectionMethod = OcclusionDetectionMethod.RayCast;
                    break;
                case QuantitativeEvaluationType.Polygon:
                    evaluationOptions.occlusionDetectionMethod = OcclusionDetectionMethod.Polygon;
                    break;
                case QuantitativeEvaluationType.Transparency:
                    evaluationOptions.occlusionDetectionMethod = OcclusionDetectionMethod.RayCast;
                    break;
                case QuantitativeEvaluationType.WireFrame:
                    evaluationOptions.occlusionDetectionMethod = OcclusionDetectionMethod.RayCast;
                    evaluationOptions.occlusionHandlingMethod = OcclusionHandlingMethod.WireFrame;
                    break;
                case QuantitativeEvaluationType.LabelScene:
                    evaluationOptions.occlusionDetectionMethod = OcclusionDetectionMethod.RayCast;
                    evaluationOptions.labelLocation = LabelLocation.World;
                    break;
                case QuantitativeEvaluationType.LabelScreen:
                    evaluationOptions.occlusionDetectionMethod = OcclusionDetectionMethod.RayCast;
                    evaluationOptions.labelLocation = LabelLocation.Screen;
                    break;
                case QuantitativeEvaluationType.Nothing:
                    evaluationOptions.occlusionDetectionMethod = OcclusionDetectionMethod.RayCast;
                    break;
                default:
                    return;
            }

            evaluationOptions.staggeredCheck = quantitativeStaggeredToggle.isOn;
            
            // finding the necessary files
            string basePath = evaluationFolderInput.text + "/Quantitative/Test/";
            
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

            _dataMover.QuantitativeEvaluationTypeType = (QuantitativeEvaluationType) quantitativeEvaluationDropdown.value;
            
            _dataMover.occlusionManagementOptions = evaluationOptions;

            SceneManager.LoadScene(1);
        }

        private void QualitativeEvaluation() {
            if (!Directory.Exists(evaluationFolderInput.text)) {
                notificationManager.ShowNotification(NotificationType.Error,
                    "[Qualitative Evaluation] The Base Folder does not exist at the specified location!");
                return;
            }
            
            evaluationOptions.occlusionDetectionMethod = OcclusionDetectionMethod.RayCast;
            evaluationOptions.occlusionHandlingMethod = OcclusionHandlingMethod.Transparency;
            evaluationOptions.labelLocation = LabelLocation.Screen;
            
            // setting up any settings
            switch ((QualitativeEvaluationType) qualitativeEvaluationDropdown.value) {
                case QualitativeEvaluationType.LabelScene:
                    evaluationOptions.labelLocation = LabelLocation.World;
                    break;
                case QualitativeEvaluationType.OccTransparency:
                    evaluationOptions.occlusionDetectionMethod = OcclusionDetectionMethod.RayCast;
                    evaluationOptions.occlusionHandlingMethod = OcclusionHandlingMethod.Transparency;
                    break;
                case QualitativeEvaluationType.OccWireFrame:
                    evaluationOptions.occlusionDetectionMethod = OcclusionDetectionMethod.RayCast;
                    evaluationOptions.occlusionHandlingMethod = OcclusionHandlingMethod.WireFrame;
                    break;
                case QualitativeEvaluationType.OccShader:
                    evaluationOptions.occlusionDetectionMethod = OcclusionDetectionMethod.Shader;
                    break;
            }

            // finding the necessary files
            var basePath = evaluationFolderInput.text + "/Test " + qualitativeEvaluationDropdown.value + "/";
            
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

            _dataMover.QualitativeEvaluationType = (QualitativeEvaluationType) qualitativeEvaluationDropdown.value;
            _dataMover.EvaluationPersonString = testerInputField.text ?? "NULL";

            _dataMover.occlusionManagementOptions = evaluationOptions;

            SceneManager.LoadScene(1);
        }

        private void NormalStart() {
            XmlHandler sceneryHandler, outputHandler, vehicleHandler, pedestrianHandler;
            
            try {
                sceneryHandler = fileImportController.GetXmlHandler<SceneryXmlHandler>();
                outputHandler = fileImportController.GetXmlHandler<SimulationOutputXmlHandler>();
                vehicleHandler = fileImportController.GetXmlHandler<VehicleModelsXmlHandler>();
                pedestrianHandler = fileImportController.GetXmlHandler<PedestrianModelsXmlHandler>();
            } catch (ArgumentNullException e) {
                notificationManager.ShowNotification(NotificationType.Error,
                    "You need to select a Scenery (.xodr), Output (.xml), Vehicle- and PedestrianModelsCatalog (.xosc)!");
                return;
            }

            _dataMover.SceneryXmlHandler = (SceneryXmlHandler) sceneryHandler;
            _dataMover.SimulationOutputXmlHandler = (SimulationOutputXmlHandler) outputHandler;
            _dataMover.VehicleModelsXmlHandler = (VehicleModelsXmlHandler) vehicleHandler;
            _dataMover.PedestrianModelsXmlHandler = (PedestrianModelsXmlHandler) pedestrianHandler;

            mainMenuSettingsController.GetOcclusionManagementOptions();
            _dataMover.occlusionManagementOptions = mainMenuSettingsController.occlusionManagementOptions;

            if (sceneryHandler != null && outputHandler != null && vehicleHandler != null &&
                pedestrianHandler != null) {

                // switching to actual visualization
                SceneManager.LoadScene(1);
            }
        }

        private void StartButtonClicked() {
            PlayerPrefs.SetString("evalFolder", evaluationFolderInput.text);
            PlayerPrefs.Save();
            
            if (quantitativeEvaluationDropdown.value != 0) {
                FpsTest();
                return;
            }
            if (qualitativeEvaluationDropdown.value != 0) {
                // evaluation is selected... load necessary files based on the test to be performed
                QualitativeEvaluation();
                return;
            } 
            
            // normal scene load
            NormalStart();
        }
    }
}