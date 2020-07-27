using System.Collections.Generic;
using UnityEngine;

namespace Importer.XMLHandlers {
    public class ScenarioXmlHandler : XmlHandler {
        public override string GetName() {
            return "Scenario";
        }

        public override void StartImport() {
            // TODO implement
        }

        public override string GetDetails() {
            return "<color=\"red\"><b>NOT SUPPORTED</b>";
        }
    }
}