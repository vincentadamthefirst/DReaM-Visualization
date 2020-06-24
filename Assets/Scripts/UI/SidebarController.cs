using UnityEngine;

namespace UI {
    public class SidebarController : MonoBehaviour {

        private Animation _animation;
        private bool _isOpen = false;

        // Start is called before the first frame update
        private void Start() {
            _animation = GetComponent<Animation>();
        }

        public void SwitchSidebar() {
            _animation.Play(_isOpen ? "SidebarClose" : "SidebarOpen");
            _isOpen = !_isOpen;
        }
    }
}
