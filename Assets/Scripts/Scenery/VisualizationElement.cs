using System;
using System.Collections.Generic;
using System.Linq;
using UI.Settings;
using UnityEngine;

namespace Scenery {

    public enum ElementOrigin {
        OpenDrive, OpenPass, Generated
    }

    /**
     * Every Element in the scene is considered a VisualizationElement.
     * This class provides basic information about the element as well as the current visualization.
     */
    public abstract class VisualizationElement : MonoBehaviour {
        public string Id { get; set; }

        public abstract ElementOrigin ElementOrigin { get; }
        
        /**
         * Invoked whenever this element is clicked.
         */
        public event EventHandler ElementClicked;

        private void OnMouseDown() => MouseClicked();

        private void OnMouseEnter() => MouseEnter();

        private void OnMouseExit() => MouseExit();

        public virtual void MouseClicked() {
            Debug.Log($"Mouse Click: {name}");
            ElementClicked?.Invoke(this, EventArgs.Empty);
        }

        public virtual void MouseEnter() {
            // may be overridden
        }
        
        public virtual void MouseExit() {
            // may be overriden
        }

        public ApplicationSettings settings;
        
        // Properties for materials
        protected static readonly int BumpMap = Shader.PropertyToID("_BumpMap");
        protected static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
        protected static readonly int OcclusionMap = Shader.PropertyToID("_OcclusionMap");
        protected static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        protected static readonly int ShaderColor = Shader.PropertyToID("_Color");
        protected static readonly int Surface = Shader.PropertyToID("_Surface");
    }
}