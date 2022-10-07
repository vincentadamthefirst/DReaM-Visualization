using Importer.XMLHandlers;
using UnityEngine;

namespace UI.Main_Menu {
    public class DataMover : MonoBehaviour {
        public SceneryXmlHandler SceneryXmlHandler { get; set; }
        
        public VehicleModelsXmlHandler VehicleModelsXmlHandler { get; set; }
        
        public PedestrianModelsXmlHandler PedestrianModelsXmlHandler { get; set; }
        
        public SimulationOutputXmlHandler SimulationOutputXmlHandler { get; set; }
        
        public ProfilesCatalogXmlHandler ProfilesCatalogXmlHandler { get; set; }
        
        public DReaMOutputXmlHandler DReaMOutputXmlHandler { get; set; }
        
        public void Start() {
            DontDestroyOnLoad(this);
        }
    }
}