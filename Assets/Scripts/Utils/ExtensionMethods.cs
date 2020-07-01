using UnityEngine;

namespace Utils {
    public static class ExtensionMethods {
        public static void SetGlobalScale (this Transform transform, Vector3 globalScale) {
            transform.localScale = Vector3.one;
            var lossyScale = transform.lossyScale;
            transform.localScale = new Vector3(globalScale.x / lossyScale.x, globalScale.y / lossyScale.y,
                globalScale.z / lossyScale.z);
        }
        
        public static void RotateRadians(this ref Vector2 v, float angle) {
            var cosAngle = Mathf.Cos(angle);
            var sinAngle = Mathf.Sin(angle);
            var tmpX = v.x * cosAngle - v.y * sinAngle;
            v.y = v.x * sinAngle + v.y * cosAngle;
            v.x = tmpX;
        }
        
        public static int RoundUpToMultipleOf(this int toRound, int multiple) {
            var remainder = toRound % multiple;
            if (remainder == 0) return toRound;
            return toRound + multiple - remainder;
        }

        public static int RoundDownToMultipleOf(this int toRound, int multiple) {
            return toRound - (toRound % multiple);
        }

        public static void ScaleToValue(this Transform transform, float length, float width, float height) {
            var allBounds = new Bounds();
            var renderers = transform.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers) {
                allBounds.Encapsulate(renderer.bounds);
            }

            var sizeX = allBounds.size.x;
            var sizeY = allBounds.size.y;
            var sizeZ = allBounds.size.z;

            var rescale = transform.localScale;
            
            rescale.x = length * rescale.x / sizeX;
            rescale.y = height * rescale.y / sizeY;
            rescale.z = width * rescale.z / sizeZ;
            
            transform.localScale = rescale;
        }
    }
}