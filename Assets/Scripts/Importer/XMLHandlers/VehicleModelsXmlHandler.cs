using System.Collections.Generic;
using UnityEngine;

namespace Importer.XMLHandlers {
    public class VehicleModelsXmlHandler : XmlHandler {
        public override string GetName() {
            return "VehicleModelsXmlHandler";
        }

        public override void StartImport() {
        }

        public override List<GameObject> GetInfoFields() {
            throw new System.NotImplementedException();
        }
    }
}