using System;
using System.Collections.Generic;
using System.Linq;
using UI;
using UnityEngine;
using Utils;
using Visualization.Agents;

namespace Visualization.OcclusionManagement {
    public class TargetController : MonoBehaviour {
        private Agent _sidePanelAgent;
        private bool _sidePanelEnabled;

        private Dictionary<AgentCard, Agent> _agentCards = new();
        private Dictionary<Agent, AgentCard> _agentCardsReverse = new();

        // the layer mask to perform raycasts for agent selection with
        private LayerMask _layerMask;
        
        // if the settings are open
        private bool _settingsOpen;
        
        private ExtendedCamera _extendedCamera;
        private AgentOcclusionManager _agentOcclusionManager;
        public RectTransform agentCardHolder;

        public void SetMenuOpen(bool value) {
            _settingsOpen = value;
        }

        public void FindAll() {
            _layerMask = LayerMask.GetMask("agent_targets", "agents_base");
            _extendedCamera = FindObjectOfType<ExtendedCamera>();
            _agentOcclusionManager = FindObjectOfType<AgentOcclusionManager>();
            _sidePanelEnabled = PlayerPrefs.GetInt("app_wip_features") > 0;
        }

        /// <summary>
        /// Used to find all agents in the scene and display their information.
        /// </summary>
        public void Prepare() {
            var agents = FindObjectsOfType<Agent>().ToList().OrderBy(x => int.Parse(x.Id));
            foreach (var agent in agents) {
                var agentCard = Resources.Load<AgentCard>("Prefabs/UI/Visualization/AgentCard");
                var newAgentCard = Instantiate(agentCard, agentCardHolder);

                newAgentCard.Parent = agentCardHolder;
                newAgentCard.Agent = agent;
                // register necessary events for this card
                newAgentCard.CardClicked += HandleCardClick;
                agent.TargetStatusChanged += newAgentCard.TargetStatusChanged;
                agent.TargetStatusChanged += TargetStatusChanged;
                
                newAgentCard.Initialize();
                _agentCards.Add(newAgentCard, agent);
                _agentCardsReverse.Add(agent, newAgentCard);
            }
        }

        private void TargetStatusChanged(object element, bool value) {
            if (!element.GetType().IsSubclassOf(typeof(Agent))) return;
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) {
                // snap the camera to the agent
                _extendedCamera.CameraController.LockedOnAgent = (Agent) element;
                _extendedCamera.CameraController.LockedOnAgentIsSet = true;
            }
        }

        /// <summary>
        /// When a card is clicked this method is called (invoked).
        /// </summary>
        private void HandleCardClick(object sender, EventArgs args) {
            var card = (AgentCard) sender;
            
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) {
                // snap the camera to the agent
                _extendedCamera.CameraController.LockedOnAgent = card.Agent;
                _extendedCamera.CameraController.LockedOnAgentIsSet = true;
                return;
            }

            card.Agent.IsTarget = !card.Agent.IsTarget;
        }
    }
}