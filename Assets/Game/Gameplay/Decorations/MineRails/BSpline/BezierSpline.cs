using System;
using System.Collections.Generic;
using UnityEngine;

namespace BSpline {
	public class BezierSpline : MonoBehaviour {
		public struct SplineSegment {
			public Vector3 p0;
			public Vector3 p1;
			public Vector3 direction;
			public float length;
		}

		[SerializeField] private int m_segmentsPerSpline = 10;
		public int segmentsPerSpline { get { return m_segmentsPerSpline; } set { m_segmentsPerSpline = value; } }

		[SerializeField] private Vector3[] m_points;
		[SerializeField] private bool m_loop;
		[SerializeField] private BezierControlPointMode[] m_modes;

		[SerializeField][HideInInspector] private List<SplineSegment> m_segments;
		[SerializeField][HideInInspector] private float[] m_splineLength;
		[SerializeField][HideInInspector] private float m_arcLength;
		public float length { get { return m_arcLength; } }

		private bool m_isDirty3D = false;
		public bool isDirty3D { get { bool isDirty = m_isDirty3D; m_isDirty3D = false; return isDirty; } }

		public int controlPointCount 	{ get { return m_points.Length; } }
		public int splineCount 			{ get { return (m_points.Length - 1) / 3; } }

		public bool loop {
			get { return m_loop; }
			set {
				m_loop = value;
				if (value == true) {
					m_modes[m_modes.Length - 1] = m_modes[0];
					SetControlPoint(0, m_points[0]);
				}
			}
		}

		public Vector3 	GetControlPoint(int _index) {
			return m_points[_index]; 
		}

		public void SetControlPoint(int _index, Vector3 _point) {
			if (_index % 3 == 0) {
				Vector3 delta = _point - m_points[_index];

				if (loop) {
					if (_index == 0) {
						m_points[1] += delta;
						m_points[m_points.Length - 2] += delta;
						m_points[m_points.Length - 1] = _point;
					} else if (_index == m_points.Length - 1) {
						m_points[0] = _point;
						m_points[1] += delta;
						m_points[_index - 1] += delta;
					} else {
						m_points[_index - 1] += delta;
						m_points[_index + 1] += delta;
					}
				} else {
					if (_index > 0) {
						m_points[_index - 1] += delta;
					}
					if (_index + 1 < m_points.Length) {
						m_points[_index + 1] += delta;
					}
				}
			}

			m_points[_index] = _point;
			EnforceMode(_index); 
		}

		public BezierControlPointMode GetControlPointMode(int _index) {
			return m_modes[(_index + 1) / 3]; 
		}

		public void SetControlPointMode(int _index, BezierControlPointMode _mode) {
			int modeIndex = (_index + 1) / 3;
			m_modes[modeIndex] = _mode;

			if (m_loop) {
				if (modeIndex == 0) {
					m_modes[m_modes.Length - 1] = _mode;
				} else if (modeIndex == m_modes.Length - 1) {
					m_modes[0] = _mode;
				}
			}

			EnforceMode(_index);
		}

		public Vector3 GetPoint(float _t, bool _normalized = false) {
			int i;

			if (_t >= 1f) {
				_t = 1f;
				i = m_points.Length - 4;
			} else {
				_t = Mathf.Clamp01(_t) * splineCount;
				i = (int)_t;
				_t -= i;
				i *= 3;
			}

			return transform.TransformPoint(Bezier.GetPoint(m_points[i], m_points[i + 1], m_points[i + 2], m_points[i + 3], _t));
		}

		public Vector3 GetVelocity(float _t, bool _normalized = false) {
			int i;

			if (_t >= 1f) {
				_t = 1f;
				i = m_points.Length - 4;
			} else {
				_t = Mathf.Clamp01(_t) * splineCount;
				i = (int)_t;
				_t -= i;
				i *= 3;
			}

			return transform.TransformPoint(Bezier.GetFirstDerivative(m_points[i], m_points[i + 1], m_points[i + 2], m_points[i + 3], _t)) - transform.position;
		}

		public Vector3 GetDirection(float _t, bool _normalized = false) {
			return GetVelocity(_t, _normalized).normalized;
		}

		public Vector3 GetUpVector(float _t, bool _normalized = false) {
			Vector3 forward = GetDirection(_t, _normalized);
			Vector3 right = Vector3.Cross(Vector3.up, forward);
			return Vector3.Cross(forward, right);
		}

		public Vector3 GetPointAtDistance(float _distance, ref Vector3 _direction, ref Vector3 _up, ref Vector3 _right) {
			if (m_segments == null) {
				CalculateArcLength();
			}

			SplineSegment data;
			if (_distance >= m_arcLength) {
				data = m_segments.Last();
				_distance = data.length;
			} else {
				int s = 0;
				while (s < m_segments.Count && _distance > m_segments[s].length) {
					_distance -= m_segments[s].length;
					s++;
				}
				if (s < m_segments.Count) {
					data = m_segments[s];
				} else {
					data = m_segments.Last();
					_distance = data.length;
				}
			}

			_direction = data.direction;

			Vector3 forward = data.direction;
			_right = Vector3.Cross(Vector3.up, forward);
			_up = Vector3.Cross(forward, _right);

			return data.p0 + data.direction * _distance;
		}


		public void Reset() {
			m_points = new Vector3[] {
				new Vector3(1f, 0f, 0f),
				new Vector3(2f, 0f, 0f),
				new Vector3(3f, 0f, 0f),
				new Vector3(4f, 0f, 0f)
			};

			m_splineLength = new float[1];
			m_arcLength = 0f;

			m_modes = new BezierControlPointMode[] { BezierControlPointMode.Free, BezierControlPointMode.Free };

			CalculateArcLength();
		}

		public void AddSpline() {
			SplineSegment lastSegment = m_segments.Last();
			Vector3 point = lastSegment.p1;
			Array.Resize(ref m_points, m_points.Length + 3);
			Array.Resize(ref m_splineLength, m_splineLength.Length + 1);

			//we'll create the new spline along last node direction
			point += lastSegment.direction;
			m_points[m_points.Length - 3] = point;
			point += lastSegment.direction;
			m_points[m_points.Length - 2] = point;
			point += lastSegment.direction;
			m_points[m_points.Length - 1] = point;

			Array.Resize(ref m_modes, m_modes.Length + 1);
			m_modes[m_modes.Length - 1] = m_modes[m_modes.Length - 2];
			EnforceMode(m_points.Length - 4);

			if (loop) {
				m_points[m_points.Length - 1] = m_points[0];
				m_modes[m_modes.Length - 1] = m_modes[0];
				EnforceMode(0);
			}

			CalculateArcLength();
		}

		private void EnforceMode(int _index) { 
			int modeIndex = (_index + 1) / 3; 

			BezierControlPointMode mode = m_modes[modeIndex];
			if (mode == BezierControlPointMode.Free || modeIndex == 0 || modeIndex == m_modes.Length - 1) {
				return;
			}

			int middleIndex = modeIndex * 3;
			int fixedIndex, enforcedIndex;
			if (_index <= middleIndex) {
				fixedIndex = middleIndex - 1;
				if (fixedIndex < 0) {
					fixedIndex = m_points.Length - 2;
				}
				enforcedIndex = middleIndex + 1;
				if (enforcedIndex >= m_points.Length) {
					enforcedIndex = 1;
				}
			} else {
				fixedIndex = middleIndex + 1;
				if (fixedIndex >= m_points.Length) {
					fixedIndex = 1;
				}
				enforcedIndex = middleIndex - 1;
				if (enforcedIndex < 0) {
					enforcedIndex = m_points.Length - 2;
				}
			}

			Vector3 middle = m_points[middleIndex];
			Vector3 enforcedTangent = middle - m_points[fixedIndex];
			if (mode == BezierControlPointMode.Aligned) {
				enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, m_points[enforcedIndex]);
			}
			m_points[enforcedIndex] = middle + enforcedTangent;
		}


		//------------------------------------
		// Utils
		//------------------------------------

		public void CalculateArcLength() {			
			m_arcLength = 0f;

			for (int i = 0; i < splineCount; ++i) {
				m_splineLength[i] = GetLengthSimpsonsRule(i, 0f, 1f);
				m_arcLength += m_splineLength[i];
			}

			SplitIntoSegments();

			m_isDirty3D = true;
		}

		// t0 and t1 are time values along all the splines. Values from 0 to splineCount
		private float GetLengthSimpsonsRule(float _t0, float _t1) {
			float multiplier = 1f;
			if (_t0 > _t1) {
				float tmp = _t0;
				_t0 = _t1;
				_t1 = tmp;
				multiplier = -1f;
			}

			int firstSpline = (int)_t0;
			_t0 -= firstSpline;

			int lastSpline = (int)_t1;
			if (lastSpline >= splineCount) {
				lastSpline = splineCount - 1;
			}
			_t1 -= lastSpline;

			if (firstSpline == lastSpline) {
				return GetLengthSimpsonsRule(firstSpline, _t0, _t1) * multiplier;
			} else {
				float length = m_splineLength[firstSpline] - GetLengthSimpsonsRule(firstSpline, 0f, _t0);

				for (int s = firstSpline + 1; s < lastSpline; ++s) {
					length += m_splineLength[s];
				}

				length += GetLengthSimpsonsRule(lastSpline, 0, _t1);

				return length * multiplier;
			}
		}

		private float GetLengthSimpsonsRule(int _spline, float _t0, float _t1) {
			// Resolution (must be an even number)
			int resolution = 20;

			// We have to divide the spline into sections
			float delta = (_t1 - _t0) / (float)resolution;

			// Everything multiplied by 1
			float x1 = GetArcLengthIntegrand(_spline, _t0) + GetArcLengthIntegrand(_spline, _t1);

			// Everythin multiplied by 2
			float x2 = 0f;
			for (int i = 2; i < resolution; i += 2) {
				float t = _t0 + delta * i;
				x2 += GetArcLengthIntegrand(_spline, t);
			}

			// Everything multiplied by 4
			float x4 = 0f;
			for (int i = 1; i < resolution; i += 2) {
				float t = _t0 + delta * i;
				x4 += GetArcLengthIntegrand(_spline, t);
			}

			// The final length
			float length = (delta / 3f) * (x1 + (4f * x4) + (2f * x2));
			return length;
		}

		// t is a time along all the splines, values from 0 to splineCount
		private float GetArcLengthIntegrand(float _t) {			
			int spline = (int)_t;
			if (spline >= splineCount) {
				spline = splineCount - 1;
			}
			_t -= spline;

			return GetArcLengthIntegrand(spline, _t);
		}

		// Get length of a spline at time t. Values from 0 to 1
		private float GetArcLengthIntegrand(int _spline, float _t) {
			int index = _spline * 3;
			Vector3 derivative = Bezier.GetDeCasteljausDerivatie(m_points[index], m_points[index + 1], m_points[index + 2], m_points[index + 3], _t);
			return derivative.magnitude;
		}

		private Vector3 GetPointLocal(float _t) {
			int i;

			if (_t >= 1f) {
				_t = 1f;
				i = m_points.Length - 4;
			} else {
				_t = Mathf.Clamp01(_t) * splineCount;
				i = (int)_t;
				_t -= i;
				i *= 3;
			}

			return Bezier.GetPoint(m_points[i], m_points[i + 1], m_points[i + 2], m_points[i + 3], _t);
		}

		private float Find_T_NewtonRaphsonsMethod(float _distance) {
			float t = 0;

			if (_distance >= m_arcLength) {
				t = splineCount;
			} else {
				int iterations = 0;
				float error = 0.001f;

				// Approximate t using Bisection Method
				float t_max = splineCount;
				float t_min = -0.1f;

				while (t_max - t_min > 0.5f) {
					float t_mid = (t_max + t_min) / 2f;

					float simpson_t_min = GetLengthSimpsonsRule(0, t_min) - _distance;
					float simpson_t_mid = GetLengthSimpsonsRule(0, t_mid) - _distance;

					if (simpson_t_min * simpson_t_mid < 0) {
						t_max = t_mid;
					} else {
						t_min = t_mid;
					}
				}

				float t_next = (t_max + t_min) / 2f; // this is an aproximated start value

				do {
					t = t_next;
					t_next = t - (GetLengthSimpsonsRule(0, t) - _distance) / GetArcLengthIntegrand(t);

					iterations++;
				} while ((iterations < 1000) && (Mathf.Abs(t_next - t) > error));
			}
		
			return t / splineCount;
		}

		private void SplitIntoSegments() {
			int segments = splineCount * m_segmentsPerSpline;
			if (m_segments == null) {
				m_segments = new List<SplineSegment>();
			}
			m_segments.Resize(segments, default(SplineSegment));


			float segmentLength = m_arcLength / (float)segments;
			float currentDistance = 0f + segmentLength;

			Vector3 lastPos = m_points[0];

			for (int s = 0; s < segments; ++s) {
				float t = Find_T_NewtonRaphsonsMethod(currentDistance);

				Vector3 pos = GetPointLocal(t);
				Vector3 dir = pos - lastPos;

				SplineSegment ss = m_segments[s];
				ss.p0 = lastPos;
				ss.p1 = pos;
				ss.direction = dir.normalized;
				ss.length = dir.magnitude;
				m_segments[s] = ss;

				lastPos = pos;

				currentDistance += segmentLength;
			}
		}


		//------------------------------------
		// Utils
		//------------------------------------

		private void OnDrawGizmosSelected() {
			if (m_segments == null) {
				CalculateArcLength();
			}

			for (int s = 0; s < m_segments.Count; ++s) {
				if (s % 2 == 0) Gizmos.color = Colors.WithAlpha(Color.red, 0.5f);
				else 			Gizmos.color = Colors.WithAlpha(Color.yellow, 0.5f);

				Vector3 p0 = transform.TransformPoint(m_segments[s].p0) + Vector3.up * 0.25f;
				Vector3 p1 = transform.TransformPoint(m_segments[s].p1) + Vector3.up * 0.25f;
				Gizmos.DrawLine(p0, p1);
			}
		}
	}
}