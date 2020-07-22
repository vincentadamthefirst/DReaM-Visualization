using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Scenery.RoadNetwork.RoadObjects {
    
    /// <summary>
    /// Repeat Parameters for objects from OpenDrive, not all types are supported yet.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class RepeatParameters {
        // tolerance for checking if floating point number is 0
        private const float Tolerance = 0.00001f;
        
        public float SStart { get; set; }
        public float Length { get; set; }
        public float Distance { get; set; }
        public float TStart { get; set; }
        public float TEnd { get; set; }
        public float HeightStart { get; set; }
        public float HeightEnd { get; set; }
        public float ZOffsetStart { get; set; }
        public float ZOffsetEnd { get; set; }

        /// <summary>
        /// The z-Offset along the s coordinate
        /// </summary>
        /// <param name="s">The s value</param>
        /// <returns>The resulting z value</returns>
        public float GetZ(float s) {
            if (s < 0 || s > Length || Math.Abs(Length) < Tolerance) return -99f;

            return Mathf.Lerp(ZOffsetStart, ZOffsetEnd, s / Length);
        }

        /// <summary>
        /// The t-Offset along the s coordinate
        /// </summary>
        /// <param name="s">The s value</param>
        /// <returns>The resulting t value</returns>
        public float GetT(float s) {
            if (s < 0 || s > Length || Math.Abs(Length) < Tolerance) return 0f;

            return Mathf.Lerp(TStart, TEnd, s / Length);
        }

        /// <summary>
        /// The height along the s coordinate
        /// </summary>
        /// <param name="s">The s value</param>
        /// <returns>The resulting height value</returns>
        public float GetHeight(float s) {
            if (s < 0 || s > Length || Math.Abs(Length) < Tolerance) return 0f;

            return Mathf.Lerp(HeightStart, HeightEnd, s / Length);
        }
    }
}