using System;
using System.Collections.Generic;
using UnityEngine;
using Visualization.Agents;

namespace Visualization {
    public class SensorManager : MonoBehaviour {
        
        public static SensorManager Instance { get; private set; }

        private Dictionary<string, List<AgentSensor>> _activeSensors = new();

        private void Awake() {
            if (Instance != null && Instance != this)
                Destroy(this);
            else {
                Instance = this;
            }
        }

        public void AgentTargetStatusChanged(object sender, bool status) {
            var agent = sender as Agent;

            if (agent == null)
                throw new ArgumentNullException(nameof(sender), "Object must be an agent.");

            if (_activeSensors.ContainsKey(agent.Id)) {
                
            }
        }
    }
}