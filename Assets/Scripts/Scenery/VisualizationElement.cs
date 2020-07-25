using System;
using System.Diagnostics;
using UnityEngine;
using Visualization.OcclusionManagement;
using Debug = UnityEngine.Debug;

namespace Scenery {
    public abstract class VisualizationElement : MonoBehaviour {
        
        /// <summary>
        /// The OpenDrive Id of this object, can be null if this object is not an OpenDrive object.
        /// </summary>
        public string OpenDriveId { get; set; }
        
        /// <summary>
        /// The currently applied options for managing occlusions.
        /// </summary>
        public OcclusionManagementOptions OcclusionManagementOptions { get; set; }

        /// <summary>
        /// The actual world anchor of the object. This might differ from the anchor of the object the script is on
        /// (e.g. for any Agent this would be the position of its Model)
        /// </summary>
        public virtual Vector3 WorldAnchor { get; }

        public abstract bool IsDistractor { get; }

        // tolerance for checking if floating point number is 0
        protected const float Tolerance = 0.00001f;
        
        // if the element is a target object
        protected bool isTarget = false;

        public virtual bool IsActive => true;

        public virtual Bounds AxisAlignedBoundingBox => new Bounds();

        // Properties for materials
        protected static readonly int BumpMap = Shader.PropertyToID("_BumpMap");
        protected static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
        protected static readonly int OcclusionMap = Shader.PropertyToID("_OcclusionMap");
        protected static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        protected static readonly int Color = Shader.PropertyToID("_Color");
        protected static readonly int Surface = Shader.PropertyToID("_Surface");

        public virtual void SetLayer(int layer) {
            // implemented in junction and road
        }

        /// <summary>
        /// Handles that this object is overlapping a target object.
        /// </summary>
        public abstract void HandleHit();

        /// <summary>
        /// Handles that the object is no longer occluding a target.
        /// </summary>
        public abstract void HandleNonHit();

        /// <summary>
        /// Returns a list of necessary points to check against based on the current method for finding the points
        /// specified in 
        /// </summary>
        /// <returns></returns>
        public virtual Vector3[] GetReferencePoints() {
            switch (OcclusionManagementOptions.pointSource) {
                case PointSource.Custom:
                    return GetReferencePointsCustom();
                case PointSource.RendererBounds:
                    return GetReferencePointsRenderer();
                case PointSource.Other:
                default:
                    return new Vector3[0];
            }
        }
        
        public virtual void SetIsTarget(bool target) {
            isTarget = target;
        }

        public bool IsTarget() {
            return isTarget;
        }

        /// <summary>
        /// Returns the reference points based on the Renderer(s) of this object and its children.
        /// </summary>
        /// <returns>Bounding Points of the Renderers</returns>
        protected abstract Vector3[] GetReferencePointsRenderer();

        protected abstract Vector3[] GetReferencePointsCustom();
    }
}