using UnityEngine;

namespace Utils {
    /// <summary>
    /// Class for extending the basic Unity Camera with some functions for the visualization
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class ExtendedCamera : MonoBehaviour {
        
        /// <summary>
        /// The Frustum Planes for this camera, get re-calculated every frame in Update()
        /// </summary>
        public Plane[] CurrentFrustumPlanes { get; private set; }

        // the Camera object this script is attached to
        public Camera Camera { get; private set; }

        /// <summary>
        /// Finds necessary objects
        /// </summary>
        private void Start() {
            Camera = GetComponent<Camera>();
        }

        /// <summary>
        /// Re-calculates frustum planes
        /// </summary>
        private void Update() {
            CurrentFrustumPlanes = GeometryUtility.CalculateFrustumPlanes(Camera);
        }
    }
}