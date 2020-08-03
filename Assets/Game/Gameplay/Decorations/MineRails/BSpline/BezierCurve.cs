using UnityEngine;

namespace BSpline {
	public class BezierCurve : MonoBehaviour {

		public Vector3[] points;

		private Transform m_transform;


		private void Start() {
			m_transform = transform;
		}

		public void Reset() {
			points = new Vector3[] {
				new Vector3(1f, 0f, 0f),
				new Vector3(2f, 0f, 0f),
				new Vector3(3f, 0f, 0f),
				new Vector3(4f, 0f, 0f)
			};
		}

		public Vector3 GetPoint(float _t) {
			return m_transform.TransformPoint(Bezier.GetPoint(points[0], points[1], points[2], points[3], _t));
		}

		public Vector3 GetVelocity(float _t) {
			return m_transform.TransformPoint(Bezier.GetFirstDerivative(points[0], points[1], points[2], points[3], _t)) - m_transform.position;
		}

		public Vector3 GetDirection(float _t) {
			return GetVelocity(_t).normalized;
		}
	}
}