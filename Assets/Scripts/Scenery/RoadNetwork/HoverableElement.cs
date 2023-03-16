using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scenery.RoadNetwork {
    public abstract class HoverableElement : VisualizationElement {
        private List<Outline> _outlines;

        private void Start() {
            _outlines = transform.GetComponentsInChildren<Outline>().ToList();
        }

        public override void MouseEnter() {
            base.MouseEnter();
            _outlines.ForEach(x => x.enabled = !SimpleCameraController.Instance.RightMouseClicked &&
                                               !SimpleCameraController.Instance.SettingsOpen);
        }

        public override void MouseExit() {
            base.MouseEnter();
            _outlines.ForEach(x => x.enabled = false);
        }
    }
}