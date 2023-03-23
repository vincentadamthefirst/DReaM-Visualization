using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Visualization;

namespace Scenery.RoadNetwork {
    public abstract class HoverableElement : VisualizationElement {
        private List<Outline> _outlines;

        private void Start() {
            if (_outlines == null)
                FindOutlines();
        }

        protected void FindOutlines() {
            _outlines = transform.GetComponentsInChildren<Outline>().ToList();
        }

        public override void MouseEnter() {
            base.MouseEnter();
            Debug.Log("Mouse Enter!");
            _outlines.ForEach(x => x.enabled = !SimpleCameraController.Instance.RightMouseClicked &&
                                               !SimpleCameraController.Instance.SettingsOpen);
        }

        public override void MouseExit() {
            base.MouseEnter();
            Debug.Log("Mouse Exit!");
            var p = VisualizationMaster.Instance.Pause;
            VisualizationMaster.Instance.Pause = true;
            _outlines.ForEach(x => x.enabled = false);
            VisualizationMaster.Instance.Pause = p;
        }
    }
}