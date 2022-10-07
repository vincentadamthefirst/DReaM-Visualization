
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;
using Visualization;
using Visualization.POIs;

namespace Importer.XMLHandlers {

    public class ValueMapper {
        private readonly Dictionary<string, int> _defaultPositionMapping = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _listPositionMapping = new Dictionary<string, int>();

        private string[] _currentSplitSample;
        
        public ValueMapper(string header) {
            var split = header.Split(new[] { "," }, StringSplitOptions.None);

            for (var index = 0; index < split.Length; index++) {
                var part = split[index];
                if (part.Contains("[{")) {
                    // list element
                    var listSplit = part.Split(new[] { "[{" }, StringSplitOptions.None);
                    _listPositionMapping.Add(listSplit[0].Replace(" ", ""), index);
                } else {
                    // default element
                    _defaultPositionMapping.Add(part.Replace(" ", ""), index);
                }
            }
        }

        public void SetCurrentSample(string sample) {
            _currentSplitSample = sample.Split(',');
        }
        
        public string GetString(string name) {
            return _currentSplitSample[_defaultPositionMapping[name]];
        }
        
        public float GetFloat(string name) {
            return float.Parse(_currentSplitSample[_defaultPositionMapping[name]], CultureInfo.InvariantCulture);
        }

        public List<string> GetList(string name) {
            var list = _currentSplitSample[_listPositionMapping[name]];
            list = list.Replace("[", "").Replace("]", "");

            var listSplit = list.Split(new[] { "}{" }, StringSplitOptions.None);

            return listSplit.Select(element => element.Replace("{", "").Replace("}", "").Replace(" ", ""))
                .Where(cleaned => !string.IsNullOrEmpty(cleaned)).ToList();
        }
    }
    
    public class DReaMOutputXmlHandler : XmlHandler {
        
        private Dictionary<int, List<string>> _agentValueMapping;

        public List<IntersectionStoppingPoints> StoppingPoints = new List<IntersectionStoppingPoints>();

        public Dictionary<string, List<ConflictArea>> ConflictAreaMapping { get; } =
            new Dictionary<string, List<ConflictArea>>();

        private ValueMapper _valueMapper;

        private int _minSampleTime;
        private int _maxSampleTime;
        
        public string RunId { get; set; }

        private XElement _runResult;

        public void SetSampleTimeLimits(int min, int max) {
            _minSampleTime = min;
            _maxSampleTime = max;
        }

        public override string GetName() {
            return "dream";
        }

        public override string GetDetails() {
            if (xmlDocument.Root == null) return "<color=\"red\"><b>XML Error</b>";
        
            var supported = new Version("0.3.0");
            
            var versionInFile = xmlDocument.Root?.Attribute("SchemaVersion")?.Value;
            var versionString = "<color=\"orange\">Version unknown";

            if (versionInFile != null) {
                var fileVersion = new Version(versionInFile);
                versionString = fileVersion.CompareTo(supported) >= 0
                    ? "<color=\"green\">Version " + versionInFile
                    : "<color=\"red\">Version " + versionInFile;
            }
            
            var returnString = versionString + "<color=\"white\">";
            
            var samples = xmlDocument.Root.Element("RunResults")?.Element("RunResult")?.Element("Cyclics")?.Element("Samples");
            
            if (samples != null && samples.Elements("Sample").Count() != 0) {
                returnString += " <b>|</b> Samples: " + samples.Elements("Sample").Count();
            } else {
                returnString += " <b>|</b><color=\"red\"> Samples missing!";
            }

            return returnString;
        }

        public void StartImport() {
            if (xmlDocument.Root == null)
                throw new Exception("XML is not formatted correctly.");

            var allRunResults = xmlDocument.Root.Element("RunResults")?.Elements("RunResult");
            foreach (var runResult in allRunResults) {
                if (!GetString(runResult, "RunId").Contains(RunId)) continue;
                _runResult = runResult;
                break;
            }
            
            if (_runResult == null)
                throw new XmlException("No RunResult found!");
            
            PrepareValueMapping();
            
            ImportStoppingPoints();
            ImportConflictAreas();
            ImportSamples();
        }

        private void ImportSamples() {
            var samples = _runResult.Element("Cyclics")?.Element("Samples")?.Elements("Sample") ??
                          throw new ArgumentMissingException("Samples not given correctly.");
            
            foreach (var sample in samples) {
                var sampleTime = GetInt(sample, "time", -1);
                if (sampleTime < 0) 
                    throw new ArgumentMissingException("Negative time values are not supported.");
                if (sampleTime < _minSampleTime || sampleTime > _maxSampleTime)
                    continue;

                var sampleAgents = sample.Elements("A").ToList();

                foreach (var sampleAgent in sampleAgents) {
                    var id = GetInt(sampleAgent, "id", -1);
                    // VisualizationMaster.Agents.ForEach(x => Debug.Log(x.Id));
                    
                    var agent = VisualizationMaster.Agents.First(x => x.Id == id);

                    // if (agent == null)
                    //     Debug.Log($"Error, could not find agent {id} in samples.");
                            
                    var agentSample = string.Concat(sampleAgent.Nodes());
                    _valueMapper.SetCurrentSample(agentSample);

                    var step = agent.SimulationSteps[sampleTime];
                    var info = step.AdditionalInformation;

                    // extract basic string info
                    info.ScanAoI = _valueMapper.GetString("ScanAOI");
                    info.GlanceType = _valueMapper.GetString("GazeType");
                    info.CrossingPhase = _valueMapper.GetString("crossingPhase");

                    // extract sensor information
                    var sensorInformation = new SensorInformation {
                        Distance = _valueMapper.GetFloat("viewDistance"),
                        Heading = _valueMapper.GetFloat("ufovAngle"),
                        OpeningAngle = _valueMapper.GetFloat("openingAngle")
                    };

                    // Debug.Log($"Agent {id}: {sensorInformation.Heading} & {sensorInformation.OpeningAngle} & {sensorInformation.Distance}");

                    step.SensorInformation.Add("driver", sensorInformation);

                    // extract other agent information
                    var otherAgents = new List<Tuple<Vector2, float>>();
                    foreach (var otherAgentString in _valueMapper.GetList("otherAgents")) {
                        var split = otherAgentString.Split(new[] { "|" }, StringSplitOptions.None);
                        
                        var posX = float.Parse(split[1], CultureInfo.InvariantCulture);
                        var posY = float.Parse(split[2], CultureInfo.InvariantCulture);
                        var hdg  = float.Parse(split[3], CultureInfo.InvariantCulture);
                        otherAgents.Add(new Tuple<Vector2, float>(new Vector2(posX, posY), hdg));
                    }

                    info.OtherAgents = otherAgents.ToArray();
                }
            }
        }

        private void ImportConflictAreas() {
            var caJunctions = _runResult.Element("ConflictAreas")?.Elements("Junction") ??
                              throw new ArgumentMissingException("Simulation Output contains no Cyclics.");
            
            foreach (var junction in caJunctions) {
                var junctionId = junction.Attribute("id")?.Value.ToLower() ?? "not on junction";
                if (!ConflictAreaMapping.ContainsKey(junctionId))
                    ConflictAreaMapping.Add(junctionId, new List<ConflictArea>());
                
                var conflictAreas = junction.Elements("ConflictArea");
                foreach (var conflictArea in conflictAreas) {
                    ConflictAreaMapping[junctionId].Add(new ConflictArea {
                        startSa = GetFloat(conflictArea, "startA"),
                        endSa = GetFloat(conflictArea, "endA"),
                        startSb = GetFloat(conflictArea, "startB"),
                        endSb = GetFloat(conflictArea, "endB"),
                        roadIdA = GetString(conflictArea, "roadA"),
                        laneIdA = GetInt(conflictArea, "laneA"),
                        roadIdB = GetString(conflictArea, "roadB"),
                        laneIdB = GetInt(conflictArea, "laneB")
                    });
                }
            }
        }

        private void ImportStoppingPoints() {
            var junctions = _runResult.Element("StoppingPoints")?.Elements("Junction");

            if (junctions == null)
                return;

            foreach (var junction in junctions) {
                var intersectionId = GetString(junction, "id", "UNKNOWN");
                var isp = new IntersectionStoppingPoints
                    { IntersectionId = intersectionId, laneStoppingPoints = new List<LaneStoppingPoints>() };

                var lanes = junction.Elements("Lane");
                foreach (var lane in lanes) {
                    var lsp = new LaneStoppingPoints {
                        LaneId = GetString(lane, "id", "UNKNOWN"),
                        stoppingPoints = new List<StoppingPoint>()
                    };

                    var points = lane.Elements("Point");
                    if (!points.Any())
                        continue;

                    foreach (var stoppingPoint in points) {
                        lsp.stoppingPoints.Add(new StoppingPoint {
                            position = new Vector2(GetFloat(stoppingPoint, "posX"), GetFloat(stoppingPoint, "posY")),
                            roadId = GetString(stoppingPoint, "road"),
                            laneId = GetString(stoppingPoint, "lane"),
                            type = GetString(stoppingPoint, "type")
                        });
                    }
                    isp.laneStoppingPoints.Add(lsp);
                }
                StoppingPoints.Add(isp);
            }
        }

        private void PrepareValueMapping() {
            var header = _runResult.Element("Cyclics")?.Element("Header") ??
                         throw new ArgumentMissingException("Simulation Output contains no Cyclics.");

            _valueMapper = new ValueMapper(string.Concat(header.Nodes()));
        }
    }
}