using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Visualization {
    [CreateAssetMenu(menuName = "AgentDesigns")]
    public class AgentDesigns : ScriptableObject {
        [Header("AgentModels")]
        public List<AgentModel> agentModels = new();

        [Header("Materials")]
        public Material vehicleChassisBase;

        public AgentModel GetAgentModel(AgentType agentType, string vehicleModel) {
            var found = agentModels.Where(am => am.agentType == agentType && vehicleModel.Contains(am.modelName));

            var models = found as AgentModel[] ?? found.ToArray();
            if (models.Count() != 0) {
                return models.First();
            }

            return agentType == AgentType.Vehicle ? GetAgentModel(agentType, "fallback") : agentModels[0];
        }
    }

    [Serializable]
    public class AgentModel {
        public AgentType agentType;
        public string model;
        public string modelName;
    }
}