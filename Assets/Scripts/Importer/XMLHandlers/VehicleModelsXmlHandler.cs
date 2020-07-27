using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using Visualization;
using Visualization.Agents;

namespace Importer.XMLHandlers {
    public class VehicleModelsXmlHandler : XmlHandler {
        public override string GetName() {
            return "Veh. Models";
        }

        public override void StartImport() {
            if (xmlDocument.Root == null) return;

            ImportAgentModels();
        }

        private void ImportAgentModels() {
            var catalog = xmlDocument.Root?.Element("Catalog") ??
                          throw new ArgumentMissingException("AgentModelsCatalog has not <Catalog> entry.");

            foreach (var vehicle in catalog.Elements("Vehicle")) {
                var name = vehicle.Attribute("name")?.Value ?? "-1";
                if (name == "-1") continue;
                
                // chassis information 
                var boundingBox = vehicle.Element("BoundingBox");
                var center = boundingBox?.Element("Center");
                var dimension = boundingBox?.Element("Dimension");

                var centerPoint =
                    new Vector3(
                        float.Parse(center?.Attribute("x")?.Value ?? "0",
                            CultureInfo.InvariantCulture.NumberFormat),
                        0,
                        float.Parse(center?.Attribute("y")?.Value ?? "0",
                            CultureInfo.InvariantCulture.NumberFormat));

                var width = float.Parse(dimension?.Attribute("width")?.Value ?? "2",
                    CultureInfo.InvariantCulture.NumberFormat);
                var length = float.Parse(dimension?.Attribute("length")?.Value ?? "5",
                    CultureInfo.InvariantCulture.NumberFormat);
                var height = float.Parse(dimension?.Attribute("width")?.Value ?? "1.8",
                    CultureInfo.InvariantCulture.NumberFormat);
                
                // wheel information
                var wheelDiameter =
                    float.Parse(vehicle.Element("Axles")?.Element("Front")?.Attribute("wheelDiameter")?.Value ?? "0.65",
                        CultureInfo.InvariantCulture.NumberFormat);

                var info = new VehicleModelInformation {
                    Width = width,
                    Length = length,
                    Height = height,
                    Center = centerPoint,
                    WheelDiameter = wheelDiameter
                };
                
                VisualizationMaster.VehicleModelCatalog.Add(name, info);
            }
        }

        public override string GetDetails() {
            return "...";
        }
    }
}