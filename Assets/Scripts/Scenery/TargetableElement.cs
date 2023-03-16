using System;
using System.Collections.Generic;
using System.Linq;
using Scenery.RoadNetwork;
using UnityEngine;

namespace Scenery {

    public abstract class TargetableElement : HoverableElement {
        [SerializeField] private bool _isTarget;

        public event EventHandler<bool> TargetStatusChanged;

        public bool IsTarget {
            get => _isTarget;
            set {
                _isTarget = value;
                Debug.Log($"{name} target status changed to {_isTarget}");
                TargetStatusChanged?.Invoke(this, _isTarget);
            }
        }

        public override void MouseClicked() {
            base.MouseClicked();
            IsTarget = !IsTarget;
        }

        public virtual Bounds AABB => new Bounds();

        /// <summary>
        /// Returns a list of necessary points to check against based on the current method for finding the points
        /// specified in 
        /// </summary>
        /// <returns></returns>
        public virtual Vector3[] GetReferencePoints() {
            //return GetReferencePointsCustom();
            return GetReferencePointsRenderer();
        }

        /// <summary>
        /// Returns the reference points based on the Renderer(s) of this object and its children.
        /// </summary>
        /// <returns>Bounding Points of the Renderers</returns>
        protected abstract Vector3[] GetReferencePointsRenderer();

        protected abstract Vector3[] GetReferencePointsCustom();
    }
}