using UnityEngine;
using System;

namespace BSpline {
	public class BezierSplineQuadratic : MonoBehaviour {

		[SerializeField] private Vector3[] m_points;
		[SerializeField] private bool m_loop;
		[SerializeField] private BezierControlPointMode[] m_modes;

		[SerializeField][HideInInspector] private float[] m_curveLength;
		[SerializeField][HideInInspector] private float[] m_curveLengthNormalized;
		[SerializeField][HideInInspector] private float m_splineLength;


		public int controlPointCount 	{ get { return m_points.Length; } }
		public int curveCount 			{ get { return (m_points.Length - 1) / 2; } }

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
				if (_normalized) {
					i = 0;
					float length = m_splineLength * _t;
					for (int l = 0; l < m_curveLength.Length; ++l) {
						if (length > m_curveLength[l]) {
							length -= m_curveLength[l];
						} else {
							_t = length / m_curveLength[l];
							i = l;
							break;
						}
					}
					i *= 3;
				} else {
					_t = Mathf.Clamp01(_t) * curveCount;
					i = (int)_t;
					_t -= i;
					i *= 3;
				}
			}

			return transform.TransformPoint(Bezier.GetPoint(m_points[i], m_points[i + 1], m_points[i + 2], m_points[i + 3], _t));
		}

		public Vector3 GetVelocity(float _t, bool _normalized = false) {
			int i;

			if (_t >= 1f) {
				_t = 1f;
				i = m_points.Length - 4;
			} else {
				if (_normalized) {
					i = 0;
					float length = m_splineLength * _t;
					for (int l = 0; l < m_curveLength.Length; ++l) {
						if (length > m_curveLength[l]) {
							length -= m_curveLength[l];
						} else {
							_t = length / m_curveLength[l];
							i = l;
							break;
						}
					}
					i *= 3;
				} else {
					_t = Mathf.Clamp01(_t) * curveCount;
					i = (int)_t;
					_t -= i;
					i *= 3;
				}
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

		public void Reset() {
			m_points = new Vector3[] {
				new Vector3(1f, 0f, 0f),
				new Vector3(2f, 0f, 0f),
				new Vector3(3f, 0f, 0f),
				new Vector3(4f, 0f, 0f)
			};

			m_curveLength = new float[1];
			m_curveLengthNormalized = new float[1];
			m_splineLength = 0f;

			m_modes = new BezierControlPointMode[] { BezierControlPointMode.Free, BezierControlPointMode.Free };

			CalculateCurveLength();
		}

		public void AddCurve() {
			Vector3 point = m_points[m_points.Length - 1];
			Array.Resize(ref m_points, m_points.Length + 3);
			Array.Resize(ref m_curveLength, m_curveLength.Length + 1);
			Array.Resize(ref m_curveLengthNormalized, m_curveLengthNormalized.Length + 1);

			point.x += 1f;
			m_points[m_points.Length - 3] = point;
			point.x += 1f;
			m_points[m_points.Length - 2] = point;
			point.x += 1f;
			m_points[m_points.Length - 1] = point;

			Array.Resize(ref m_modes, m_modes.Length + 1);
			m_modes[m_modes.Length - 1] = m_modes[m_modes.Length - 2];
			EnforceMode(m_points.Length - 4);

			if (loop) {
				m_points[m_points.Length - 1] = m_points[0];
				m_modes[m_modes.Length - 1] = m_modes[0];
				EnforceMode(0);
			}

			CalculateCurveLength();
		}

		public void CalculateCurveLength() {
			m_splineLength = 0f;

			for (int i = 0; i < curveCount; ++i) {
				int index = i * 3;

				m_curveLength[i] = 0;

				Vector3 pointA = m_points[index];
				for (float t = 0; t < 1f; t += 0.01f) {
					Vector3 pointB = Bezier.GetPoint(m_points[index], m_points[index + 1], m_points[index + 2], m_points[index + 3], t);

					m_curveLength[i] += (pointB - pointA).magnitude;
					pointA = pointB;
				}

				m_splineLength += m_curveLength[i];
			}

			for (int i = 0; i < curveCount; ++i) {
				m_curveLengthNormalized[i] = m_curveLength[i] / m_splineLength;
			}
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
	}
}