﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(PathIA))]
public class PathIAEditor : Editor {

	private PathIA m_target;

	// Use this for initialization
	void Awake() {
		m_target = (PathIA)target;
	}

	void OnSceneGUI() {	
		if (m_target != null) {
			List<Vector3> segments = new List<Vector3>();

			Debug.Log(Event.current.type);

			DrawSegmentSplitters(ref segments);
			DrawPoints();
			DrawLines();

			// Check Event
			Event e = Event.current;
			if (e.type == EventType.MouseDown) {

				// ----------------------------------------------------------------------------------------------------
				int splitIndex = -1;
				for (int i = 0; i < segments.Count; i++) {
					Vector2 guiPoint = HandleUtility.WorldToGUIPoint(segments[i] + m_target.transform.position);				
										
					if ((e.mousePosition - guiPoint).magnitude < 15f) {
						splitIndex = i;
						break;
					}
				}

				if (splitIndex >= 0) {
					SerializedProperty p = serializedObject.FindProperty("m_points");
					p.arraySize++;
					for (int i = p.arraySize - 1; i > splitIndex; i--) {
						p.GetArrayElementAtIndex(i).vector3Value = p.GetArrayElementAtIndex(i - 1).vector3Value;
					}
					p.GetArrayElementAtIndex(splitIndex + 1).vector3Value = segments[splitIndex];
					serializedObject.ApplyModifiedProperties();
				} else {
					int deleteIndex = -1;				
					for (int i = 0; i < m_target.points.Count; i++) {
						Vector2 guiPoint = HandleUtility.WorldToGUIPoint(m_target.points[i] + m_target.transform.position);				
						
						if ((e.mousePosition - guiPoint).magnitude < 15f) {
							deleteIndex = i;
							break;
						}
					}

					if (deleteIndex >= 0) {
						SerializedProperty p = serializedObject.FindProperty("m_points");
						p.DeleteArrayElementAtIndex(deleteIndex);
						serializedObject.ApplyModifiedProperties();
					}
				}
				// ----------------------------------------------------------------------------------------------------
			}
		}
	}

	public override void OnInspectorGUI() {
		// ------------------
		// do inspector stuff
		// ------------------
		SerializedProperty p = serializedObject.FindProperty("m_points");
		//EditorGUILayout.PropertyField(p, true);

		if (p.arraySize < 2) {
			p.arraySize = 2;
			p.GetArrayElementAtIndex(0).vector3Value = Vector3.left;
			p.GetArrayElementAtIndex(1).vector3Value = Vector3.right;
		}

		serializedObject.ApplyModifiedProperties();
		// ------------------

		//m_target.UpdateBounds();
	}

	private void DrawPoints() {
		bool isTargetDirty = false;
		SerializedProperty p = serializedObject.FindProperty("m_points");
		float size = HandleUtility.GetHandleSize(Vector3.zero) * 0.15f;

		Handles.color = Color.yellow;
		for (int i = 0; i < p.arraySize; i++) {
			EditorGUI.BeginChangeCheck();
			Vector3 point = Handles.FreeMoveHandle(p.GetArrayElementAtIndex(i).vector3Value + m_target.transform.position, Quaternion.identity, size, Vector3.zero, Handles.SphereCap);
			if (EditorGUI.EndChangeCheck()) {
				p.GetArrayElementAtIndex(i).vector3Value = point - m_target.transform.position;
				isTargetDirty = true;
			}
		}

		if (isTargetDirty) {
			serializedObject.ApplyModifiedProperties();
		}
	}

	private void DrawSegmentSplitters(ref List<Vector3> _segments) {

		// Build segment list
		float size = HandleUtility.GetHandleSize(Vector3.zero) * 0.1f;
		SerializedProperty p = serializedObject.FindProperty("m_points");	
		int segmentCount = (p.arraySize < 3)? 1 : p.arraySize;

		Handles.color = Color.white;
		for (int i = 0; i < segmentCount; i++) {
			int left = i;
			int right = (i + 1) % p.arraySize;
			
			Vector3 pos = p.GetArrayElementAtIndex(left).vector3Value + (p.GetArrayElementAtIndex(right).vector3Value - p.GetArrayElementAtIndex(left).vector3Value) * 0.5f;
			Handles.FreeMoveHandle(pos + m_target.transform.position, Quaternion.identity, size, Vector3.zero, Handles.SphereCap);

			_segments.Add(pos);
		}
	}

	private void DrawLines() {
		SerializedProperty p = serializedObject.FindProperty("m_points");	
		int segmentCount = (p.arraySize < 3)? 1 : p.arraySize;
		Handles.color = Color.yellow;

		for (int i = 0; i < segmentCount; i++) {
			Handles.DrawLine(p.GetArrayElementAtIndex(i).vector3Value + m_target.transform.position, p.GetArrayElementAtIndex((i + 1) % p.arraySize).vector3Value + m_target.transform.position);
		}
	}
}
