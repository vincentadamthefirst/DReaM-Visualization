using System;

namespace Scenery.RoadNetwork.RoadObjects {
    public abstract class RoadObject : SceneryElement {
        public RoadObjectType RoadObjectType { get; set; }
        
        public RoadObjectOrientation Orientation { get; set; }
        
        public float S { get; set; }
        
        public float T { get; set; }
        
        public float ZOffset { get; set; }
        
        public float Heading { get; set; }

        public float Height { get; set; }
        
        public Road Parent { get; set; }
        
        public RepeatParameters RepeatParameters { get; set; }
        
        public RoadDesign RoadDesign { get; set; }

        protected bool markedForDelete;

        private void Update() {
            if (!markedForDelete) return;
            Parent.RoadObjects.Remove(this);
            Destroy(this);
        }

        protected abstract void Repeat();

        public abstract void Show();
    }
}