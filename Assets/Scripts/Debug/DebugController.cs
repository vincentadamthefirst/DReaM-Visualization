using UnityEngine;

namespace OwnDebug {
    
    /// <summary>
    /// Singleton class to handle all console operations. Main entry point for console logging as well as 
    /// </summary>
    public sealed class DebugController : MonoBehaviour {
        private DebugController() {}

        public static DebugController instance;

        private void Awake() {
            if (instance != null && instance != this) {
                Destroy(gameObject);
            } else {
                instance = this;
            }
        }

        public void SendLogMessage() {
            
        }
    }


    public enum LogLevel {
        Debug = 0, Normal = 2, Warnings = 4
    }
}