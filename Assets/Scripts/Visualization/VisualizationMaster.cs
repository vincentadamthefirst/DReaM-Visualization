using System;
using System.Collections.Generic;
using UnityEngine;
using Visualization.Agents;

namespace Visualization {
    public class VisualizationMaster : MonoBehaviour {

        public List<Agent> Agents { get; } = new List<Agent>();

        public AgentDesigns agentDesigns;
        
        public int MaxSampleTime { get; set; }
        
        public int CurrentTime { get; set; }
        
        public bool PlayBackwards { get; set; }

        public bool Pause { get; set; } = true;

        public void PrepareAgents() {
            Agents.ForEach(a => a.Prepare());
        }

        public VehicleAgent InstantiateVehicleAgent(string modelType) {
            var na = Instantiate(agentDesigns.vehiclePrefab, transform, true);
            var model = Instantiate(agentDesigns.GetAgentModel(AgentType.Vehicle, modelType).model, na.transform, true);
            Agents.Add(na);
            na.Model = model;
            
            return na;
        }
        
        public PedestrianAgent InstantiatePedestrian() {
            var na = Instantiate(agentDesigns.pedestrianPrefab, transform, true);
            Agents.Add(na);

            return na;
        }

        private void Update() {
            if (Input.GetKeyUp(KeyCode.Space)) {
                if (Pause)
                    Pause = false;
                else {
                    Agents.ForEach(a => a.Pause());
                    Pause = true;
                }
            }

            if (Input.GetKeyUp(KeyCode.Backspace)) {
                PlayBackwards = !PlayBackwards;
            }
            
            if (Pause) return;
            
            CurrentTime = PlayBackwards
                ? CurrentTime - Mathf.RoundToInt(Time.deltaTime * 1000f)
                : CurrentTime + Mathf.RoundToInt(Time.deltaTime * 1000f);

            if (CurrentTime < 0) CurrentTime = 0;
            else if (CurrentTime > MaxSampleTime) CurrentTime = MaxSampleTime;

            foreach (var agent in Agents) {
                agent.UpdateForTimeStep(CurrentTime);
            }
        }
    }
}