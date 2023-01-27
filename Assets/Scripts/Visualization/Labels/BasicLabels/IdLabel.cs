﻿using UnityEngine;

namespace Visualization.Labels.BasicLabels {
    public class IdLabel : MonoBehaviour {
        
        public Camera MainCamera { get; set; }

        private void Update() {
            transform.LookAt(2 * transform.position - MainCamera.transform.position);
        }
    }
}