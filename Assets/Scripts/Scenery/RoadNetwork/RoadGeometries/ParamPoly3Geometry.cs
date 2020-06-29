using System;
using UnityEngine;
using Utils;

namespace Scenery.RoadNetwork.RoadGeometries {
    public class ParamPoly3Geometry : RoadGeometry {

        private float _aV, _aU;
        private float _bV, _bU;
        private float _cV, _cU;
        private float _dV, _dU;

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
            //if (s >= length) s = length - 0.5f;
            // TODO check why this is so buggy

            var k = 0f;
            var lastPosition = new Vector2();
            var delta = new Vector2();
            var p = 0f;
            var position = new Vector2(_aU, _aV);

            while (k < s) {
                lastPosition = position;
                p += 1 / length;

                position.x = _aU + _bU * p + _cU * p * p + _dU * p * p * p;
                position.y = _aV + _bV * p + _cV * p * p + _dV * p * p * p;

                delta = position - lastPosition;
                var deltaLength = delta.magnitude;

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
            var k = 0f;
            var lastPosition = new Vector2();
            var delta = new Vector2();
            var p = 0f;
            var position = new Vector2(_aU, _aV);

            while (k < s) {
                lastPosition = position;
                p += 1 / length;

                position.x = _aU + _bU * p + _cU * p * p + _dU * p * p * p;
                position.y = _aV + _bV * p + _cV * p * p + _dV * p * p * p;

                delta = position - lastPosition;
                var deltaLength = delta.magnitude;

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

            var direction = new Vector2();
            if (s > 0) direction = delta;
            else direction.x = 1f;

            direction.RotateRadians(hdg);
            direction.Normalize();

            if (direction.y > 1f) direction.y = 1f;
            if (direction.y < -1f) direction.y = -1f;

            var angle = Mathf.Asin(direction.y);
            if (direction.x >= 0f) return angle;
            return Mathf.PI - angle;
        }
    }
}