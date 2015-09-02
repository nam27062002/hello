﻿using UnityEditor;
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
			DrawHandle(m_target.bounds);

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
			m_target.radius = EditorGUILayout.FloatField("Radius", m_target.radius);
			m_target.color = EditorGUILayout.ColorField("Color", m_target.color);

			if (GUI.changed) {
				EditorUtility.SetDirty(m_target);
			}
		}
	}

	private bool UpdateCenterHandle() {

		EditorGUI.BeginChangeCheck();
		Vector3 position = MoveHandle(m_target.bounds.center);
		
		if (EditorGUI.EndChangeCheck()) {

			m_target.offset += (Vector2)(position - m_target.bounds.center);
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

	private void DrawHandle(Bounds _bounds) {

		m_radius 	= _bounds.extents.x;

		m_center	= _bounds.center;
		m_right 	= new Vector3(_bounds.max.x, _bounds.center.y, _bounds.max.z);
		m_left 		= new Vector3(_bounds.min.x, _bounds.center.y, _bounds.max.z);
		m_top 		= new Vector3(_bounds.center.x, _bounds.max.y, _bounds.max.z);
		m_bottom 	= new Vector3(_bounds.center.x, _bounds.min.y, _bounds.max.z);

		Handles.color =  m_target.color;
		Handles.DrawSolidDisc(m_center, Vector3.forward, m_radius);

		Color outline = m_target.color;
		outline.a = 1f;
		
		Handles.color = outline;
		Handles.DrawWireDisc(m_center, Vector3.forward, m_radius);
	}

	private Vector3 MoveHandle(Vector3 _pos) {

		Handles.color = new Color(0.76f, 0.23f, 0.13f, 1f);
		return Handles.FreeMoveHandle(_pos, Quaternion.identity, 12f, Vector3.zero, Handles.DotCap);
	}
}
