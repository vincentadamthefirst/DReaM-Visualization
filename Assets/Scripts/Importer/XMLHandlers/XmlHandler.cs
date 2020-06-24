using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Linq;
using UnityEngine;
using Version = Utils.VersionSystem.Version;

namespace Importer.XMLHandlers {

    public abstract class XmlHandler {
        private Version _fileVersion;
        protected string filePath;
        protected XDocument xmlDocument;

        public void SetFilePath(string path) {
            filePath = path;
            xmlDocument = XDocument.Load(filePath);
        }

        public string GetFilePath() {
            return filePath;
        }

        public abstract string GetName();

        public abstract void StartImport();

        public abstract List<GameObject> GetInfoFields();
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