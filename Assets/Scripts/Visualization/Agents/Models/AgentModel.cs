using UnityEngine;

namespace Visualization.Agents.Models {
    
    /// <summary>
    /// Class to realize a model in the world. Handles all model specific setup routines for scaling.
    /// </summary>
    public abstract class AgentModel : MonoBehaviour {
        
        /// <summary>
        /// The agent this model belongs to
        /// </summary>
        public Agent Agent { get; set; }

        /// <summary>
        /// Scales all elements of the underlying 3D model for the agent.
        /// </summary>
        public abstract void PreRun();

        /// <summary>
        /// Updates elements of this model for the current time step.
        /// </summary>
        public abstract void UpdateStep(SimulationStep simulationStep);
    }
}