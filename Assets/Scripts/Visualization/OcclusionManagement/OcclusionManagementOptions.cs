using UnityEngine;

namespace Visualization.OcclusionManagement {
    
    [CreateAssetMenu(menuName = "OcclusionManagementOptions")]
    public class OcclusionManagementOptions : ScriptableObject {
        // LABEL OPTIONS
        public LabelLocation labelLocation;
        
        // WORLD SPACE OCCLUSION MANAGEMENT OPTIONS

        /// <summary>
        /// The general method for detecting occlusion in the scene.
        /// Any other options will be ignored, if OcclusionDetectionMethod.Shader is selected
        /// </summary>
        public OcclusionDetectionMethod occlusionDetectionMethod;

        /// <summary>
        /// The method of handling a found occlusion.
        /// </summary>
        public OcclusionHandlingMethod occlusionHandlingMethod;
        
        // ADDITIONAL RAYCAST OPTIONS

        /// <summary>
        /// A given amount of points will be sampled to perform the RayCast.
        /// The amount is specified in randomPointAmount.
        /// </summary>
        public bool sampleRandomPoints;

        /// <summary>
        /// If this option is set to true the rays will originate from the near clip plane of the Camera, not the
        /// position of the Camera.
        /// </summary>
        public bool nearClipPlaneAsStart = true;

        /// <summary>
        /// Each line of the collider box will receive rayCastPrecision additional points
        /// </summary>
        public int rayCastPrecision = 3;

        /// <summary>
        /// Amount of random Points to be sampled for RayCasting
        /// </summary>
        public int randomPointAmount = 15;
        
        // ADDITIONAL GENERAL OPTIONS
        
        /// <summary>
        /// The points to be used for constructing polygons or sending out rays based on OcclusionHandlingMethod
        /// </summary>
        public PointSource pointSource;

        /// <summary>
        /// If the algorithm for occlusion detection checks for a combined BoundingBox of an object or handles
        /// each BoundingBox by itself. Setting this to false will result in decreased performance.
        /// </summary>
        public bool useCombinedBoundingBox = true;

        /// <summary>
        /// Objects that are not in the view frustum of the camera will be ignored in occlusion detection.
        /// </summary>
        public bool preCheckViewFrustum = true;

        /// <summary>
        /// The alpha value to be used for Transparency. If decreaseAlpha = false this will be used for every object.
        /// </summary>
        public float transparencyValue = 0.2f;

        /// <summary>
        /// Only check one target each frame. Will go through n targets in n frames.
        /// Based on the fact that there is only a slight change in relative position of target and camera each frame
        /// we can assume that checking each target every frame is not necessary and the work can be split to multiple
        /// LateUpdate() calls, reducing the performance impact.
        ///
        /// This method will lead to significant loss of precision if used with a large number of targets. For 60 FPS
        /// and 60 targets each target will only be tested once per second!
        /// </summary>
        public bool staggeredCheck = true;
    }

    /// <summary>
    /// Enum describing where in the world a label will be placed.
    /// </summary>
    public enum LabelLocation {
        Screen, World
    }

    /// <summary>
    /// Enum describing the general Occlusion Detection method.
    /// </summary>
    public enum OcclusionDetectionMethod {
        Shader, RayCast, Polygon
    }

    /// <summary>
    /// Enum describing the Handling method for Occlusion
    /// </summary>
    public enum OcclusionHandlingMethod {
        Transparency, WireFrame
    }

    /// <summary>
    /// The source of points to be used in RayCasts and Polygon Construction
    /// </summary>
    public enum PointSource {
        Custom, RendererBounds, Other
    }
}