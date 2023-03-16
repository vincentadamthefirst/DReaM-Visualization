using Scenery;
using Scenery.RoadNetwork;
using Visualization.Labels.BasicLabels;

namespace Visualization.POIs {
    public class StoppingPointMarker : HoverableElement {
        public override ElementOrigin ElementOrigin => ElementOrigin.OpenPass;

        public TextLabel InfoLabel { get; set; }

        public override void MouseEnter() {
            base.MouseEnter();
            InfoLabel.gameObject.SetActive(true);
        }
        
        public override void MouseExit() {
            base.MouseExit();
            InfoLabel.gameObject.SetActive(false);
        }
    }
}