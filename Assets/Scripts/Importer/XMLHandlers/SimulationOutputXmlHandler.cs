using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using UnityEngine;
using Visualization;
using Visualization.Agents;
using Visualization.SimulationEvents;

namespace Importer.XMLHandlers {
    
    public  class XmlAgent {
        public int Id { get; set; }
        public string AgentGroup { get; set; }
            
        public string AgentTypeString { get; set; }
            
        public AgentType AgentType { get; set; }
            
        public string ModelType { get; set; }
            
        public Agent ActualAgent { get; set; }
            
        public Dictionary<string, int> ValuePositions { get; } = new Dictionary<string, int>();

        public List<string> UnknownDataNames { get; } = new List<string>();

        public Dictionary<int, SimulationStep> SimulationSteps { get; } = new Dictionary<int, SimulationStep>();
    }

    public class SimulationOutputXmlHandler : XmlHandler {

        private Dictionary<int, XmlAgent> _xmlAgents;

        private Dictionary<int, List<SimulationEvent>> _events = new Dictionary<int, List<SimulationEvent>>();

        private List<XElement> _samples;
        
        public string RunId { get; set; }

        private XElement _runResult;

        private int _minSampleTime = int.MaxValue;
        private int _maxSampleTime = int.MinValue;
        private int _sampleStep = -1;

        private static readonly List<string> KnownAttributes = new List<string>
            { "AccelerationEgo", "BrakeLight", "IndicatorState", "XPosition", "YPosition", "VelocityEgo", "YawAngle" };


        public void SetSampleTimeLimits(int min, int max) {
            _minSampleTime = min;
            _maxSampleTime = max;
        }

        public override string GetName() {
            return "out";
        }

        public List<string> GetRuns() {
            if (xmlDocument.Root == null) {
                throw new ArgumentException("XML document not correctly formatted (could not find root).");
            }

            var runResults = xmlDocument.Root.Element("RunResults")?.Elements("RunResult");

            if (runResults != null) return runResults.Select(runResult => GetString(runResult, "RunId")).ToList();
            throw new ArgumentException("XML document does not contain RunResults.");
        }

        private void FindRunResult() {
            if (xmlDocument.Root != null) {
                var allRunResults = xmlDocument.Root.Element("RunResults")?.Elements("RunResult");
                foreach (var runResult in allRunResults) {
                    if (!GetString(runResult, "RunId").Contains(RunId)) continue;
                    _runResult = runResult;
                    break;
                }
            }
        }

        public virtual void StartImport() {
            _xmlAgents = new Dictionary<int, XmlAgent>();
            
            FindRunResult();

            if (_runResult == null)
                throw new XmlException("No RunResult found!");

            GetSampleSize();
            
            VisualizationMaster.MaxSampleTime = _maxSampleTime;
            VisualizationMaster.MinSampleTime = _minSampleTime;
            VisualizationMaster.SampleStep = _sampleStep;
            
            ParseEvents();
            ParseXmlAgents();
            ParseAgentData();
            CreateAgents();
        }

        public string[] GetUnknownAttributeNames() {
            var firstAgentAttributeNames = _xmlAgents[_xmlAgents.Keys.First()].UnknownDataNames;
            return firstAgentAttributeNames.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>minTime, maxTime, sampleAmount, sampleStep</returns>
        public Tuple<int, int, int, int> GetSampleSize() {
            if (xmlDocument.Root == null) {
                throw new ArgumentException("XML document not correctly formatted (could not find root).");
            }

            FindRunResult();
            var samples = _runResult.Element("Cyclics")?.Element("Samples")?.Elements("Sample");
            if (samples == null) {
                throw new Exception("An error occured while trying to load the samples of the simulation output.");
            }

            var min = int.MaxValue;
            var max = int.MinValue;

            _samples = samples.ToList();
            var step = GetInt(_samples[1], "Time", 0) - GetInt(_samples[0], "Time", 0);
            _sampleStep = step;
            
            foreach (var time in _samples.Select(sample => GetInt(sample, "Time", 0))) {
                if (time > max) max = time;
                if (time < min) min = time;
            }

            return new Tuple<int, int, int, int>(min, max, _samples.Count, step);
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

            var statistics = _runResult.Element("RunStatistics");
            var events = _runResult.Element("Events");
            var agents = _runResult.Element("Agents");

            if (statistics == null) {
                return versionString + " <color=\"red\"> RunResults missing!";
            }

            var returnString = versionString + "<color=\"white\">";

            if (events != null) {
                returnString += " <b>|</b> Events: " + events.Elements("Event").Count();
            }

            if (statistics.Element("StopReason") != null) {
                returnString += " <b>|</b> Stop: " + String.Concat(statistics.Element("StopReason").Nodes());
            }

            if (agents != null && agents.Elements("Agent").Count() != 0) {
                returnString += " <b>|</b> Agents: " + agents.Elements("Agent").Count();
            } else {
                returnString += " <b>|</b><color=\"red\"> Agents missing!";
            }

            return returnString;
        }

        private void ParseEvents() {
            if (xmlDocument.Root == null) return;
            var events = xmlDocument.Root.XPathSelectElement("//Events")?.XPathSelectElements("Event");
            if (events == null) return;
            
            foreach (var eventElement in events) {
                var type = eventElement.Attribute("Type");
                if (type == null) continue;
                var timeStep = int.Parse(eventElement.Attribute("Time")?.Value ?? "-1");
                if (timeStep == -1) continue;

                var newEvent = new SimulationEvent {TimeStep = timeStep};
                switch (type.Value) {
                    case "AEBActive":
                        newEvent.EventType = SimulationEventType.AEBActive;
                        break;
                    case "AEBInactive":
                        newEvent.EventType = SimulationEventType.AEBInactive;
                        break;
                    default:
                        newEvent.EventType = SimulationEventType.Unsupported;
                        break;
                }

                if (newEvent.EventType == SimulationEventType.AEBActive || newEvent.EventType == SimulationEventType.AEBInactive) {
                    var agentIdAttribute = eventElement.XPathSelectElement("EventParameter[@Key='AgentId']")?.Attribute("Value");
                    if (agentIdAttribute == null) {
                        continue;
                    }

                    var id = int.Parse(agentIdAttribute.Value);
                    newEvent.AgentId = id;
                }
                
                if (_events.ContainsKey(timeStep))
                    _events[timeStep].Add(newEvent);
                else
                    _events.Add(timeStep, new List<SimulationEvent> {newEvent});
            }
        }

        private void CreateAgents() {
            foreach (var xmlAgent in _xmlAgents.Values) {
                switch (xmlAgent.AgentType) {
                    case AgentType.Pedestrian:
                        xmlAgent.ActualAgent = VisualizationMaster.InstantiatePedestrian(xmlAgent.ModelType, xmlAgent.Id);
                        xmlAgent.ActualAgent.name = "Ped #" + xmlAgent.Id + " [" + xmlAgent.ModelType + "]";
                        break;
                    default:
                        xmlAgent.ActualAgent = VisualizationMaster.InstantiateVehicleAgent(xmlAgent.ModelType, xmlAgent.Id);
                        xmlAgent.ActualAgent.name = "Car #" + xmlAgent.Id + " [" + xmlAgent.ModelType + "]";
                        break;
                }
                
                xmlAgent.ActualAgent.Id = xmlAgent.Id;
                xmlAgent.ActualAgent.OpenDriveId = xmlAgent.Id + "";
                xmlAgent.ActualAgent.SimulationSteps = xmlAgent.SimulationSteps;
            }
        }

        private void ParseAgentData() {
            if (xmlDocument.Root == null) return;

            foreach (var sample in _samples) {
                var sampleTime = GetInt(sample, "Time", -1); 
                if (sampleTime < _minSampleTime || sampleTime > _maxSampleTime)
                    continue;
                
                var sampleString = string.Concat(sample.Nodes());
                var sampleSplit = sampleString.Split(new[] {","}, StringSplitOptions.None);
                
                foreach (var xmlAgent in _xmlAgents.Values) {
                    var st = new SimulationStep();
                    
                    if (!ParseSampleBaseValues(st, xmlAgent, sampleTime, sampleSplit)) continue;
                    
                    switch (xmlAgent.AgentType) {
                        case AgentType.Vehicle:
                            ParseVehicleSampleValues(st, xmlAgent, sampleSplit);
                            break;
                        case AgentType.Pedestrian:
                            ParsePedestrianSampleValues(st, xmlAgent, sampleSplit);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    
                    xmlAgent.SimulationSteps.Add(st.Time, st);
                }
            }
        }

        private void ParseVehicleSampleValues(SimulationStep step, XmlAgent agent, string[] sampleSplit) {
            var info = new AdditionalVehicleInformation();
            step.AdditionalInformation = info;

            if (agent.ValuePositions.ContainsKey("BrakeLight") &&
                agent.ValuePositions["BrakeLight"] <= sampleSplit.Length - 1) {

                var brakeInfo = sampleSplit[agent.ValuePositions["BrakeLight"]];
                info.Brake = brakeInfo.Replace(" ", "") == "1";
            }

            if (agent.ValuePositions.ContainsKey("IndicatorState") &&
                agent.ValuePositions["IndicatorState"] <= sampleSplit.Length - 1) {

                var indicatorInfo = sampleSplit[agent.ValuePositions["IndicatorState"]];
                var indicatorState = IndicatorState.None;
                
                // ReSharper disable once ConvertSwitchStatementToSwitchExpression
                switch (indicatorInfo.Replace(" ", "")) {
                    case "0":
                        indicatorState = IndicatorState.None;
                        break;
                    case "1":
                        indicatorState = IndicatorState.Left;
                        break;
                    case "2":
                        indicatorState = IndicatorState.Right;
                        break;
                    case "3":
                        indicatorState = IndicatorState.Warn;
                        break;
                }

                info.IndicatorState = indicatorState;
            }
        }

        private void ParsePedestrianSampleValues(SimulationStep step, XmlAgent agent, string[] sampleSplit) {
            var info = new AdditionalPedestrianInformation();
            step.AdditionalInformation = info;

            if (agent.ValuePositions.ContainsKey("BrakeLight") &&
                agent.ValuePositions["BrakeLight"] <= sampleSplit.Length - 1) {

                var brakeInfo = sampleSplit[agent.ValuePositions["BrakeLight"]];
                info.Stopping = brakeInfo.Replace(" ", "") == "1";
            }
        }

        private void ParseXmlAgents() {
            if (xmlDocument.Root == null) return;
            var agents = _runResult.Element("Agents")?.Elements("Agent") ??
                         throw new ArgumentMissingException("Simulation Output contains no Agents.");
            var header = _runResult.Element("Cyclics")?.Element("Header") ??
                         throw new ArgumentMissingException("Simulation Output contains no Cyclics.");

            foreach (var agent in agents) {
                var newAgent = new XmlAgent {
                    Id = GetInt(agent, "Id", -1),
                    AgentGroup = agent.Attribute("AgentTypeGroupName")?.Value ?? "none",
                    AgentTypeString = agent.Attribute("AgentTypeName")?.Value ?? "none",
                    ModelType = agent.Attribute("VehicleModelType")?.Value ?? "none"
                };
                
                if (newAgent.Id < 0)
                    throw new ArgumentException($"Agent Ids must be non-negative integers. (agent {agent.Attribute("id")?.Value})");

                if (_xmlAgents.ContainsKey(newAgent.Id))
                    throw new ArgumentException($"Agent Ids must be unique! (agent {newAgent.Id})");

                newAgent.AgentType = newAgent.AgentTypeString.ToLower().Contains("pedestrian")
                    ? AgentType.Pedestrian
                    : AgentType.Vehicle;

                _xmlAgents.Add(newAgent.Id, newAgent);
            }

            var headerSplit = string.Concat(header.Nodes()).Split(new[] {","}, StringSplitOptions.None);

            for (var i = 0; i < headerSplit.Length; i++) {
                var elementSplit = headerSplit[i].Split(new[] {":"}, StringSplitOptions.None);
                if (elementSplit.Length != 2)
                    throw new ArgumentException("Header is not formatted correctly.");

                var agentId = int.Parse(elementSplit[0].Replace(" ", ""));
                var paramName = elementSplit[1].Replace(" ", "");
                
                _xmlAgents[agentId].ValuePositions.Add(paramName, i);

                // if the attribute is not known it will be stored as such to be displayed as text later
                if (!KnownAttributes.Contains(paramName)) {
                    _xmlAgents[agentId].UnknownDataNames.Add(paramName);
                }
            }
        }

        private bool ParseSampleBaseValues(SimulationStep step, XmlAgent agent, int sampleTime, string[] sampleSplit) {
            step.Time = sampleTime;

            var posXString = sampleSplit[agent.ValuePositions["XPosition"]].Replace(" ", "");
            var posYString = sampleSplit[agent.ValuePositions["YPosition"]].Replace(" ", "");

            if (posXString == "" || posYString == "") {
                return false;
            } 

            step.Velocity = float.Parse(sampleSplit[agent.ValuePositions["VelocityEgo"]],
                CultureInfo.InvariantCulture.NumberFormat);
            step.Acceleration = float.Parse(sampleSplit[agent.ValuePositions["AccelerationEgo"]],
                CultureInfo.InvariantCulture.NumberFormat);
            step.Rotation = float.Parse(sampleSplit[agent.ValuePositions["YawAngle"]],
                CultureInfo.InvariantCulture.NumberFormat);

            step.OnId = sampleSplit[agent.ValuePositions["Road"]].Replace(" ", "");

            var posX = float.Parse(posXString, CultureInfo.InvariantCulture.NumberFormat);
            var posY = float.Parse(posYString, CultureInfo.InvariantCulture.NumberFormat);
            
            step.Position = new Vector2(posX, posY);
            
            if (_events.ContainsKey(step.Time)) {
                var foundEvent = _events[step.Time].Find(x => x.AgentId == agent.Id);
                if (foundEvent != null) {
                    step.Events.Add(foundEvent);
                }
            }

            foreach (var unknownDataName in agent.UnknownDataNames) {
                var text = sampleSplit[agent.ValuePositions[unknownDataName]];

                if (Regex.IsMatch(text, @"[0-9]+.[0-9]+")) {
                    step.UnknownInformation.Add(float.Parse(text, CultureInfo.InvariantCulture.NumberFormat));
                } else {
                    step.UnknownInformation.Add(text);
                }
            }

            return true;
        }
    }
}