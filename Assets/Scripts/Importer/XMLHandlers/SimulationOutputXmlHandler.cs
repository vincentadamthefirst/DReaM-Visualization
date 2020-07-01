using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using UnityEngine;
using Visualization;
using Visualization.Agents;

namespace Importer.XMLHandlers {
    public class SimulationOutputXmlHandler : XmlHandler {

        private Dictionary<int, XmlAgent> _xmlAgents;

        public VisualizationMaster visualizationMaster;

        private int _maxSampleTime = int.MinValue;

        public override string GetName() {
            return "SimulationOutputXmlHandler";
        }

        public override void StartImport() {
            _xmlAgents = new Dictionary<int, XmlAgent>();
            
            if (xmlDocument.Root == null) return; // TODO Error handling
            
            ParseXmlAgents();
            CreateAgents();
            ParseAgentData();

            visualizationMaster.MaxSampleTime = _maxSampleTime;
        }

        public override List<GameObject> GetInfoFields() {
            throw new System.NotImplementedException();
        }

        private void CreateAgents() {
            foreach (var xmlAgent in _xmlAgents.Values) {
                switch (xmlAgent.AgentType) {
                    case AgentType.Pedestrian:
                        xmlAgent.ActualAgent = visualizationMaster.InstantiatePedestrian();
                        break;
                    default:
                        xmlAgent.ActualAgent = visualizationMaster.InstantiateVehicleAgent(xmlAgent.ModelType);
                        break;
                }
            }
        }

        private void ParseAgentData() {
            if (xmlDocument.Root == null) return;
            var samples = xmlDocument.Root.Element("RunResults")?.Element("RunResult")
                              ?.Element("Cyclics")?.Element("Samples")?.Elements("Sample") ??
                          throw new ArgumentMissingException("Samples not given correctly.");
                
            foreach (var sample in samples) {
                foreach (var xmlAgent in _xmlAgents.Values) {
                    var st = new SimulationStep();
                    
                    if (!ParseSampleBaseValues(st, xmlAgent, sample)) continue;
                    
                    switch (xmlAgent.AgentType) {
                        case AgentType.Vehicle:
                            ParseVehicleSampleValues(st, xmlAgent, sample);
                            break;
                        case AgentType.Pedestrian:
                            ParsePedestrianSampleValues(st, xmlAgent, sample);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    
                    xmlAgent.ActualAgent.SimulationSteps.Add(st.Time, st);
                }
            }
        }

        private void ParseVehicleSampleValues(SimulationStep step, XmlAgent agent, XElement sample) {
            // TODO implement
            var info = new AdditionalVehicleInformation();
            step.AdditionalInformation = info;
        }

        private void ParsePedestrianSampleValues(SimulationStep step, XmlAgent agent, XElement sample) {
            // TODO implement
        }

        private void ParseXmlAgents() {
            if (xmlDocument.Root == null) return;
            var result = xmlDocument.Root.Element("RunResults")?.Element("RunResult") ??
                         throw new ArgumentMissingException("Simulation Output contains no RunResult.");

            var agents = result.Element("Agents")?.Elements("Agent") ??
                         throw new ArgumentMissingException("Simulation Output contains no Agents.");
            var header = result.Element("Cyclics")?.Element("Header") ??
                         throw new ArgumentMissingException("Simulation Output contains no Cyclics.");

            foreach (var agent in agents) {
                var newAgent = new XmlAgent();
                newAgent.Id = int.Parse(agent.Attribute("Id")?.Value ??
                                        throw new ArgumentMissingException("Agent has no id!"));
                newAgent.AgentGroup = agent.Attribute("AgentTypeGroupName")?.Value ?? "none";
                newAgent.AgentTypeString = agent.Attribute("AgentTypeName")?.Value ?? "none";
                newAgent.ModelType = agent.Attribute("VehicleModelType")?.Value ?? "none";

                newAgent.AgentType = newAgent.AgentTypeString.ToLower() == "pedestrian"
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
            }
        }

        private bool ParseSampleBaseValues(SimulationStep step, XmlAgent agent, XElement sample) {
            var sampleTime = int.Parse(sample.Attribute("Time")?.Value ??
                                       throw new ArgumentMissingException("A sample has no time value."));
            var sampleString = string.Concat(sample.Nodes());
            var sampleSplit = sampleString.Split(new[] {","}, StringSplitOptions.None);

            step.Time = sampleTime;
            if (sampleTime > _maxSampleTime) _maxSampleTime = sampleTime;

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

            var posX = float.Parse(posXString, CultureInfo.InvariantCulture.NumberFormat);
            var posY = float.Parse(posYString, CultureInfo.InvariantCulture.NumberFormat);
            
            step.Position = new Vector2(posX, posY);
            return true;
        }

        private class XmlAgent {
            public int Id { get; set; }
            public string AgentGroup { get; set; }
            
            public string AgentTypeString { get; set; }
            
            public AgentType AgentType { get; set; }
            
            public string ModelType { get; set; }
            
            public Agent ActualAgent { get; set; }
            
            public Dictionary<string, int> ValuePositions { get; private set; }

            public XmlAgent() {
                ValuePositions = new Dictionary<string, int>();
            }
        }
    }
}