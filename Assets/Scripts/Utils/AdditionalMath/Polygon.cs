using System.Collections.Generic;
using UnityEngine;

namespace Utils.AdditionalMath {

	public class Polygon {
		private IList<Vector2> _points;
		private List<Vector2> _edges;

		public Polygon(IList<Vector2> points) {
			_points = points;
			_edges = new List<Vector2>();
			BuildEdges();
		}

		private void BuildEdges() {
			Vector2 p1;
			Vector2 p2;
			for (var i = 0; i < _points.Count; i++) {
				p1 = _points[i];
				if (i + 1 >= _points.Count) {
					p2 = _points[0];
				} else {
					p2 = _points[i + 1];
				}
				_edges.Add(p2 - p1);
			}
		}

		public List<Vector2> Edges() {
			return _edges;
		}

		public IList<Vector2> Points() {
			return _points;
		}

		public Vector2 Center {
			get {
				float totalX = 0;
				float totalY = 0;
				for (int i = 0; i < _points.Count; i++) {
					totalX += _points[i].x;
					totalY += _points[i].y;
				}

				return new Vector2(totalX / (float)_points.Count, totalY / (float)_points.Count);
			}
		}

		public void Offset(Vector2 v) {
			Offset(v.x, v.y);
		}

		private void Offset(float x, float y) {
			for (var i = 0; i < _points.Count; i++) {
				var p = _points[i];
				_points[i] = new Vector2(p.x + x, p.y + y);
			}
		}
	}
}

