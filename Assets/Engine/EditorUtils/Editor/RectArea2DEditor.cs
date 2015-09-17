using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(RectArea2D))]
public class RectArea2DEditor : Editor {


	private RectArea2D m_target;
	private Vector3[] m_vertexs;
	private Vector3 m_center;
	private Vector3 m_right;
	private Vector3 m_left;
	private Vector3 m_top;
	private Vector3 m_bottom;


	public void Awake() {
		m_target = (RectArea2D)target;
		m_vertexs = new Vector3[5];
	}

	void OnSceneGUI() {

		bool isTargetDirty = false;

		if (m_target) {
			DrawHandle(m_target.bounds.bounds);
		
			for (int i = 0; i < 4; i++) {
				isTargetDirty = isTargetDirty || UpdateVertexHandles(i);
			}

			isTargetDirty = isTargetDirty || UpdateCenterHandle();
			isTargetDirty = isTargetDirty || UpdateHorizontalHandles();
			isTargetDirty = isTargetDirty || UpdateVerticalHandles();

			if (isTargetDirty) {
				EditorUtility.SetDirty(m_target);
			}
		}
	}

	public override void OnInspectorGUI() {

		if (m_target) {

			GUI.changed = false;

			m_target.offset = EditorGUILayout.Vector2Field("Offset", m_target.offset);
			m_target.size = EditorGUILayout.Vector2Field("Size", m_target.size);
			m_target.color = EditorGUILayout.ColorField("Color", m_target.color);

			if (GUI.changed) {
				EditorUtility.SetDirty(m_target);
			}
		}
	}

	private bool UpdateVertexHandles(int _index) {
	
		EditorGUI.BeginChangeCheck();
		Vector3 position = MoveHandle(m_vertexs[_index]);

		if (EditorGUI.EndChangeCheck()) {

			Vector2 offset = (Vector2)(position - m_vertexs[_index]);						
			m_target.offset += offset * 0.5f;
			switch (_index) {
			case 1: offset.y *= -1;	break;
			case 2: offset 	 *= -1;	break;
			case 3: offset.x *= -1;	break;
			}
			m_target.size += offset;
			return true;
		}

		return false;
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
			m_target.offset.x += offset * 0.5f;
			m_target.size.x += offset;
			return true;
		}

		EditorGUI.BeginChangeCheck();
		position = MoveHandle(m_left);
		
		if (EditorGUI.EndChangeCheck()) {
			
			float offset = (position.x - m_left.x);
			m_target.offset.x += offset * 0.5f;
			m_target.size.x -= offset;			
			return true;
		}
		
		return false;
	}

	private bool UpdateVerticalHandles() {
		
		EditorGUI.BeginChangeCheck();
		Vector3 position = MoveHandle(m_top);
		
		if (EditorGUI.EndChangeCheck()) {
			
			float offset = (position.y - m_top.y);
			m_target.offset.y += offset * 0.5f;
			m_target.size.y += offset;
			return true;
		}
		
		EditorGUI.BeginChangeCheck();
		position = MoveHandle(m_bottom);
		
		if (EditorGUI.EndChangeCheck()) {
			
			float offset = (position.y - m_bottom.y);
			m_target.offset.y += offset * 0.5f;
			m_target.size.y -= offset;			
			return true;
		}
		
		return false;
	}

	private void DrawHandle(Bounds _bounds) {
				
		m_vertexs[0] = _bounds.max;
		m_vertexs[1] = new Vector3(_bounds.max.x, _bounds.min.y, _bounds.max.z);
		m_vertexs[2] = _bounds.min;
		m_vertexs[3] = new Vector3(_bounds.min.x, _bounds.max.y, _bounds.max.z);
		m_vertexs[4] = _bounds.max;

		m_center	= _bounds.center;
		m_right 	= new Vector3(_bounds.max.x, _bounds.center.y, _bounds.max.z);
		m_left 		= new Vector3(_bounds.min.x, _bounds.center.y, _bounds.max.z);
		m_top 		= new Vector3(_bounds.center.x, _bounds.max.y, _bounds.max.z);
		m_bottom 	= new Vector3(_bounds.center.x, _bounds.min.y, _bounds.max.z);

		Handles.color = m_target.color;
		Handles.DrawAAConvexPolygon(m_vertexs);

		Color outline = m_target.color;
		outline.a = 1f;

		Handles.color = outline;
		Handles.DrawAAPolyLine(2f, m_vertexs);
	}

	private Vector3 MoveHandle(Vector3 _pos) {

		Handles.color = new Color(0.76f, 0.23f, 0.13f, 1f);
		float size = HandleUtility.GetHandleSize(Vector3.zero) * 0.05f;
		return Handles.FreeMoveHandle(_pos, Quaternion.identity, size, Vector3.zero, Handles.DotCap);
	}
}
