using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Xml.Linq;
using Visualization;
using Version = Utils.VersionSystem.Version;

namespace Importer.XMLHandlers {

    public abstract class XmlHandler {
        private Version _fileVersion;
        private string _filePath;
        protected XDocument xmlDocument;

        public void SetFilePath(string path) {
            _filePath = path;
            xmlDocument = XDocument.Load(_filePath);
        }

        public string GetFilePath() {
            return _filePath;
        }

        public abstract string GetName();

        public abstract string GetDetails();
        
        protected static float GetFloat(XElement element, string name, float fallback = 0) {
            return float.Parse(element.Attribute(name)?.Value ?? $"{fallback}", CultureInfo.InvariantCulture);
        }

        protected static string GetString(XElement element, string name, string fallback = "NONE") {
            return element.Attribute(name)?.Value ?? fallback;
        }

        protected static int GetInt(XElement element, string name, int fallback = 0) {
            return int.Parse(element.Attribute(name)?.Value ?? $"{fallback}");
        }
    }

    [Serializable]
    public class ArgumentMissingException : Exception {
        public ArgumentMissingException() { }
        public ArgumentMissingException(string message) : base(message) { }
        public ArgumentMissingException(string message, Exception inner) : base(message, inner) { }

        protected ArgumentMissingException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class ArgumentUnknownException : Exception {
        public ArgumentUnknownException() { }
        public ArgumentUnknownException(string message) : base(message) { }
        public ArgumentUnknownException(string message, Exception inner) : base(message, inner) { }

        protected ArgumentUnknownException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }
}