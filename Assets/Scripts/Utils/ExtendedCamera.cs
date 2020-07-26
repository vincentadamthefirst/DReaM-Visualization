using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Utils {
    /// <summary>
    /// Class for extending the basic Unity Camera with some functions for the visualization
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class ExtendedCamera : MonoBehaviour {

        // the additional URP camera data
        public UniversalAdditionalCameraData cameraData;
        
        /// <summary>
        /// The Frustum Planes for this camera, get re-calculated every frame in Update()
        /// </summary>
        public Plane[] CurrentFrustumPlanes { get; private set; }

        /// <summary>
        /// The main Camera in the scene
        /// </summary>
        public Camera Camera { get; private set; }
        
        /// <summary>
        /// The Camera Controller
        /// </summary>
        public SimpleCameraController CameraController { get; private set; }

        /// <summary>
        /// Finds necessary objects
        /// </summary>
        private void Start() {
            Camera = GetComponent<Camera>();
            CameraController = Camera.GetComponent<SimpleCameraController>();
        }

        /// <summary>
        /// Re-calculates frustum planes
        /// </summary>
        private void Update() {
            CurrentFrustumPlanes = GeometryUtility.CalculateFrustumPlanes(Camera);
        }
    }
}