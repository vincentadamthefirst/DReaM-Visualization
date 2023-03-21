using System.Globalization;
using System.Linq;
using System.Xml.XPath;
using UnityEngine;
using Visualization;
using Visualization.Agents;

namespace Importer.XMLHandlers {

    public sealed class ProfilesCatalogXmlHandler : XmlHandler {
        public void StartImport() {
            var vehCompProfile = xmlDocument.Root?.XPathSelectElement("//VehicleComponentProfile[@Type='Sensor_CAEB']");
            if (vehCompProfile == null) return;

            var sensorDirectionAttribute = vehCompProfile.XPathSelectElement("Double[@Key='sensorDirectionAngle']")?.Attribute("Value");
            var sensorDistanceAttribute = vehCompProfile.XPathSelectElement("Double[@Key='sensorDistance']")?.Attribute("Value");
            var sensorAngleAttribute = vehCompProfile.XPathSelectElement("Double[@Key='sensorOpeningAngle']")?.Attribute("Value");

            var direction = float.Parse(sensorDirectionAttribute?.Value ?? "0", CultureInfo.InvariantCulture);
            var distance = float.Parse(sensorDistanceAttribute?.Value ?? "100", CultureInfo.InvariantCulture);
            var opening = float.Parse(sensorAngleAttribute?.Value ?? "180", CultureInfo.InvariantCulture);
            
            // convert to radians
            direction *= Mathf.Deg2Rad;
            opening *= Mathf.Deg2Rad;
            
            Debug.Log("Found AEB sensor");
            Debug.Log($"Dir: {direction}, Dis: {distance}, Ope: {opening}" );

            foreach (var simulationStep in VisualizationMaster.Instance.Agents
                         .Where(agent => agent is not PedestrianAgent)
                         .Select(agent => agent.SimulationSteps.Values.OrderBy(s => s.Time).ToArray())
                         .SelectMany(samples => samples)) {
                simulationStep.SensorInformation.Add("aeb", new SensorInformation {
                    Heading = simulationStep.Rotation + direction, Distance = distance, OpeningAngle = opening
                });
            }
        }

        public override string GetName() {
            return "profiles";
        }

        public override string GetDetails() {
            return "details";
        }
    }
}