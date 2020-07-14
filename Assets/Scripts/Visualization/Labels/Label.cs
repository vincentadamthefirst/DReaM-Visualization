using System;
using System.Collections.Generic;
using UnityEngine;

namespace Visualization.Labels {
    public abstract class Label : MonoBehaviour {

        /// <summary>
        /// Updates the strings for this label. The parameters must be ordered correctly.
        /// </summary>
        /// <param name="parameters">The parameters that should be displayed.</param>
        public abstract void UpdateStrings(params string[] parameters);

        /// <summary>
        /// Updates the integers for this label. The parameters must be ordered correctly.
        /// </summary>
        /// <param name="parameters">The parameters that should be displayed.</param>
        public abstract void UpdateIntegers(params int[] parameters);

        /// <summary>
        /// Updates the floats for this label. The parameters must be ordered correctly.
        /// </summary>
        /// <param name="parameters">The parameters that should be displayed.</param>
        public abstract void UpdateFloats(params float[] parameters);

        /// <summary>
        /// Sets the colors for this label. The parameters must be ordered correctly.
        /// </summary>
        /// <param name="parameters">The parameters that should be set.</param>
        public abstract void SetColors(params Color[] parameters);

        /// <summary>
        /// Sets the strings for this label. The parameters must be ordered correctly.
        /// </summary>
        /// <param name="parameters">The parameters that should be set.</param>
        public abstract void SetStrings(params string[] parameters);

        /// <summary>
        /// Sets the floats for this label. The parameters must be ordered correctly.
        /// </summary>
        /// <param name="parameters">The parameters that should be set.</param>
        public abstract void SetFloats(params float[] parameters);

        /// <summary>
        /// Activate this label
        /// </summary>
        public abstract void Activate();
        
        /// <summary>
        /// Deactivate this label
        /// </summary>
        public abstract void Deactivate();
    }
}
