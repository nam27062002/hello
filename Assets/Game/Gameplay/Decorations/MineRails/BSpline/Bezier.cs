using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BSpline {
	public static class Bezier {

		//----------------------
		// Quadratic
		//----------------------
		public static Vector3 GetPoint(Vector3 _p0, Vector3 _p1, Vector3 _p2, float _t) {
			_t = Mathf.Clamp01(_t);
			float oneMinusT = 1f - _t;
			return (oneMinusT * oneMinusT * _p0) + (2f * oneMinusT * _t * _p1) + (_t * _t * _p2);
		}

		public static Vector3 GetFirstDerivative(Vector3 _p0, Vector3 _p1, Vector3 _p2, float _t) {
			return (2f * (1f - _t) * (_p1 - _p0)) + (2f * _t * (_p2 - _p1));
		}


		//----------------------
		// Cubic
		//----------------------
		public static Vector3 GetPoint(Vector3 _p0, Vector3 _p1, Vector3 _p2, Vector3 _p3, float _t) {
			_t = Mathf.Clamp01(_t);
			float oneMinusT = 1f - _t;
			return (oneMinusT * oneMinusT * oneMinusT * _p0) + (3f * oneMinusT * oneMinusT * _t * _p1) + (3f * oneMinusT * _t * _t * _p2) + (_t * _t * _t * _p3);
		}

		public static Vector3 GetFirstDerivative(Vector3 _p0, Vector3 _p1, Vector3 _p2, Vector3 _p3, float _t) {
			_t = Mathf.Clamp01(_t);
			float oneMinusT = 1f - _t;
			return (3f * oneMinusT * oneMinusT * (_p1 - _p0)) + (6f * oneMinusT * _t * (_p2 - _p1)) + (3f * _t * _t * (_p3 - _p2));
		}

		public static Vector3 GetDeCasteljausDerivatie(Vector3 _p0, Vector3 _p1, Vector3 _p2, Vector3 _p3, float _t) {
			Vector3 dU = _t * _t * (-3f * (_p0 - 3f * (_p1 - _p2) - _p3));
			dU += _t * (6f * (_p0 - (2f * _p1) + _p2));
			dU += -3f * (_p0 - _p1);
			return dU;
		}
	}
}