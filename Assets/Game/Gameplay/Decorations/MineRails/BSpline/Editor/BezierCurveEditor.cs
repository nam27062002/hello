using UnityEditor;
using UnityEngine;

namespace BSpline {
	[CustomEditor(typeof(BezierCurve))]
	public class BezierCurveEditor : Editor {
		private const int LINE_STEPS = 10;
		private const float DIR_SCALE = 0.5f;

		private BezierCurve m_curve;
		private Transform m_handleTransform;
		private Quaternion m_handleRotation;


		private Vector3 ShowPoint(int _index) {
			Vector3 point = m_handleTransform.TransformPoint(m_curve.points[_index]);
			EditorGUI.BeginChangeCheck();
			point = Handles.DoPositionHandle(point, m_handleRotation);
			if (EditorGUI.EndChangeCheck()) {
				Undo.RecordObject(m_curve, "Move Point");
				EditorUtility.SetDirty(m_curve);
				m_curve.points[_index] = m_handleTransform.InverseTransformPoint(point);
			}
			return point;
		}

		private void OnSceneGUI() {
			m_curve = target as BezierCurve;
			m_handleTransform = m_curve.transform;
			m_handleRotation = Tools.pivotRotation == PivotRotation.Local ? m_handleTransform.rotation : Quaternion.identity;

			Vector3 p0 = ShowPoint(0);
			Vector3 p1 = ShowPoint(1);
			Vector3 p2 = ShowPoint(2);
			Vector3 p3 = ShowPoint(3);

			Handles.color = Color.gray;
			Handles.DrawLine(p0, p1);
			Handles.DrawLine(p2, p3);

			ShowDirections();
			Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 2f);
		}

		private void ShowDirections() {			
			Handles.color = Color.green;
			Vector3 point = m_curve.GetPoint(0f);
			Handles.DrawLine(point, point + m_curve.GetDirection(0f) * DIR_SCALE);

			for (int i = 1; i <= LINE_STEPS; i++) {
				point = m_curve.GetPoint(i / (float)LINE_STEPS);
				Handles.DrawLine(point, point + m_curve.GetDirection(i / (float)LINE_STEPS) * DIR_SCALE);
			}
		}
	}
}