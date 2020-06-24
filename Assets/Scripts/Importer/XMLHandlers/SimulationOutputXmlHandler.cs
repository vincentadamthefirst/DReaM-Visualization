using System.Collections.Generic;
using UnityEngine;

namespace Importer.XMLHandlers {
    public class SimulationOutputXmlHandler : XmlHandler {
        public override string GetName() {
            return "SimulationOutputXmlHandler";
        }

        public override void StartImport() {
            //throw new System.NotImplementedException();
        }

        public override List<GameObject> GetInfoFields() {
            throw new System.NotImplementedException();
        }
    }
}