using System;
using System.Collections.Generic;
using System.Linq;
using Scenery;
using UI;
using UnityEngine;
using Utils;
using Visualization.Agents;

namespace Visualization.OcclusionManagement {
    public class TargetController : MonoBehaviour {

        public AgentCard agentCardPrefab;
        
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
        private RectTransform _agentCardHolder;

        public void SetMenuOpen(bool value) {
            _settingsOpen = value;
        }

        public void FindAll() {
            _layerMask = LayerMask.GetMask("agent_targets", "agents_base");
            _extendedCamera = FindObjectOfType<ExtendedCamera>();
            _agentOcclusionManager = FindObjectOfType<AgentOcclusionManager>();
            _agentCardHolder = transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<RectTransform>();
            _sidePanelEnabled = PlayerPrefs.GetInt("app_wip_features") > 0;
        }

        /// <summary>
        /// Checks for mouse clicks.
        /// </summary>
        private void Update() {
            CheckMouseClick();
        }

        /// <summary>
        /// Used to find all agents in the scene and display their information.
        /// </summary>
        public void Prepare() {
            var agents = FindObjectsOfType<Agent>().ToList().OrderBy(x => int.Parse(x.Id));
            foreach (var agent in agents) {
                Debug.LogError("THE AGENTCARD PREFAB DOES NOT EXIST YET, SKIPPING");
                var agentCard = Resources.Load<AgentCard>("Prefabs/UI/Visualization/AgentCard");
                var newObject = Instantiate(agentCard, _agentCardHolder);
                newObject.Parent = _agentCardHolder;
                newObject.Agent = agent;
                // register necessary events for this card
                newObject.CardClicked += HandleCardClick;
                agent.TargetStatusChanged += newObject.TargetStatusChanged;
                _agentCards.Add(newObject, agent);
                _agentCardsReverse.Add(agent, newObject);
            }
        }

        /// <summary>
        /// Sends a ray out into the scene and retrieves the first agent it hits (if there is one). Changes this agents
        /// target status.
        /// </summary>
        private void CheckMouseClick() {
            if (_settingsOpen) return;
            if (!_extendedCamera) return;
            if (!Input.GetMouseButtonDown(0)) return;

            var ray = _extendedCamera.Camera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit, _layerMask)) return;

            var hitElement = _agentOcclusionManager.AllColliders[hit.collider];
            if (!hitElement.GetType().IsSubclassOf(typeof(Agent))) return;
            
            hitElement.Clicked();
            
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) {
                // snap the camera to the agent
                _extendedCamera.CameraController.LockedOnAgent = (Agent) hitElement;
                _extendedCamera.CameraController.LockedOnAgentIsSet = true;
                return;
            }

            if (hitElement is TargetableElement element) {
                if (element.IsTarget) {
                    element.IsTarget = false;
                    
                    // TODO enable side Panel writing
                    
                    // if (_sidePanelEnabled && (hitElement as Agent).WriteToSidePanel) {
                    //     (hitElement as Agent).WriteToSidePanel = false;
                    //     if (Targets.Count > 0) {
                    //         var newSidePanelWriter = ((Agent)Targets.Last());
                    //         newSidePanelWriter.WriteToSidePanel = true;
                    //         _sidePanelAgent = newSidePanelWriter;
                    //     }
                    // }
                } else {
                    element.IsTarget = true;
                    
                    //
                    //
                    // if (_sidePanelEnabled) {
                    //     if (_sidePanelAgent != null)
                    //         _sidePanelAgent.WriteToSidePanel = false;
                    //     (hitElement as Agent).WriteToSidePanel = true;
                    //     _sidePanelAgent = hitElement as Agent;
                    // }
                }
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
            
            var clicked = _agentCards[card];
            clicked.IsTarget = !_agentOcclusionManager.Targets.Contains(clicked);
        }
    }
}