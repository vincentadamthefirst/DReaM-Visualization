using System;
using UI.Settings;
using UnityEngine;

namespace Scenery {

    public enum ElementOrigin {
        OpenDrive, OpenPass,
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

        public void Clicked() {
            ElementClicked?.Invoke(this, EventArgs.Empty);
        }


        public ApplicationSettings settings;
        
        // Properties for materials
        protected static readonly int BumpMap = Shader.PropertyToID("_BumpMap");
        protected static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
        protected static readonly int OcclusionMap = Shader.PropertyToID("_OcclusionMap");
        protected static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        protected static readonly int Color = Shader.PropertyToID("_Color");
        protected static readonly int Surface = Shader.PropertyToID("_Surface");
    }
}