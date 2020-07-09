using UnityEngine;

namespace Visualization {
    
    [CreateAssetMenu(menuName = "VisualizationDesign")]
    public class VisualizationDesign : ScriptableObject {

        [Header("Simulation Events")]
        public Color crashEvent = Color.red;
        public Color otherEvent = Color.white;

        public GameObject eventMarkerPrefab;

        public bool removeUnknownEvents = true;

        public bool GetEventColor(string eventName, ref Color color) {
            if (eventName.ToLower().Contains("crash")) {
                color = crashEvent;
                return true;
            } 
            
            color = otherEvent;
            return !removeUnknownEvents;
        }
    }
}