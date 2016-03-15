using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(CircleArea2D))]
public class CircleArea2DEditor : Editor {


	private CircleArea2D m_target;

	private float 	m_radius;
	private Vector3 m_center;
	private Vector3 m_right;
	private Vector3 m_left;
	private Vector3 m_top;
	private Vector3 m_bottom;


	public void Awake() {
		m_target = (CircleArea2D)target;
		m_radius = 0;
	}

	void OnSceneGUI() {
		bool isTargetDirty = false;

		if (m_target) {
			DrawHandle();

			isTargetDirty = isTargetDirty || UpdateCenterHandle();
			isTargetDirty = isTargetDirty || UpdateHorizontalHandles();
			isTargetDirty = isTargetDirty || UpdateVerticalHandles();

			if (isTargetDirty) {
				UpdateBounds();
				EditorUtility.SetDirty(m_target);
			}
		}
	}

	public override void OnInspectorGUI() {
		if (m_target) {
			GUI.changed = false;

			m_target.offset = EditorGUILayout.Vector2Field("Offset", m_target.offset);
			m_target.radius = EditorGUILayout.FloatField("Radius", m_target.radius);
			m_target.color = EditorGUILayout.ColorField("Color", m_target.color);

			if (GUI.changed) {
				UpdateBounds();
				EditorUtility.SetDirty(m_target);
			}
		}
	}

	private bool UpdateCenterHandle() {

		EditorGUI.BeginChangeCheck();
		Vector3 position = MoveHandle(m_center);
		
		if (EditorGUI.EndChangeCheck()) {

			m_target.offset += (Vector2)(position - m_center);
			return true;
		}
		
		return false;
	}

	private bool UpdateHorizontalHandles() {
		
		EditorGUI.BeginChangeCheck();
		Vector3 position = MoveHandle(m_right);
		
		if (EditorGUI.EndChangeCheck()) {

			float offset = (position.x - m_right.x);
			m_target.radius += offset;
			return true;
		}

		EditorGUI.BeginChangeCheck();
		position = MoveHandle(m_left);
		
		if (EditorGUI.EndChangeCheck()) {
			
			float offset = (position.x - m_left.x);
			m_target.radius -= offset;			
			return true;
		}
		
		return false;
	}

	private bool UpdateVerticalHandles() {
		
		EditorGUI.BeginChangeCheck();
		Vector3 position = MoveHandle(m_top);
		
		if (EditorGUI.EndChangeCheck()) {
			
			float offset = (position.y - m_top.y);
			m_target.radius += offset;
			return true;
		}
		
		EditorGUI.BeginChangeCheck();
		position = MoveHandle(m_bottom);
		
		if (EditorGUI.EndChangeCheck()) {
			
			float offset = (position.y - m_bottom.y);
			m_target.radius -= offset;			
			return true;
		}
		
		return false;
	}

	private void DrawHandle() {
		Bounds bounds = m_target.bounds.bounds;

		m_radius 	= m_target.radius;
		m_center	= m_target.center;

		m_right 	= new Vector3(m_center.x + m_target.radius, m_center.y, m_center.z);
		m_left 		= new Vector3(m_center.x - m_target.radius, m_center.y, m_center.z);
		m_top 		= new Vector3(m_center.x, m_center.y + m_target.radius, m_center.z);
		m_bottom 	= new Vector3(m_center.x, m_center.y - m_target.radius, m_center.z);

		Handles.color =  m_target.color;
		Handles.DrawSolidDisc(m_center, Vector3.forward, m_radius);

		Color outline = m_target.color;
		outline.a = 1f;
		
		Handles.color = outline;
		Handles.DrawWireDisc(m_center, Vector3.forward, m_radius);
	}

	private Vector3 MoveHandle(Vector3 _pos) {

		Handles.color = new Color(0.76f, 0.23f, 0.13f, 1f);
		float size = HandleUtility.GetHandleSize(Vector3.zero) * 0.05f;
		return Handles.FreeMoveHandle(_pos, Quaternion.identity, size, Vector3.zero, Handles.DotCap);
	}

	private void UpdateBounds() {
		Vector3 size = Vector3.one * m_target.radius * 2f;
		m_target.bounds.UpdateBounds(m_target.center, size);
	}
}
