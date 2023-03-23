using UnityEngine;

namespace UI.UIStyling {
    [ExecuteInEditMode]
    public abstract class UIStyle : MonoBehaviour {
        
        public abstract UIStyleElementType GetElementType();
        
        private void OnEnable() {
            UIStyler.Instance?.SingleUpdate(this);
        }
    }
}