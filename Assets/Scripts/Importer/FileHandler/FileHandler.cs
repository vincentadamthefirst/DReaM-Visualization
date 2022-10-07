using System;
using System.IO;
using System.Xml.Linq;
using Version = Utils.VersionSystem.Version;

namespace Importer.FileHandler {

    /**
     * Represents any file handler.
     */
    public abstract class FileHandler {

        protected Version fileVersion;
        protected string filePath;
        protected XDocument xml;

        public void SetFilePath(string path) {
            filePath = path;

            if (!File.Exists(path))
                throw new FileNotFoundException($"The provided file ('{path}') does not exist!");

            try {
                xml = XDocument.Load(filePath);
            } catch (Exception) {
                throw new Exception($"There was a problem reading the file ('{path}') as an XML.");
            }
        }

        /**
         * Returns a list of all data, that this file contains for the visualization.
         */
        public abstract FileData[] Satisfies();

        /**
         * Returns the type of file this handler supports.
         */
        public abstract FileType GetFileType();

        /**
         * Returns the version of this file.
         */
        public abstract Version GetFileVersion();

        /**
         * Fills the data for this FileHandler by copying it from another.
         */
        public void CopyFrom(FileHandler other) { 
               
        }
    }

    public enum FileType {
        Scenario,
        OpenDrive,
        VehicleModels,
        PedestrianModels,
        
    }

    public enum FileData {
        Unknown = 0,
        AgentPositional = 100,
        AgentSensors = 101,
        Scenery = 200,
        VehicleModels = 300,
        PedestrianModels = 301,
    }
}