namespace Importer.XMLHandlers {
    public interface IAgentMovementImporter {

        string GetApplicableFileEndings();

        string GetImporterName();

        // add the ui controller so that the importer can use UI components...
        ImporterStatus SetFilePath(string path);

        FileInformation GetFileInformation();

        MovementInformation GetAgentMovement();
    }

    // set of status reports that can occur during import
    public enum ImporterStatus {
        None = 0,
        ErrorUnspecified = 400,
        ErrorFileNotFound = 401,
        ErrorParsing = 402,
        
    }

    // container to store all information related to the file
    public struct FileInformation {
        // UI information
        public string title;
        public string subTitle;
        public string additionalNote;
        
        // run information
        public int agentAmount;
        public int eventAmount;
        public uint recordedTimeMs;
    }

    // container to store all actions / movement from agents
    public struct MovementInformation {
        
    }
}