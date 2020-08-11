using Evaluation;
using Importer.XMLHandlers;
using UnityEngine;
using Visualization.OcclusionManagement;

namespace UI.Main_Menu {
    public class DataMover : MonoBehaviour {

        // the options that get passed from the main menu to the visualization
        public OcclusionManagementOptions occlusionManagementOptions;
        
        // the current evaluationType (if any)
        public QualitativeEvaluationType QualitativeEvaluationType { get; set; }
        
        // the type of fps test to be performed (none if there should be none)
        public QuantitativeEvaluationType QuantitativeEvaluationTypeType { get; set; }
        
        // the current tester (if any)
        public string EvaluationPersonString { get; set; }

        public SceneryXmlHandler SceneryXmlHandler { get; set; }
        
        public VehicleModelsXmlHandler VehicleModelsXmlHandler { get; set; }
        
        public PedestrianModelsXmlHandler PedestrianModelsXmlHandler { get; set; }
        
        public SimulationOutputXmlHandler SimulationOutputXmlHandler { get; set; }
        
        public void Start() {
            DontDestroyOnLoad(this);
        }
    }
}