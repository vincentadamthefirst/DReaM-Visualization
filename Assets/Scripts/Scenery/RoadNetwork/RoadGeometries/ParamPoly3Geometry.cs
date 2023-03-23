using System;
using UnityEngine;
using Utils;

namespace Scenery.RoadNetwork.RoadGeometries {
    
    /// <summary>
    /// Class representing a ParamPoly3Geometry from OpenDrive
    /// </summary>
    public class ParamPoly3Geometry : RoadGeometry {

        // the parameters
        private readonly float _aV, _aU;
        private readonly float _bV, _bU;
        private readonly float _cV, _cU;
        private readonly float _dV, _dU;

        public ParamPoly3Geometry(float sStart, float x, float y, float hdg, float length, float aV, float aU, float bV,
            float bU, float cV, float cU, float dV, float dU) : base(sStart, x, y, hdg, length) {

            _aV = aV;
            _aU = aU;
            _bV = bV;
            _bU = bU;
            _cV = cV;
            _cU = cU;
            _dV = dV;
            _dU = dU;
        }
        public override Vector2 Evaluate(float s, float t) {
            if (Math.Abs(_aV) < Tolerance && Math.Abs(_bV) < Tolerance && Math.Abs(_cV) < Tolerance &&
                Math.Abs(_dV) < Tolerance) {
                return LineGeometry.GetLine(s, t, x, y, hdg);
            }

            var k = 0f;
            var lastPosition = new Vector2();
            var delta = new Vector2();
            var p = 0f;
            var position = new Vector2(_aU, _aV);

            while (k < s) {
                lastPosition = position;
                p += 1 / Length;

                var stepSize = 1f;
                var deltaLength = float.MaxValue;

                while (deltaLength > 0.1) {
                    var pNew = p + stepSize;
                    position.x = _aU + _bU * pNew + _cU * pNew * pNew + _dU * pNew * pNew * pNew;
                    position.y = _aV + _bV * pNew + _cV * pNew * pNew + _dV * pNew * pNew * pNew;

                    delta = position - lastPosition;
                    deltaLength = delta.magnitude;
                    stepSize *= .5f;
                }

                p += stepSize;

                if (Math.Abs(deltaLength) < Tolerance) {
                    throw new RoadGeometryGenerationException("ParamPoly3 generation could not be finished.");
                }

                if (k + deltaLength > s) {
                    var scale = (s - k) / deltaLength;
                    delta *= scale;
                    deltaLength = s - k;
                }

                k += deltaLength;
            }

            var offset = lastPosition + delta;
            var norm = 0 < s ? delta : new Vector2(1, 0);
            
            norm.RotateRadians(-Mathf.PI / 2);
            norm = norm.normalized;

            offset += norm * -t;
            offset.RotateRadians(hdg);
            
            offset.x += x;
            offset.y += y;

            return offset;
        }

        public override float EvaluateHeading(float s) {
            if (Math.Abs(_aV) < Tolerance && Math.Abs(_bV) < Tolerance && Math.Abs(_cV) < Tolerance &&
                Math.Abs(_dV) < Tolerance) {
                return hdg;
            }
            
            var k = 0f;
            var lastPosition = new Vector2();
            var delta = new Vector2();
            var p = 0f;
            var position = new Vector2(_aU, _aV);

            while (k < s) {
                lastPosition = position; 
                var stepSize = 1f;
                var deltaLength = float.MaxValue;

                while (deltaLength > 0.1) {
                    var pNew = p + stepSize;
                    position.x = _aU + _bU * pNew + _cU * pNew * pNew + _dU * pNew * pNew * pNew;
                    position.y = _aV + _bV * pNew + _cV * pNew * pNew + _dV * pNew * pNew * pNew;

                    delta = position - lastPosition;
                    deltaLength = delta.magnitude;
                    stepSize *= .5f;
                }
                p += stepSize;

                if (Math.Abs(deltaLength) < Tolerance) {
                    throw new RoadGeometryGenerationException("ParamPoly3 generation could not be finished.");
                }

                if (k + deltaLength > s) {
                    var scale = (s - k) / deltaLength;
                    delta *= scale;
                    deltaLength = s - k;
                }

                k += deltaLength;
            }
            
            var dU = _bU + 2 * _cU * p + 3 * _dU * p * p;
            var dV = _bV + 2 * _cV * p + 3 * _dV * p * p;

            return hdg + Mathf.Atan2(dV, dU);
        }
    }
}