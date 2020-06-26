using UnityEngine;

namespace Utils {
    public static class ExtensionMethods {
        public static void SetGlobalScale (this Transform transform, Vector3 globalScale) {
            transform.localScale = Vector3.one;
            var lossyScale = transform.lossyScale;
            transform.localScale = new Vector3(globalScale.x / lossyScale.x, globalScale.y / lossyScale.y,
                globalScale.z / lossyScale.z);
        }
    }
}