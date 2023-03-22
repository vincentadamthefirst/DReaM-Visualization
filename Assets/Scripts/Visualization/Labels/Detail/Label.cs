using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils;
using Visualization.Agents;

namespace Visualization.Labels.Detail {

    public class Label : MonoBehaviour, IDragHandler {

        public RectTransform main;

        private readonly List<RectTransform> _infoPanels = new();
        private readonly List<LabelEntry> _labelEntries = new ();

        private TMP_Text _agentId;
        private SVGImage _indicatorLeftImage;
        private TMP_Text _brakeText;
        private SVGImage _indicatorRightImage;

        private readonly string[] _agentTypeNames = { "Unknown", "Car", "Truck", "Bike", "Motorcycle", "Pedestrian" };

        private Reference<bool> _indicatorLeft;
        private Reference<bool> _brake;
        private Reference<bool> _indicatorRight;

        private RectTransform _parent;
        private RectTransform _self;

        private void Awake() {
            _agentId = main.Find("Agent ID").GetComponent<TMP_Text>();
            var lights = main.Find("Lights");
            _indicatorLeftImage = lights.Find("Left").GetComponent<SVGImage>();
            _brakeText = lights.Find("Brake").GetComponent<TMP_Text>();
            _indicatorRightImage = lights.Find("Right").GetComponent<SVGImage>();
            _parent = (RectTransform) transform.parent;
            _self = (RectTransform) transform;
        }

        public void Initialize(Agent agent) {
            if (agent.GetType() == typeof(BoxAgent) || agent.GetType() == typeof(VehicleAgent)) {
                GeneralSetup(agent);
                VehicleSetup(agent.DynamicData.ActiveSimulationStep.AdditionalInformation as AdditionalVehicleInformation);
            } else if (agent.GetType() == typeof(PedestrianAgent)) {
                GeneralSetup(agent);
            } else {
                throw new ArgumentException("Unknown agent object provided.");
            }
        }

        private void VehicleSetup(AdditionalVehicleInformation avi) {
            _indicatorLeft = new Reference<bool>(() =>
                avi.IndicatorState is IndicatorState.Left or IndicatorState.Warn);
            _brake = new Reference<bool>(() => avi.Brake);
            _indicatorRight = new Reference<bool>(() =>
                avi.IndicatorState is IndicatorState.Right or IndicatorState.Warn);
        }

        private void GeneralSetup(Agent agent) {
            _agentId.text = $"{agent.Id} ({_agentTypeNames[(int) agent.StaticData.AgentTypeDetail]})";
        }

        private void UpdateLightStatus() {
            _indicatorLeftImage.color = _indicatorLeft.Value ? new Color(243, 142, 72) : new Color(169, 69, 0);
            _brakeText.color = _brake.Value ? new Color(235, 0, 0) : new Color(150, 0, 0);
            _indicatorRightImage.color = _indicatorRight.Value ? new Color(243, 142, 72) : new Color(169, 69, 0);
        }

        private bool CheckIfLastInfoPanelOverflows(float nextSize) {
            var last = _infoPanels[^1];
            var totalChildSize = last.Cast<RectTransform>().Sum(child => child.rect.height + 3);
            return totalChildSize + nextSize > 246;
        }

        public void AddLabelTextEntry(string title, Reference<string> reference) {
            var labelTextEntryPrefab = Resources.Load<LabelTextEntry>("Prefabs/UI/Visualization/Labels/LabelTextEntry");
            var rect = labelTextEntryPrefab.GetComponent<RectTransform>().rect;
            if (_infoPanels.Count == 0 || CheckIfLastInfoPanelOverflows(rect.height)) {
                var infoPanelPrefab = Resources.Load<RectTransform>("Prefabs/UI/Visualization/Labels/InfoPanel");
                var infoPanel = Instantiate(infoPanelPrefab, transform);
                _infoPanels.Add(infoPanel);
            }
            var parent = _infoPanels[^1];
            var textEntry = Instantiate(labelTextEntryPrefab, parent);
            textEntry.Initialize(title);
            textEntry.Reference = reference;
            _labelEntries.Add(textEntry);
        }

        public void TriggerUpdate() {
            UpdateLightStatus();
            _labelEntries.ForEach(x => x.TriggerUpdate());
        }

        public void OnDrag(PointerEventData _) {
            var newPosX = Input.mousePosition.x - (_parent.rect.width / 2f);
            var newPosY = Input.mousePosition.y - (_parent.rect.height / 2f);

            if (newPosX > (_parent.rect.width - _self.rect.width) / 2f)
                newPosX = (_parent.rect.width - _self.rect.width) / 2f;
            else if (newPosX < -((_parent.rect.width - _self.rect.width) / 2f))
                newPosX = -((_parent.rect.width - _self.rect.width) / 2f);
            
            if (newPosY > (_parent.rect.height - _self.rect.height) / 2f) 
                newPosY = (_parent.rect.height - _self.rect.height) / 2f;
            else if (newPosY < -((_parent.rect.height - _self.rect.height) / 2f))
                newPosY = -((_parent.rect.height - _self.rect.height) / 2f);

            transform.localPosition = new Vector3(newPosX, newPosY, 0);
        }
    }
}