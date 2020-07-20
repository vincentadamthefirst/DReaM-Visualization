using UnityEngine;

namespace Scenery {
    public class SceneryElement : MonoBehaviour {
        public string OpenDriveId { get; set; }
        
        // tolerance for checking if floating point number is 0
        protected const float Tolerance = 0.00001f;

        // Properties for materials
        protected static readonly int BumpMap = Shader.PropertyToID("_BumpMap");
        protected static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
        protected static readonly int OcclusionMap = Shader.PropertyToID("_OcclusionMap");
        protected static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        public virtual void SetLayer(int layer) {
            // implemented in junction and road
        }
    }
}