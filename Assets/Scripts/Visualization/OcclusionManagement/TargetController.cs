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
        
        /// <summary>
        /// If the controller functionality should be disabled
        /// </summary>
        public bool Disable { get; set; }
        
        /// <summary>
        /// The current Targets
        /// </summary>
        public List<VisualizationElement> Targets { get; } = new List<VisualizationElement>();

        /// <summary>
        /// The collider mapping also in use in AgentOcclusionManager
        /// </summary>
        public Dictionary<Collider, VisualizationElement> ColliderMapping { get; set; } =
            new Dictionary<Collider, VisualizationElement>();

        // dictionary containing all agent cards
        private Dictionary<AgentCard, Agent> _agentCards = new Dictionary<AgentCard, Agent>();
        
        // reverse dictionary for agent cards
        private Dictionary<Agent, AgentCard> _agentCardsReverse = new Dictionary<Agent, AgentCard>();

        // the layer mask to perform raycasts for agent selection with
        private LayerMask _layerMask;
        
        // if the settings are open
        private bool _settingsOpen;

        // the extended camera script
        private ExtendedCamera _extendedCamera;

        // the occlusion manager
        private AgentOcclusionManager _agentOcclusionManager;

        // the UI element for holding all agent cards
        private RectTransform _agentCardHolder;

        public void SetSettingsOpen(bool value) {
            _settingsOpen = value;
        }

        public void FindAll() {
            _layerMask = LayerMask.GetMask("agent_targets", "agents_base");
            _extendedCamera = FindObjectOfType<ExtendedCamera>();
            _agentOcclusionManager = FindObjectOfType<AgentOcclusionManager>();
            _agentCardHolder = transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<RectTransform>();
        }

        /// <summary>
        /// Checks for mouse clicks.
        /// </summary>
        private void Update() {
            if (Disable) return;
            CheckMouseClick();
        }

        /// <summary>
        /// Used to find all agents in the scene and display their information.
        /// </summary>
        public void Prepare() {
            var agents = FindObjectsOfType<Agent>().ToList()
                .OrderBy(x => int.Parse(x.OpenDriveId));
            foreach (var agent in agents) {
                var newCard = Instantiate(agentCardPrefab, _agentCardHolder);
                newCard.Parent = _agentCardHolder;
                newCard.SetColor(agent.ColorMaterial.color);
                newCard.SetText(agent.name.Split(new [] {" ["}, StringSplitOptions.None)[0]);
                newCard.SetIsTarget(false);
                newCard.TargetController = this;
                _agentCards.Add(newCard, agent);
                _agentCardsReverse.Add(agent, newCard);
            }
        }

        /// <summary>
        /// Sends a ray out into the scene and retrieves the first agent it hits (if there is one). Changes this agents
        /// target status.
        /// </summary>
        private void CheckMouseClick() {
            if (_settingsOpen) return;
            if (!Input.GetMouseButtonDown(0)) return;

            var ray = _extendedCamera.Camera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit, _layerMask)) return;

            var hitElement = ColliderMapping[hit.collider];
            if (!hitElement.GetType().IsSubclassOf(typeof(Agent))) return;
            
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) {
                // snap the camera to the agent

                _extendedCamera.CameraController.LockedOnAgent = (Agent) hitElement;
                _extendedCamera.CameraController.LockedOnAgentIsSet = true;
                
                return;
            }
            
            if (Targets.Contains(hitElement)) {
                Targets.Remove(hitElement);
                hitElement.SetIsTarget(false);
                _agentCardsReverse[(Agent) hitElement].SetIsTarget(false);
                _agentOcclusionManager.SetTarget((Agent) hitElement, false);
            } else {
                Targets.Add(hitElement);
                hitElement.SetIsTarget(true);
                _agentCardsReverse[(Agent) hitElement].SetIsTarget(true);
                _agentOcclusionManager.SetTarget((Agent) hitElement, true);
            }
        }

        /// <summary>
        /// When a card is clicked this method is called.
        /// </summary>
        public void HandleCardClick(AgentCard card) {
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) {
                // snap the camera to the agent

                _extendedCamera.CameraController.LockedOnAgent = _agentCards[card];
                _extendedCamera.CameraController.LockedOnAgentIsSet = true;
                
                return;
            }
            
            var clicked = _agentCards[card];
            
            if (Targets.Contains(clicked)) {
                Targets.Remove(clicked);
                clicked.SetIsTarget(false);
                card.SetIsTarget(false);
                _agentOcclusionManager.SetTarget(clicked, false);
            } else {
                Targets.Add(clicked);
                clicked.SetIsTarget(true);
                card.SetIsTarget(true);
                _agentOcclusionManager.SetTarget(clicked, true);
            }
        }


        public void SetAllTargets() {
            foreach (var entry in _agentCardsReverse) {
                Targets.Add(entry.Key);
                entry.Key.SetIsTarget(true);
                entry.Value.SetIsTarget(true);
                _agentOcclusionManager.SetTarget(entry.Key, true);
            }
        }
    }
}