using UnityEngine;

namespace Scenery {
    public class SceneryElement : MonoBehaviour {
        public string OpenDriveId { get; set; }
        
        // tolerance for checking if floating point number is 0
        protected const float Tolerance = 0.00001f;
    }
}