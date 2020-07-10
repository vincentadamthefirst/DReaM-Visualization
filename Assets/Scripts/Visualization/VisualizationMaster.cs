using System;
using System.Collections.Generic;
using Scenery.RoadNetwork;
using UI;
using UnityEngine;
using Visualization.Agents;

namespace Visualization {
    public class VisualizationMaster : MonoBehaviour {

        public PlaybackControl playbackControl;

        public List<Agent> Agents { get; } = new List<Agent>();

        public AgentDesigns agentDesigns;
        
        public int MaxSampleTime { get; set; }
        
        public int CurrentTime { get; set; }
        
        public bool PlayBackwards { get; set; }

        public bool Pause { get; set; } = true;

        public void PrepareAgents() {
            Agents.ForEach(a => a.Prepare());
            
            playbackControl.SetTotalTime(MaxSampleTime);
            
            // TODO remove (only for debugging)
            Agents.ForEach(a => a.SetIsTarget(true));
        }

        /// <summary>
        /// Initializes a new vehicle agent and returns a reference to it.
        /// </summary>
        /// <param name="modelType">The model type of the agent</param>
        /// <returns>The instantiated agent</returns>
        public VehicleAgent InstantiateVehicleAgent(string modelType) {
            var na = Instantiate(agentDesigns.vehiclePrefab, transform, true);
            na.RoadNetworkHolder = FindObjectOfType<RoadNetworkHolder>();
            var model = Instantiate(agentDesigns.GetAgentModel(AgentType.Vehicle, modelType).model, na.transform, true);
            Agents.Add(na);
            na.Model = model;
            
            return na;
        }
        
        /// <summary>
        /// Initializes a new pedestrian agent and returns a reference to it.
        /// </summary>
        /// <returns>The instantiated agent</returns>
        public PedestrianAgent InstantiatePedestrian() {
            var na = Instantiate(agentDesigns.pedestrianPrefab, transform, true);
            na.RoadNetworkHolder = FindObjectOfType<RoadNetworkHolder>();
            var model = Instantiate(agentDesigns.GetAgentModel(AgentType.Pedestrian, "pedestrian").model, na.transform, true);
            Agents.Add(na);
            na.Model = model;
            
            return na;
        }

        private void Update() {
            if (Pause) return;
            
            CurrentTime = PlayBackwards
                ? CurrentTime - Mathf.RoundToInt(Time.deltaTime * 1000f)
                : CurrentTime + Mathf.RoundToInt(Time.deltaTime * 1000f);

            if (CurrentTime < 0) CurrentTime = 0;
            else if (CurrentTime > MaxSampleTime) CurrentTime = MaxSampleTime;

            foreach (var agent in Agents) {
                agent.UpdateForTimeStep(CurrentTime, PlayBackwards);
            }
            
            playbackControl.UpdateCurrentTime(CurrentTime);
        }
    }
}