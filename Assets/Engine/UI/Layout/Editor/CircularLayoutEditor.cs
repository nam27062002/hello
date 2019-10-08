// CircularLayoutEditor.cs
// Battlegrounds Proto
// 
// Created by Alger Ortín Castellví on 07/10/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom editor for the CircularLayout class.
/// </summary>
[CustomEditor(typeof(CircularLayout), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class CircularLayoutEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	private CircularLayout m_targetCircularLayout = null;

	// Cache some properties
	private SerializedProperty m_minAngleProp = null;
	private SerializedProperty m_maxAngleProp = null;

	// Cache some content as well
	private static GUIContent s_minMaxPrefixLabelContent = new GUIContent("Angle Range", "Layout's arc angle range");
	private static GUIContent s_minLabelContent = new GUIContent("min", "Layout's arc start angle");
	private static GUIContent s_maxLabelContent = new GUIContent("max", "Layout's arc end angle");

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetCircularLayout = target as CircularLayout;

		// Cache some properties
		m_minAngleProp = serializedObject.FindProperty("m_minAngle");
		m_maxAngleProp = serializedObject.FindProperty("m_maxAngle");
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_targetCircularLayout = null;

		// Clear cached properties
		m_minAngleProp = null;
		m_maxAngleProp = null;
	}

	/// <summary>
	/// Draw the inspector.
	/// </summary>
	public override void OnInspectorGUI() {
		// Instead of modifying script variables directly, it's advantageous to use the SerializedObject and 
		// SerializedProperty system to edit them, since this automatically handles private fields, multi-object 
		// editing, undo, and prefab overrides.

		// Update the serialized object - always do this in the beginning of OnInspectorGUI.
		serializedObject.Update();

		// Unity's "script" property - draw disabled
		EditorGUI.BeginDisabledGroup(true);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"), true);
		EditorGUI.EndDisabledGroup();

		// Loop through all serialized properties and work with special ones
		SerializedProperty p = serializedObject.GetIterator();
		p.Next(true);	// To get first element
		do {
			// Properties requiring special treatment
			// Properties we don't want to show
			if(p.name == "m_ObjectHideFlags" || p.name == "m_Script") {
				// Do nothing
			}

			// Min/Max angle: show in the same line
			else if(p.name == m_minAngleProp.name) {
				// Join in a horizontal group
				EditorGUILayout.Space();
				EditorGUILayout.BeginHorizontal();

				// Show prefix label
				EditorGUILayout.PrefixLabel(s_minMaxPrefixLabelContent);

				// Reset indent level after the prefix label
				int indentLevel = EditorGUI.indentLevel;
				EditorGUI.indentLevel = 0;

				// Adjust label width to fit "min" and "max" words
				float labelWidth = EditorGUIUtility.labelWidth;
				EditorGUIUtility.labelWidth = 30f;

				// Warning color if values are weird
				bool weirdValues = m_minAngleProp.floatValue > m_maxAngleProp.floatValue;
				Color contentColorBackup = GUI.contentColor;
				if(weirdValues) {
					GUI.contentColor = Color.yellow;
				}

				// Show min and max float fields
				EditorGUILayout.PropertyField(m_minAngleProp, s_minLabelContent, true);
				EditorGUILayout.PropertyField(m_maxAngleProp, s_maxLabelContent, true);

				// Restore gui settings
				GUI.contentColor = contentColorBackup;
				EditorGUIUtility.labelWidth = labelWidth;
				EditorGUI.indentLevel = indentLevel;

				// Finish horizontal group
				EditorGUILayout.EndHorizontal();

				// Throw a warning if values are weird
				if(weirdValues) {
					EditorGUILayout.HelpBox("Possible invalid angle values.\nAre you sure that's what you want?", MessageType.Warning);
				}
			}

			else if(p.name == m_maxAngleProp.name) {
				// Already done together with the minAngle
			}

			// Default property display
			else {
				EditorGUILayout.PropertyField(p, true);
			}
		} while(p.NextVisible(false));      // Only direct children, not grand-children (will be drawn by default if using the default EditorGUI.PropertyField)

		// Apply changes to the serialized object - always do this in the end of OnInspectorGUI.
		serializedObject.ApplyModifiedProperties();
	}

	/// <summary>
	/// The scene is being refreshed.
	/// </summary>
	public void OnSceneGUI() {
		// Scene-related stuff
	}

    /// <summary>
	/// Draw gizmos for the CircularLayout component.
	/// </summary>
    [DrawGizmo(GizmoType.Selected | GizmoType.Active)]
    public static void DrawGizmos(CircularLayout _target, GizmoType _gizmoType) {
		// Nothing if component is not enabled
		if(!_target.isActiveAndEnabled) return;

		// Aux vars
		float angleDistance = Mathf.Abs(_target.maxAngle - _target.minAngle);
		float correctedMinAngle = _target.CorrectAngle(_target.minAngle);
		float correctedMaxAngle = _target.CorrectAngle(_target.maxAngle);

		// Set Handles matrix to this object's transform
		RectTransform rt = _target.transform as RectTransform;
		Handles.matrix = rt.localToWorldMatrix;

		// Draw angle range
		Handles.color = new Color(0.2f, 0.8f, 1f, 0.15f);	// Translucid Sky Blue
		Handles.DrawSolidArc(
			Vector3.zero,
			Vector3.back,
			RotateXYDegrees(Vector3.right, correctedMinAngle),
			_target.clockwise ? angleDistance : -angleDistance,
			_target.radius
		);

		// Draw origin ref
		Handles.color = Color.yellow;
		Handles.DrawLine(
			Vector3.zero,
			RotateXYDegrees(Vector3.right * _target.radius, _target.CorrectAngle(0f))
		);

		// Draw min angle
		Handles.color = Color.red;
		Handles.DrawLine(
			Vector3.zero,
			RotateXYDegrees(Vector3.right * _target.radius, correctedMinAngle)
		);

		// Draw max angle
		Handles.color = Color.red;
		Handles.DrawLine(
			Vector3.zero,
			RotateXYDegrees(Vector3.right * _target.radius, correctedMaxAngle)
		);

		// Draw arrow body
		Handles.color = Color.red;
		float arrowAngleDist = Mathf.Min(angleDistance * 0.30f, 60f);    // Xdeg or 30% of the arc if distance is less than Xdeg
		arrowAngleDist = _target.clockwise ? arrowAngleDist : -arrowAngleDist;
		Handles.DrawWireArc(
			Vector3.zero,
			Vector3.back,
			RotateXYDegrees(Vector3.right, correctedMinAngle),
			arrowAngleDist,
			_target.radius
		);

		// Draw arrow tip
		float arrowAngle = _target.CorrectAngle(_target.minAngle + (_target.clockwise ? arrowAngleDist : -arrowAngleDist));
		Vector3 arrowTipPoint = Vector3.right * _target.radius;
		arrowTipPoint = RotateXYDegrees(arrowTipPoint, arrowAngle);
		Handles.matrix = Handles.matrix * Matrix4x4.Translate(arrowTipPoint);
		Handles.matrix = Handles.matrix * Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, arrowAngle));
		float arrowWingsSize = _target.radius * 0.1f; // Proportional to the radius
		Handles.DrawAAPolyLine(
			new Vector3(1f, _target.clockwise ? 1f : -1f, 0f).normalized * arrowWingsSize,
			Vector3.zero,
			new Vector3(-1f, _target.clockwise ? 1f : -1f, 0f).normalized * arrowWingsSize
		);

		// Restore Handles matrix and color :)
		Handles.matrix = Matrix4x4.identity;
		Handles.color = Color.white;
	}

	/// <summary>
	/// Rotate the given vector in the XY plane.
	/// </summary>
	/// <returns>The original vector rotated in the XY plane.</returns>
	/// <param name="_v">Vector to be rotated.</param>
	/// <param name="_angle">Angle to rotate.</param>
	private static Vector3 RotateXYDegrees(Vector3 _v, float _angle) {
		_angle *= Mathf.Deg2Rad;
		float sin = Mathf.Sin(_angle);
		float cos = Mathf.Cos(_angle);
		float x = _v.x;
		float y = _v.y;
		return new Vector3(x * cos - y * sin, x * sin + y * cos);
	}
}