using UnityEngine;

namespace Scenery {
    public class SceneryElement : MonoBehaviour {
        public string OpenDriveId { get; set; }
        
        // tolerance for checking if floating point number is 0
        protected const float Tolerance = 0.00001f;

        protected const int RenderQueueOffsetStartRoads = 20;
        protected const int RenderQueueOffsetEndRoads = 219;
        protected const int RenderQueueOffsetStartRoadmark = 220;
        protected const int RenderQueueOffsetEndRoadmark = 419;
    }
}