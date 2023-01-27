using System.Collections.Generic;
using UnityEngine;

namespace Visualization.Agents {
    public class CustomPoints : MonoBehaviour {

        public List<Vector3> customPoints = new();

        /// <summary>
        /// Draw the points for debugging in the scene
        /// </summary>
        private void OnDrawGizmos() {
            Gizmos.color = Color.black;
            customPoints.ForEach(p => Gizmos.DrawSphere(p, 0.1f));
        }
    }
}