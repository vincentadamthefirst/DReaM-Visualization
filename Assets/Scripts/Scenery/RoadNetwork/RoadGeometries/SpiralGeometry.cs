using System;
using Meta.Numerics.Functions;
using UnityEngine;
using Utils;

namespace Scenery.RoadNetwork.RoadGeometries {
    
    /// <summary>
    /// Class representing a SpiralGeometry from OpenDrive
    /// </summary>
    public class SpiralGeometry : RoadGeometry {
        
        // the parameters
        private readonly float _curvatureStart;
        private readonly float _curvatureEnd;
        
        private static readonly float SqrtPiHalf = Mathf.Sqrt(Mathf.PI / 2);

        public SpiralGeometry(float sStart, float x, float y, float hdg, float length, float curvatureStart,
            float curvatureEnd) : base(sStart, x, y, hdg, length) {

            _curvatureStart = curvatureStart;
            _curvatureEnd = curvatureEnd;
        }

        public override Vector2 Evaluate(float s, float t) {
            var curvatureStart = _curvatureStart;
            var curvatureEnd = _curvatureEnd;

            if (Math.Abs(curvatureStart - curvatureEnd) < Tolerance) {
                throw new ArgumentException("Curvatures are the same for spiral!");
            }

            if (!((0.0 <= curvatureStart && 0.0 <= curvatureEnd) ||
                  (0.0 >= curvatureStart && 0.0 >= curvatureEnd))) {
                throw new ArgumentException("Curvatures must both have the same sign!");
            }

            if (s > Length) {
                s = Length;
            }

            if (curvatureStart >= 0f && curvatureEnd >= 0f) {
                return curvatureStart < curvatureEnd
                    ? SpiralPartA(s, t, curvatureStart, curvatureEnd)
                    : SpiralPartB(Length - s, t, curvatureEnd, curvatureStart);
            } else {
                // curvatureStart < 0 && curvatureEnd <= 0
                curvatureStart *= -1;
                curvatureEnd *= -1;

                return curvatureStart < curvatureEnd
                    ? SpiralPartC(s, t, curvatureStart, curvatureEnd)
                    : SpiralPartD(Length - s, t, curvatureEnd, curvatureStart);
            }
        }

        public override float EvaluateHeading(float s) {
            var curvatureStart = _curvatureStart;
            var curvatureEnd = _curvatureEnd;

            if (Math.Abs(curvatureStart - curvatureEnd) < Tolerance) {
                throw new ArgumentException("Curvatures are the same for spiral!");
            }

            if (!((0.0 <= curvatureStart && 0.0 <= curvatureEnd) ||
                  (0.0 >= curvatureStart && 0.0 >= curvatureEnd))) {
                throw new ArgumentException("Curvatures must both have the same sign!");
            }
            
            // TODO actual implementation

            return hdg;
        }

        private Vector2 SpiralPartA(float s, float t, float curvatureStart, float curvatureEnd) {
            if (Math.Abs(curvatureEnd) < Tolerance) 
                throw new ArgumentException("End curvature cannot be 0!");

            var radiusEnd = 1f / curvatureEnd;
            var distanceEnd = Length / (1f - radiusEnd * curvatureStart);
                    
            if (Length > distanceEnd) throw new ArgumentException();

            var distanceStart = distanceEnd - Length;
            var a = Mathf.Sqrt(2f * radiusEnd * distanceEnd);

            // TODO maybe change Re and Im Part for x and y

            var start = new Vector2 {
                x = Convert.ToSingle(AdvancedMath.FresnelC(distanceStart / a / SqrtPiHalf) * a * SqrtPiHalf),
                y = Convert.ToSingle(AdvancedMath.FresnelS(distanceStart / a / SqrtPiHalf) * a * SqrtPiHalf)
            };

            var distanceOffset = distanceStart + s;
            var offset = new Vector2 {
                x = Convert.ToSingle(AdvancedMath.FresnelC(distanceOffset / a / SqrtPiHalf) * a * SqrtPiHalf),
                y = Convert.ToSingle(AdvancedMath.FresnelS(distanceOffset / a / SqrtPiHalf) * a * SqrtPiHalf),
            };

            offset -= start;

            var tangentAngle = distanceOffset * distanceOffset / a / a;
            if (0f > curvatureEnd) tangentAngle = -tangentAngle;

            var normAngle = tangentAngle + Mathf.PI / 2f;
            normAngle %= 2f * Mathf.PI;

            var norm = new Vector2(1f, 0);
            norm.RotateRadians(normAngle);
            norm *= t;

            offset += norm;
            offset.RotateRadians(hdg);

            offset.x += x;
            offset.y += y;

            return offset;
        }

        private Vector2 SpiralPartB(float s, float t, float curvatureStart, float curvatureEnd) {
            if (Math.Abs(curvatureEnd) < Tolerance) 
                throw new ArgumentException("End curvature cannot be 0!");

            var radiusEnd = 1f / curvatureEnd;
            var distanceEnd = Length / (1f - radiusEnd * curvatureStart);
                    
            if (Length > distanceEnd) throw new ArgumentException();

            var distanceStart = distanceEnd - Length;
            var a = Mathf.Sqrt(2f * radiusEnd * distanceEnd);

            // TODO maybe change Re and Im Part for x and y

            var start = new Vector2 {
                x = Convert.ToSingle(AdvancedMath.FresnelC(distanceStart / a / SqrtPiHalf) * a * SqrtPiHalf),
                y = Convert.ToSingle(AdvancedMath.FresnelS(distanceStart / a / SqrtPiHalf) * a * SqrtPiHalf)
            };

            var distanceOffset = distanceStart + s;
            var offset = new Vector2 {
                x = Convert.ToSingle(AdvancedMath.FresnelC(distanceOffset / a / SqrtPiHalf) * a * SqrtPiHalf),
                y = Convert.ToSingle(AdvancedMath.FresnelS(distanceOffset / a / SqrtPiHalf) * a * SqrtPiHalf),
            };

            offset -= start;

            var tangentAngle = distanceOffset * distanceOffset / a / a;
            if (0f > curvatureEnd) tangentAngle = -tangentAngle;

            var normAngle = tangentAngle + Mathf.PI / 2f;
            normAngle %= 2f * Mathf.PI;

            var norm = new Vector2(1f, 0);
            norm.RotateRadians(normAngle);
            norm *= t;

            offset += norm;

            var endOffset = new Vector2 {
                x = Convert.ToSingle(AdvancedMath.FresnelC(distanceEnd / a / SqrtPiHalf) * a * SqrtPiHalf),
                y = Convert.ToSingle(AdvancedMath.FresnelS(distanceEnd / a / SqrtPiHalf) * a * SqrtPiHalf),
            };
            endOffset -= start;

            var tangentAngleEnd = distanceEnd * distanceEnd / a / a;
            if (0 > curvatureEnd) tangentAngleEnd = -tangentAngleEnd;

            tangentAngleEnd = -tangentAngleEnd + Mathf.PI;

            offset -= endOffset;
            offset.y *= -1f;

            offset.RotateRadians(hdg - tangentAngleEnd);

            offset.x += x;
            offset.y += y;

            return offset;
        }
        
        private Vector2 SpiralPartC(float s, float t, float curvatureStart, float curvatureEnd) {
            if (Math.Abs(curvatureEnd) < Tolerance) 
                throw new ArgumentException("End curvature cannot be 0!");

            var radiusEnd = 1f / curvatureEnd;
            var distanceEnd = Length / (1f - radiusEnd * curvatureStart);
                    
            if (Length > distanceEnd) throw new ArgumentException();

            var distanceStart = distanceEnd - Length;
            var a = Mathf.Sqrt(2f * radiusEnd * distanceEnd);

            // TODO maybe change Re and Im Part for x and y

            var start = new Vector2 {
                x = Convert.ToSingle(AdvancedMath.FresnelC(distanceStart / a / SqrtPiHalf) * a * SqrtPiHalf),
                y = Convert.ToSingle(AdvancedMath.FresnelS(distanceStart / a / SqrtPiHalf) * a * SqrtPiHalf)
            };

            var distanceOffset = distanceStart + s;
            var offset = new Vector2 {
                x = Convert.ToSingle(AdvancedMath.FresnelC(distanceOffset / a / SqrtPiHalf) * a * SqrtPiHalf),
                y = Convert.ToSingle(AdvancedMath.FresnelS(distanceOffset / a / SqrtPiHalf) * a * SqrtPiHalf),
            };

            offset -= start;

            var tangentAngle = distanceOffset * distanceOffset / a / a;
            if (0f > curvatureEnd) tangentAngle = -tangentAngle;

            var normAngle = tangentAngle + (Mathf.PI / 2f);
            normAngle %= 2f * Mathf.PI;

            var norm = new Vector2(-1f, 0);
            norm.RotateRadians(normAngle);
            norm *= t;

            offset += norm;
            offset.y *= -1f;
            offset.RotateRadians(hdg);

            offset.x += x;
            offset.y += y;

            return offset;
        }
        
        private Vector2 SpiralPartD(float s, float t, float curvatureStart, float curvatureEnd) {
            if (Math.Abs(curvatureEnd) < Tolerance) 
                throw new ArgumentException("End curvature cannot be 0!");

            var radiusEnd = 1f / curvatureEnd;
            var distanceEnd = Length / (1f - radiusEnd * curvatureStart);
                    
            if (Length > distanceEnd) throw new ArgumentException();

            var distanceStart = distanceEnd - Length;
            var a = Mathf.Sqrt(2f * radiusEnd * distanceEnd);

            // TODO maybe change Re and Im Part for x and y

            var start = new Vector2 {
                x = Convert.ToSingle(AdvancedMath.FresnelC(distanceStart / a / SqrtPiHalf) * a * SqrtPiHalf),
                y = Convert.ToSingle(AdvancedMath.FresnelS(distanceStart / a / SqrtPiHalf) * a * SqrtPiHalf)
            };

            var distanceOffset = distanceStart + s;
            var offset = new Vector2 {
                x = Convert.ToSingle(AdvancedMath.FresnelC(distanceOffset / a / SqrtPiHalf) * a * SqrtPiHalf),
                y = Convert.ToSingle(AdvancedMath.FresnelS(distanceOffset / a / SqrtPiHalf) * a * SqrtPiHalf),
            };

            offset -= start;

            var tangentAngle = distanceOffset * distanceOffset / a / a;
            if (0f > curvatureEnd) tangentAngle = -tangentAngle;

            var normAngle = tangentAngle + (Mathf.PI / 2f);
            normAngle %= (2f * Mathf.PI);

            var norm = new Vector2(-1f, 0);
            norm.RotateRadians(normAngle);
            norm *= t;

            offset += norm;

            var endOffset = new Vector2 {
                x = Convert.ToSingle(AdvancedMath.FresnelC(distanceEnd / a / SqrtPiHalf) * a * SqrtPiHalf),
                y = Convert.ToSingle(AdvancedMath.FresnelS(distanceEnd / a / SqrtPiHalf) * a * SqrtPiHalf),
            };
            endOffset -= start;

            var tangentAngleEnd = distanceEnd * distanceEnd / a / a;
            if (curvatureEnd < 0) tangentAngleEnd = -tangentAngleEnd;

            tangentAngleEnd -= Mathf.PI;

            offset -= endOffset;
            offset.RotateRadians(hdg - tangentAngleEnd);

            offset.x += x;
            offset.y += y;

            return offset;
        }
    }
}