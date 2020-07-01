﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Visualization.Agents;

namespace Visualization {
    [CreateAssetMenu(menuName = "AgentDesigns")]
    public class AgentDesigns : ScriptableObject {

        [Header("Prefabs")] 
        public PedestrianAgent pedestrianPrefab;
        public VehicleAgent vehiclePrefab;
        
        [Header("AgentModels")]
        public List<AgentModel> agentModels = new List<AgentModel>();

        public AgentModel GetAgentModel(AgentType agentType, string vehicleModel) {
            var found = agentModels.Where(am => am.agentType == agentType && vehicleModel.Contains(am.modelName));

            var models = found as AgentModel[] ?? found.ToArray();
            if (models.Count() != 0) {
                return models.First();
            } else {
                return null;
                // TODO return basic cube
            }
        }
    }

    [Serializable]
    public class AgentModel {
        public AgentType agentType;
        public GameObject model;
        public string modelName;
    }
}