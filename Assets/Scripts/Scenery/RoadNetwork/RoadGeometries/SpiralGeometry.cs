using System;
using Meta.Numerics.Functions;
using UnityEngine;
using Utils;
// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable

namespace Scenery.RoadNetwork.RoadGeometries {
    
    /// <summary>
    /// Class representing a SpiralGeometry from OpenDrive
    /// </summary>
    public class SpiralGeometry : RoadGeometry {
        
        // the parameters
        private readonly float _curvatureStart;
        private readonly float _curvatureEnd;

        private readonly float _cDot;
        private readonly float _lStart;
        private readonly float _lEnd;

        private readonly float _tStart, _a, _sign;

        public SpiralGeometry(float sStart, float x, float y, float hdg, float length, float curvatureStart,
            float curvatureEnd) : base(sStart, x, y, hdg, length) {

            _curvatureStart = curvatureStart;
            _curvatureEnd = curvatureEnd;

            if (length != 0)
                _cDot = (curvatureEnd - curvatureStart) / length;

            if (_cDot != 0)
                _lStart = curvatureStart / _cDot;
            else
                _lStart = 0;

            _lEnd = _lStart + length;
            float rl;

            if (curvatureStart != 0)
                rl = _lStart / curvatureStart;
            else if (curvatureEnd != 0)
                rl = _lEnd / curvatureEnd;
            else {
                _tStart = 0;
                _a = 0;
                _sign = 0;
                return;
            }

            _tStart = .5f * _lStart * curvatureStart;
            _a = Mathf.Sqrt(Mathf.Abs(rl));
            _sign = Mathf.Sign(rl);
        }

        private static readonly float SqrtPI = Mathf.Sqrt(Mathf.PI);

        public override Vector2 Evaluate(float s, float t) {
            if (Math.Abs(_curvatureStart) < Tolerance && Math.Abs(_curvatureEnd) < Tolerance) {
                return LineGeometry.GetLine(s, t, x, y, hdg);
            }

            if (Math.Abs(_curvatureStart - _curvatureEnd) < Tolerance) {
                return ArcGeometry.GetArc(s, t, x, y, hdg, _curvatureStart);
            }
            
            var curvAtsOffset = _curvatureStart + _cDot * s;
            Vector2 start, end;

            start.x = Convert.ToSingle(AdvancedMath.FresnelC(_lStart / _a / SqrtPI));
            start.y = Convert.ToSingle(AdvancedMath.FresnelS(_lStart / _a / SqrtPI));
            start *= (_a * SqrtPI);
            start.y *= _sign;
            
            end.x = Convert.ToSingle(AdvancedMath.FresnelC((_lStart + s) / _a / SqrtPI));
            end.y = Convert.ToSingle(AdvancedMath.FresnelS((_lStart + s) / _a / SqrtPI));
            end *= (_a * SqrtPI);
            end.y *= _sign;

            var diff = end - start;
            var tEnd = (_lStart + s) * curvAtsOffset / 2.0f;
            var dHdg = tEnd - _tStart;
            diff.RotateRadians(-_tStart + hdg);
            var endHdg = hdg + dHdg;

            var unit = Vector2.right;
            unit.RotateRadians(endHdg + Mathf.PI / 2f);
            unit *= t;

            return diff + unit + new Vector2(x, y);
        }

        public override float EvaluateHeading(float s) {
            if (Math.Abs(_curvatureStart) < Tolerance && Math.Abs(_curvatureEnd) < Tolerance) {
                return hdg;
            }

            if (Math.Abs(_curvatureStart - _curvatureEnd) < Tolerance) {
                return ArcGeometry.GetArgHeading(s, hdg, _curvatureStart);
            }

            var cEnd = _curvatureStart + _cDot * s;
            return hdg + .5f * (cEnd * (_lStart + s) - _curvatureStart * _lStart);
        }
    }
}