using System;
using UnityEngine;

namespace Scenery {
    public class MouseClickPropagator : MonoBehaviour {

        private TargetableElement _parent;

        private static int maxDepth = 5;

        private void OnMouseDown() => _parent.MouseClicked();

        private void OnMouseEnter() => _parent.MouseEnter();

        private void OnMouseExit() => _parent.MouseExit();

        private void Start() {
            _parent = FindTargetableInParent(transform);
        }

        private TargetableElement FindTargetableInParent(Transform start) {
            var current = start;
            var depth = 0;
            
            while (true) {
                if (depth > maxDepth) return null;

                if (current.GetComponent<TargetableElement>() != null) return current.GetComponent<TargetableElement>();
                current = current.transform.parent;
                depth += 1;
            }
        }
    }
}