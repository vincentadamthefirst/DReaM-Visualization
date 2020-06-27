using UnityEngine;

namespace Scenery.RoadNetwork.RoadObjects {
    public class RoadObjectSquare : RoadObject {
        public float Length { get; set; }
        
        public float Width { get; set; }

        protected override void Repeat() {
            if (RepeatParameters == null) return;

            var start = RepeatParameters.SStart;
            var end = RepeatParameters.Length;
            var dist = RepeatParameters.Distance;
            
            for (var s = 0f; s <= end; s += dist) {
                // Create a copy of the current RoadObject and change some values for it
                var newChild = Instantiate(this);
                newChild.RepeatParameters = null;
                newChild.Heading = Heading;
                newChild.T = RepeatParameters.GetT(s);
                newChild.ZOffset = RepeatParameters.GetZ(s);
                newChild.Height = RepeatParameters.GetHeight(s);
                newChild.S = s + start;
                newChild.Orientation = Orientation;
                newChild.Parent = Parent;
                newChild.RoadObjectType = RoadObjectType;
                newChild.SubType = SubType;
                newChild.RoadDesign = RoadDesign;
                newChild.name = name;
                newChild.Width = Width;
                newChild.Length = Length;
                newChild.Show();
            }

            markedForDelete = true;
        }

        public override void Show() {
            Repeat();
            
            var completeHdg = Parent.EvaluateHeading(S) + Heading;
            transform.rotation = Quaternion.Euler(0, Mathf.Rad2Deg * completeHdg, 0);
        }
    }
}