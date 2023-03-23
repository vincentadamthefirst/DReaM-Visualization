using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Visualization.Agents;

namespace UI.SidePanel {
    public class SidePanel : MonoBehaviour {
        public RectTransform content;

        private TMP_Dropdown _agentSelection;
        private Button _openButton;

        private bool _currentOpenStatus;
        private readonly List<SidePanelEntry> _entries = new();
        private LTDescr _currentAnimation;

        private Agent _currentAgent;
        private readonly Dictionary<int, Agent> _agentDropdownIndexMapping = new();

        private void Awake() {
            _openButton = transform.Find("OpenButton").GetComponent<Button>();
            _agentSelection = transform.Find("AgentSelection").GetComponent<TMP_Dropdown>();

            _openButton.onClick.AddListener(OpenClose);
            _agentSelection.onValueChanged.AddListener(AgentChange);
        }

        private void AgentChange(int dropDownIndex) {
            if (!_agentDropdownIndexMapping.ContainsKey(dropDownIndex)) {
                Debug.LogError("Dictionary does not contain index provided by dropdown...");
                return;
            }

            SetNewAgent(_agentDropdownIndexMapping[dropDownIndex]);
        }

        public void CollectAgents() {
            _agentSelection.ClearOptions();
            var allAgents = FindObjectsOfType<Agent>();
            var agentOptionStrings = new List<string>();
            foreach (var agent in allAgents) {
                agentOptionStrings.Add($"Agent #{agent.Id} ({agent.StaticData.AgentTypeDetail.ToString()})");
                _agentDropdownIndexMapping.Add(agentOptionStrings.Count - 1, agent);
            }

            _agentSelection.AddOptions(agentOptionStrings);
            if (allAgents.Length > 0)
                SetNewAgent(allAgents[0]);
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        }

        private void SetNewAgent(Agent agent) {
            if (_currentAgent != null)
                _currentAgent.AgentUpdated -= UpdateAll;
            agent.AgentUpdated += UpdateAll;

            // remove all entries
            while (_entries.Count > 0) {
                Destroy(_entries[0].gameObject);
            }

            // create new entries
            if (agent.SimulationSteps.Values.First().AllInfo.Count == 0)
                return;

            var i = 0;
            foreach (var (valueName, value) in agent.SimulationSteps.Values.First().AllInfo) {
                var textEntryPrefab = Resources.Load<TextSidePanelEntry>("Prefabs/UI/SidePanel/TextSidePanelEntry");
                var textEntry = Instantiate(textEntryPrefab, content);
                textEntry.Initialize(valueName, i % 2 == 0 ? .4f : .2f);
                Reference<string> reference;
                if (value is float) {
                    reference = new Reference<string>(() => {
                        var f = (float)agent.DynamicData.ActiveSimulationStep.AllInfo[valueName];
                        return $"{f:F2}";
                    });
                } else if (value is List<Tuple<Vector2, float>>) {
                    reference = new Reference<string>(() => {
                        var result = "";
                        var list =
                            (List<Tuple<Vector2, float>>)agent.DynamicData.ActiveSimulationStep.AllInfo[valueName];
                        for (var index = 0; index < list.Count; index++) {
                            result += $"[{list[index].Item1:F2}, {list[index].Item2:F2}]";
                            if (index < list.Count - 2)
                                result += "<br>";
                        }

                        return result;
                    });
                } else {
                    reference = new Reference<string>(() =>
                        agent.DynamicData.ActiveSimulationStep.AllInfo[valueName].ToString());
                }

                textEntry.Reference = reference;
                _entries.Add(textEntry);
                i += 1;
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        }

        private void UpdateAll(object sender, EventArgs _) {
            _entries.ForEach(x => x.TriggerUpdate());
        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.Tab)) {
                OpenClose();
            }
        }

        private void OpenClose() {
            var moveTime = .3f;

            if (_currentAnimation != null) {
                moveTime = _currentAnimation.passed;
                LeanTween.cancel(_currentAnimation.id);
            }

            _currentOpenStatus = !_currentOpenStatus;
            LeanTween.move((RectTransform)transform, new Vector3(_currentOpenStatus ? 0 : -290, -10, 0), moveTime);
        }
    }
}