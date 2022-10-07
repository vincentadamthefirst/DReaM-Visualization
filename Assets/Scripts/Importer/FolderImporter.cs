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
            {"vehicle", XmlType.VehicleModels},
            {"pedestrian", XmlType.PedestrianModels},
            {"dream", XmlType.DReaM},                       // needs to be placed in front of "output"
            {"output", XmlType.SimulationOutput},
            {"profiles", XmlType.ProfilesCatalog}
        };

        private static readonly Dictionary<XmlType, Type> Handlers = new Dictionary<XmlType, Type> {
            {XmlType.DReaM, typeof(DReaMOutputXmlHandler)},
            {XmlType.Scenario, typeof(ScenarioXmlHandler)},
            {XmlType.Scenery, typeof(SceneryXmlHandler)},
            {XmlType.PedestrianModels, typeof(PedestrianModelsXmlHandler)},
            {XmlType.VehicleModels, typeof(VehicleModelsXmlHandler)},
            {XmlType.SimulationOutput, typeof(SimulationOutputXmlHandler)},
            {XmlType.ProfilesCatalog, typeof(ProfilesCatalogXmlHandler)}
        };

        private static readonly string[] Extensions = {".xodr", ".xml", ".xosc"};

        private int[] _supportedFilesCount;
        private List<XmlHandler> _xmlHandlers;

        public List<Tuple<XmlType, XmlHandler>> GetPossibleFiles(string basePath) {
            var resultList = new List<Tuple<XmlType, XmlHandler>>();

            // checking if the main directory exists
            if (!Directory.Exists(basePath)) throw new DirectoryNotFoundException("Base directory not found.");

            var files = DirectoryFileSearchRecursive(basePath, 0, 10);
            foreach (var file in files) {
                var normalizedFileName = file.ToLower();
                
                // iterating over all elements in FileStrings to determine what kind of file it is
                // for this to work, each type of file needs to have a unique string in its name
                foreach (var entry in FileStrings) {
                    // retrieve the file extension
                    var containsExtension = Extensions.Aggregate(false,
                        (current, extension) => current || normalizedFileName.Contains(extension));
                    // check if the file contains a certain name and check the file extension validity
                    if (!normalizedFileName.Contains(entry.Key) || !containsExtension) continue;

                    var newHandler = (XmlHandler) Activator.CreateInstance(Handlers[entry.Value]);
                    newHandler.SetFilePath(file);
                    resultList.Add(new Tuple<XmlType, XmlHandler>(entry.Value, newHandler));
                    break;
                }
            }

            resultList = resultList.OrderByDescending(t => t.Item1).ToList();
            return resultList;
        }

        /// <summary>
        /// Searches for files in a directory in a recursive manner.
        /// </summary>
        /// <param name="dir">The directory in which to start searching</param>
        /// <param name="recursionDepth">current recursion depth, start at 0</param>
        /// <param name="maximumRecursionDepth">maximum recursion depth</param>
        /// <returns></returns>
        private IEnumerable<string> DirectoryFileSearchRecursive(string dir, int recursionDepth, int maximumRecursionDepth) {
            var files = Directory.GetFiles(dir).ToList();

            if (recursionDepth > maximumRecursionDepth) return files;
            
            foreach (var d in Directory.GetDirectories(dir)) {
                files.AddRange(DirectoryFileSearchRecursive(d, recursionDepth + 1, maximumRecursionDepth));
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