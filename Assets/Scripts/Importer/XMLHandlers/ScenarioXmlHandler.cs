namespace Importer.XMLHandlers {
    public class ScenarioXmlHandler : XmlHandler {
        public override string GetName() {
            return "Scenario";
        }

        public virtual void StartImport() {
            // TODO implement
        }

        public override string GetDetails() {
            return "<color=\"red\"><b>NOT SUPPORTED</b>";
        }
    }
}