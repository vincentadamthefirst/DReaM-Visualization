using System;
using TMPro;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.UI;
using Visualization;
using Visualization.Agents;

namespace UI {

    [Serializable]
    public struct AgentIcons {
        public Sprite pedestrian;
        public Sprite bike;
        public Sprite motorcycle;
        public Sprite car;
        public Sprite truck;
    }
    
    public class AgentCard : MonoBehaviour {
        public RectTransform Parent { get; set; }
        public Agent Agent { get; set; }

        public AgentIcons agentIcons;
        
        public event EventHandler CardClicked;
        
        private RectTransform _mainObject;
        public TextMeshProUGUI text;
        public SVGImage iconImage;
        public Image colorBand;

        public void CustomAwake() {
            _mainObject = GetComponent<RectTransform>();

            _mainObject.localScale = Vector3.one * 1f;
            
            if (Agent == null)
                Debug.Log("Agent null");
            
            if (Agent.StaticData == null)
                Debug.Log("SD null");
            
            if (Agent.StaticData.ColorMaterial == null)
                Debug.Log("CM null");
            
            colorBand.color = Agent.StaticData.ColorMaterial.color;
            text.text = Agent.name.Split(new [] {" ["}, StringSplitOptions.None)[0];
            iconImage.sprite = Agent.StaticData.AgentTypeDetail switch {
                AgentTypeDetail.Unknown => agentIcons.car,
                AgentTypeDetail.Car => agentIcons.car,
                AgentTypeDetail.Truck => agentIcons.truck,
                AgentTypeDetail.Bike => agentIcons.bike,
                AgentTypeDetail.Motorcycle => agentIcons.motorcycle,
                AgentTypeDetail.Pedestrian => agentIcons.pedestrian,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public void TargetStatusChanged(object sender, bool value) {
            _mainObject.localScale = value ? Vector3.one * 1.1f : Vector3.one * 1f;
            LayoutRebuilder.ForceRebuildLayoutImmediate(Parent);
        }

        public void Clicked() {
            CardClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}