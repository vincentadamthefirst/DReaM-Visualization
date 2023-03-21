using Scenery.RoadNetwork;
using UnityEngine;

namespace Scenery {
    public class MouseHoverPropagator : MonoBehaviour {
        
        private HoverableElement _parent;

        private const int MaxDepth = 5;

        private void OnMouseEnter() => _parent.MouseEnter();

        private void OnMouseExit() => _parent.MouseExit();

        private void Start() {
            _parent = FindHoverableElementInParent(transform);
        }

        private static HoverableElement FindHoverableElementInParent(Transform start) {
            var current = start;
            var depth = 0;
            
            while (true) {
                if (depth > MaxDepth) return null;

                if (current.GetComponent<HoverableElement>() != null) return current.GetComponent<HoverableElement>();
                current = current.transform.parent;
                depth += 1;
            }
        }
        
    }
}