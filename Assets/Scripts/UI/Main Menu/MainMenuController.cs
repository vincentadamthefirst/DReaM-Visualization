using Importer.XMLHandlers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI.Main_Menu {
    public class MainMenuController : MonoBehaviour {

        public Button startButton;

        public FileImportController fileImportController;

        private DataMover _dataMover;

        public void Start() {
            startButton.onClick.AddListener(StartButtonClicked);
            _dataMover = FindObjectOfType<DataMover>();
        }

        private void StartButtonClicked() {
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
    }
}