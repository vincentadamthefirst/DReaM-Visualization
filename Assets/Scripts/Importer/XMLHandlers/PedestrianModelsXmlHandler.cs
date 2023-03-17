using System.Globalization;
using UnityEngine;
using Visualization;
using Visualization.Agents;

namespace Importer.XMLHandlers {
    public class PedestrianModelsXmlHandler : XmlHandler {

        public override string GetName() {
            return "ped";
        }

        public virtual void StartImport() {
            ImportPedestrianModels();
        }
        
        private void ImportPedestrianModels() {
            var catalog = xmlDocument.Root?.Element("Catalog") ??
                          throw new ArgumentMissingException("PedestrianModelsCatalog has not <Catalog> entry.");

            foreach (var vehicle in catalog.Elements("Pedestrian")) {
                var name = vehicle.Attribute("name")?.Value ?? "-1";
                if (name == "-1") continue;
                
                // body information 
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

                var width = float.Parse(dimension?.Attribute("width")?.Value ?? "0.7",
                    CultureInfo.InvariantCulture.NumberFormat);
                var length = float.Parse(dimension?.Attribute("length")?.Value ?? "0.7",
                    CultureInfo.InvariantCulture.NumberFormat);
                var height = float.Parse(dimension?.Attribute("height")?.Value ?? "1.8",
                    CultureInfo.InvariantCulture.NumberFormat);

                var info = new PedestrianModelInformation() {
                    Width = width,
                    Length = length,
                    Height = height,
                    Center = centerPoint,
                };
                
                VisualizationMaster.Instance.PedestrianModelCatalog.Add(name, info);
            }
        }

        public override string GetDetails() {
            return "...";
        }
    }
}