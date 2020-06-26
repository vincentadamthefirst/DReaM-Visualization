using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Scenery.RoadNetwork.RoadObjects {
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

        public float GetZ(float s) {
            if (s < 0 || s > Length || Math.Abs(Length) < Tolerance) return -99f;

            return Mathf.Lerp(ZOffsetStart, ZOffsetEnd, s / Length);
        }

        public float GetT(float s) {
            if (s < 0 || s > Length || Math.Abs(Length) < Tolerance) return 0f;

            return Mathf.Lerp(TStart, TEnd, s / Length);
        }

        public float GetHeight(float s) {
            if (s < 0 || s > Length || Math.Abs(Length) < Tolerance) return 0f;

            return Mathf.Lerp(HeightStart, HeightEnd, s / Length);
        }
    }
}