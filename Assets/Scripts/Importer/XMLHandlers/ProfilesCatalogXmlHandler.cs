using System.Globalization;
using System.Linq;
using System.Xml.XPath;
using UnityEngine;
using Visualization;
using Visualization.Agents;

namespace Importer.XMLHandlers {

    public class ProfilesCatalogXmlHandler : XmlHandler {
        public virtual void StartImport() {
            if (xmlDocument.Root == null) return;

            var vehCompProfile = xmlDocument.Root.XPathSelectElement("//VehicleComponentProfile[@Type='Sensor_CAEB']");
            if (vehCompProfile == null) return;

            var sensorDirectionAttribute = vehCompProfile.XPathSelectElement("Double[@Key='sensorDirectionAngle']")?.Attribute("Value");
            var sensorDistanceAttribute = vehCompProfile.XPathSelectElement("Double[@Key='sensorDistance']")?.Attribute("Value");
            var sensorAngleAttribute = vehCompProfile.XPathSelectElement("Double[@Key='sensorOpeningAngle']")?.Attribute("Value");

            Debug.Log(sensorAngleAttribute?.Value ?? "yikes");
            
            var direction = float.Parse(sensorDirectionAttribute?.Value ?? "0", CultureInfo.InvariantCulture);
            var distance = float.Parse(sensorDistanceAttribute?.Value ?? "100", CultureInfo.InvariantCulture);
            var opening = float.Parse(sensorAngleAttribute?.Value ?? "180", CultureInfo.InvariantCulture);
            
            // convert to radians
            direction *= Mathf.Deg2Rad;
            opening *= Mathf.Deg2Rad;
            
            Debug.Log($"Dir: {direction}, Dis: {distance}, Ope: {opening}" );
            
            foreach (var agent in VisualizationMaster.Instance.Agents) {
                if (agent is PedestrianAgent)
                    continue;
                
                var samples = agent.SimulationSteps.Values.OrderBy(s => s.Time).ToArray();
                foreach (var simulationStep in samples) {
                    simulationStep.SensorInformation.Add("aeb", new SensorInformation {
                        Heading = simulationStep.Rotation + direction, Distance = distance, OpeningAngle = opening
                    });
                }
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