using UnityEditor;
using UnityEngine;

namespace BSpline {
	[CustomEditor(typeof(BezierSpline))]
	public class BezierSplineEditor : Editor {

		private const int LINE_STEPS = 10;
		private const float DIR_SCALE = 0.5f;
		private const int STEPS_PER_CURVE = 10;
		private const float HANDLE_SIZE = 0.04f;
		private const float PICK_SIZE = 0.06f;

		private static Color[] modeColors = {
			Color.white,
			Color.yellow,
			Color.cyan
		};

		private BezierSpline m_spline;
		private Transform m_handleTransform;
		private Quaternion m_handleRotation;

		private int m_selectedIndex = -1;


		public override void OnInspectorGUI () {			
			m_spline = target as BezierSpline;

			EditorGUI.BeginChangeCheck();
			int segments = EditorGUILayout.IntField("Segments per curve", m_spline.segmentsPerSpline);
			if (EditorGUI.EndChangeCheck()) {
				Undo.RecordObject(m_spline, "Change segments");
				EditorUtility.SetDirty(m_spline);
				m_spline.segmentsPerSpline = segments;
				m_spline.CalculateArcLength();
			}

			EditorGUI.BeginChangeCheck();
			bool loop = EditorGUILayout.Toggle("Loop", m_spline.loop);
			if (EditorGUI.EndChangeCheck()) {
				Undo.RecordObject(m_spline, "Toggle Loop");
				EditorUtility.SetDirty(m_spline);
				m_spline.loop = loop;
			}

			if (m_selectedIndex >= 0 && m_selectedIndex < m_spline.controlPointCount) {
				DrawSelectedPointInspector();
			}

			if (GUILayout.Button("Add Curve")) {
				Undo.RecordObject(m_spline, "Add Curve");
				m_spline.AddSpline();
				EditorUtility.SetDirty(m_spline);
			}

			if (GUILayout.Button("Reset")) {
				Undo.RecordObject(m_spline, "Reset");
				m_spline.Reset();
				EditorUtility.SetDirty(m_spline);
			}
		}

		private void DrawSelectedPointInspector() {
			GUILayout.Label("Selected Point");
			EditorGUI.BeginChangeCheck();
			Vector3 point = EditorGUILayout.Vector3Field("Position", m_spline.GetControlPoint(m_selectedIndex));
		
			if (EditorGUI.EndChangeCheck()) {
				Undo.RecordObject(m_spline, "Move Point");
				EditorUtility.SetDirty(m_spline);
				m_spline.SetControlPoint(m_selectedIndex, point);
				m_spline.CalculateArcLength();
			}

			EditorGUI.BeginChangeCheck();
			BezierControlPointMode mode = (BezierControlPointMode)EditorGUILayout.EnumPopup("Mode", m_spline.GetControlPointMode(m_selectedIndex));
			if (EditorGUI.EndChangeCheck()) {
				Undo.RecordObject(m_spline, "Change Point Mode");
				m_spline.SetControlPointMode(m_selectedIndex, mode);
				m_spline.CalculateArcLength();
				EditorUtility.SetDirty(m_spline);
			}

			if (GUILayout.Button("Delete Curve")) {
				Undo.RecordObject(m_spline, "Delete Curve");
				m_spline.DeleteSpline(m_selectedIndex);
				EditorUtility.SetDirty(m_spline);
			}
		}

		private void OnSceneGUI() {
			m_spline = target as BezierSpline;
			m_handleTransform = m_spline.transform;
			m_handleRotation = Tools.pivotRotation == PivotRotation.Local ? m_handleTransform.rotation : Quaternion.identity;

			Vector3 p0 = ShowPoint(0);
			for (int i = 1; i < m_spline.controlPointCount; i += 3) {				
				Vector3 p1 = ShowPoint(i);
				Vector3 p2 = ShowPoint(i + 1);
				Vector3 p3 = ShowPoint(i + 2);

				Handles.color = Color.gray;
				Handles.DrawLine(p0, p1);
				Handles.DrawLine(p2, p3);

				Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 2f);
				p0 = p3;
			}

			ShowDirections();
		}

		private void ShowDirections() {
			Handles.color = Color.green;
			Vector3 point = m_spline.GetPoint(0f);
			Handles.DrawLine(point, point + m_spline.GetDirection(0f) * DIR_SCALE);

			int steps = STEPS_PER_CURVE * m_spline.splineCount;

			for (int i = 1; i <= steps; i++) {
				point = m_spline.GetPoint(i / (float)steps);
				Handles.DrawLine(point, point + m_spline.GetDirection(i / (float)steps) * DIR_SCALE);
			}
		}

		private Vector3 ShowPoint(int _index) {
			Vector3 point = m_handleTransform.TransformPoint(m_spline.GetControlPoint(_index));
			float size = HandleUtility.GetHandleSize(point) * 2f;

			if (_index == 0) {
				size *= 2f;
			}

			Handles.color = modeColors[(int)m_spline.GetControlPointMode(_index)];
			if (Handles.Button(point, m_handleRotation, size * HANDLE_SIZE, size * PICK_SIZE, Handles.DotHandleCap)) {
				m_selectedIndex = _index;
				Repaint();
			}

			if (m_selectedIndex == _index) {
				EditorGUI.BeginChangeCheck();
				point = Handles.DoPositionHandle(point, m_handleRotation);

				if (EditorGUI.EndChangeCheck()) {
					Undo.RecordObject(m_spline, "Move Point");
					EditorUtility.SetDirty(m_spline);
					m_spline.SetControlPoint(_index, m_handleTransform.InverseTransformPoint(point));
					m_spline.CalculateArcLength();
				}
			}
			return point;
		}
	}
}