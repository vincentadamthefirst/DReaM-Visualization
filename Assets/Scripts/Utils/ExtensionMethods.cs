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
        
        public static void SetSizeX(this Transform transform, float newX) {
            var allBounds = new Bounds();
            foreach (var mf in transform.GetComponentsInChildren<MeshFilter>()) {
                allBounds.Encapsulate(mf.mesh.bounds);
            }
            var size = allBounds.size;
            var oldParent = transform.parent;
            transform.parent = null;

            var oldScale = transform.localScale;
            var newYScale = (oldScale.x / size.x) * newX;
            oldScale.x = newYScale;

            transform.localScale = oldScale;
            
            transform.parent = oldParent;
        }

        public static void SetSizeY(this Transform transform, float newY) {
            var allBounds = new Bounds();
            foreach (var mf in transform.GetComponentsInChildren<MeshFilter>()) {
                allBounds.Encapsulate(mf.mesh.bounds);
            }
            var size = allBounds.size;
            var oldParent = transform.parent;
            transform.parent = null;

            var oldScale = transform.localScale;
            var newYScale = (oldScale.y / size.y) * newY;
            oldScale.y = newYScale;

            transform.localScale = oldScale;
            
            transform.parent = oldParent;
        }

        public static void SetSizeZ(this Transform transform, float newZ) {
            var allBounds = new Bounds();
            foreach (var mf in transform.GetComponentsInChildren<MeshFilter>()) {
                allBounds.Encapsulate(mf.mesh.bounds);
            }
            var size = allBounds.size;
            var oldParent = transform.parent;
            transform.parent = null;

            var oldScale = transform.localScale;
            var newYScale = (oldScale.z / size.z) * newZ;
            oldScale.z = newYScale;

            transform.localScale = oldScale;
            
            transform.parent = oldParent;
        }

        public static void SetTotalSize(this Transform transform, float newX, float newY, float newZ) {
            transform.SetSizeX(newX);
            transform.SetSizeY(newY);
            transform.SetSizeZ(newZ);
        }

        public static void SetSize(this Transform transform, float newSizeX, float newSizeY, float newSizeZ) {
            var oldParent = transform.parent;

            transform.parent = null;

            var allBounds = new Bounds();
            var renderers = transform.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers) {
                allBounds.Encapsulate(renderer.bounds);
            }

            var sizeX = allBounds.size.x;
            var sizeY = allBounds.size.y;
            var sizeZ = allBounds.size.z;

            var oldScale = transform.localScale;
            
            var newScale = new Vector3(
                newSizeX * (oldScale.x / sizeX),
                newSizeY * (oldScale.y / sizeY),
                newSizeZ * (oldScale.z / sizeZ)
            );

            transform.localScale = newScale;

            transform.parent = oldParent;
        }

        public static void ChangeToTransparent(this Material material, float alpha) {
            var color = material.color;
            color.a = alpha;
            material.color = color;
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0);
            material.SetOverrideTag("RenderType", "Transparent");
            material.SetInt("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = (int) UnityEngine.Rendering.RenderQueue.Transparent;
            material.SetShaderPassEnabled("ShadowCaster", false);
        }
        
        public static void SetLayerRecursive(this GameObject obj, int layer) {
            obj.layer = layer;
            foreach (Transform child in obj.transform) {
                SetLayerRecursive(child.gameObject, layer);
            }
        }
    }
}