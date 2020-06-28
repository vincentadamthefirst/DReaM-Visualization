using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Importer.XMLHandlers;

namespace Importer {

    public class FolderImporter {
        private static readonly Dictionary<string, XmlType> FileStrings = new Dictionary<string, XmlType> {
            {"scenario", XmlType.Scenario},
            {"scenery", XmlType.Scenery},
            {"vehiclemodels", XmlType.VehicleModels},
            {"pedestrianmodels", XmlType.PedestrianModels},
            {"simulationoutput", XmlType.SimulationOutput}
        };

        private static readonly Dictionary<XmlType, Type> Handlers = new Dictionary<XmlType, Type> {
            {XmlType.Scenario, typeof(ScenarioXmlHandler)},
            {XmlType.Scenery, typeof(SceneryXmlHandler)},
            {XmlType.PedestrianModels, typeof(PedestrianModelsXmlHandler)},
            {XmlType.VehicleModels, typeof(VehicleModelsXmlHandler)},
            {XmlType.SimulationOutput, typeof(SimulationOutputXmlHandler)}
        };

        private static readonly string[] Extensions = new[] {".xodr", ".xml", ".xosc"};

        private int[] _supportedFilesCount;
        private List<XmlHandler> _xmlHandlers;

        public List<Tuple<XmlType, XmlHandler>> GetPossibleFiles(string basePath) {
            var resultList = new List<Tuple<XmlType, XmlHandler>>();

            // checking if the main directory exists
            if (!Directory.Exists(basePath)) throw new DirectoryNotFoundException("Base directory not found.");

            var files = DirectoryFileSearchRecursive(basePath);
            foreach (var file in files) {
                var normalizedFileName = file.ToLower();
                foreach (var entry in FileStrings) {
                    var containsExtension = Extensions.Aggregate(false,
                        (current, extension) => current || normalizedFileName.Contains(extension));
                    if (!normalizedFileName.Contains(entry.Key) || !containsExtension) continue;
                    var newHandler = (XmlHandler) Activator.CreateInstance(Handlers[entry.Value]);
                    newHandler.SetFilePath(file);
                    resultList.Add(new Tuple<XmlType, XmlHandler>(entry.Value, newHandler));
                }
            }
            
            resultList = resultList.OrderByDescending(t => t.Item1).ToList();
            return resultList;
        }

        private IEnumerable<string> DirectoryFileSearchRecursive(string dir) {
            var files = Directory.GetFiles(dir).ToList();
            foreach (var d in Directory.GetDirectories(dir)) {
                files.AddRange(DirectoryFileSearchRecursive(d));
            }

            return files;
        }
    }

    [Serializable]
    public class DirectoryMissingException : Exception {
        public DirectoryMissingException() { }
        public DirectoryMissingException(string message) : base(message) { }
        public DirectoryMissingException(string message, Exception inner) : base(message, inner) { }

        protected DirectoryMissingException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }
}