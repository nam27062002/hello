// LevelDataEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 13/12/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom editor for the LevelData class.
/// </summary>
[CustomEditor(typeof(LevelData), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class LevelDataEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	LevelData m_targetLevelData = null;

	// Interesting properties
	SerializedProperty m_boundsProp = null;
	SerializedProperty m_boundsColorProp = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetLevelData = target as LevelData;

		// Get interesting properties
		m_boundsProp = serializedObject.FindProperty("m_bounds");
		m_boundsColorProp = serializedObject.FindProperty("m_boundsColor");

		// For real Unity? Editors for ScriptableObjects don't call the OnSceneGUI message -_-
		SceneView.onSceneGUIDelegate += OnSceneGUI;
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_targetLevelData = null;

		// For real Unity? Editors for ScriptableObjects don't call the OnSceneGUI message -_-
		SceneView.onSceneGUIDelegate -= OnSceneGUI;
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

		// Loop through all serialized properties and work with special ones
		SerializedProperty p = serializedObject.GetIterator();
		p.Next(true);	// To get first element
		do {
			// Properties requiring special treatment
			// Unity's "script" property
			if(p.name == "m_Script") {
				// Draw the property, disabled
				bool wasEnabled = GUI.enabled;
				GUI.enabled = false;
				EditorGUILayout.PropertyField(p, true);
				GUI.enabled = wasEnabled;
			}

			// Bounds
			else if(p.name == m_boundsProp.name) {
				// Since we're not using the default drawer, the separator attribute is ignored -_-
				EditorGUILayoutExt.Separator(new SeparatorAttribute("Other Data"));

				// Easier to just define left-right-top-bottom coords
				Rect newBounds = new Rect();
				EditorGUI.BeginChangeCheck();

				EditorGUILayout.BeginHorizontal(); {
					EditorGUILayout.PrefixLabel(p.displayName);
					EditorGUILayout.BeginVertical(); {
						EditorGUILayout.BeginHorizontal(); {
							EditorGUIUtility.labelWidth = 40f;
							newBounds.xMin = EditorGUILayout.FloatField("X Min", p.rectValue.xMin);
							newBounds.xMax = EditorGUILayout.FloatField("X Max", p.rectValue.xMax);
						} EditorGUILayout.EndHorizontal();

						EditorGUILayout.BeginHorizontal(); {
							newBounds.yMin = EditorGUILayout.FloatField("Y Min", p.rectValue.yMin);
							newBounds.yMax = EditorGUILayout.FloatField("Y Max", p.rectValue.yMax);
							EditorGUIUtility.labelWidth = 0f;
						} EditorGUILayout.EndHorizontal();
					} EditorGUILayout.EndVertical();
				} EditorGUILayout.EndHorizontal();

				// If something changed, validate new values
				if(EditorGUI.EndChangeCheck()) {
					Validate(p.rectValue, ref newBounds);
				}
				p.rectValue = newBounds;
			}

			// Properties we don't want to show
			else if(p.name == "m_ObjectHideFlags") {
				// Do nothing
			}

			// Default property display
			else {
				EditorGUILayout.PropertyField(p, true);
			}
		} while(p.NextVisible(false));		// Only direct children, not grand-children (will be drawn by default if using the default EditorGUI.PropertyField)

		// Apply changes to the serialized object - always do this in the end of OnInspectorGUI.
		serializedObject.ApplyModifiedProperties();
	}

	/// <summary>
	/// The scene is being refreshed.
	/// </summary>
	Vector3 m_testPos = Vector3.zero;
	public void OnSceneGUI(SceneView _sceneView) {
		// Scene-related stuff
		Handles.matrix = Matrix4x4.identity;
		Rect bounds = m_boundsProp.rectValue;	// Shorter notation, since it's editor code we don't care (much) about performance

		// Draw bounds in the world space
		Vector3 tl = new Vector3(bounds.xMin, bounds.yMin, 0f);
		Vector3 tr = new Vector3(bounds.xMax, bounds.yMin, 0f);
		Vector3 bl = new Vector3(bounds.xMin, bounds.yMax, 0f);
		Vector3 br = new Vector3(bounds.xMax, bounds.yMax, 0f);

		Handles.color = m_boundsColorProp.colorValue;
		Handles.DrawPolyLine(tl, tr, br, bl, tl);

		// Allow modifying via arrows
		Handles.color = Colors.white;
		Range handleSize = new Range(10f, 75f);

		Vector3 xMin = new Vector3(bounds.xMin, bounds.center.y, 0f);
		xMin = Handles.FreeMoveHandle(xMin, Quaternion.identity, handleSize.Clamp(HandleUtility.GetHandleSize(xMin)), Vector3.zero, Handles.SphereCap);

		Vector3 xMax = new Vector3(bounds.xMax, bounds.center.y, 0f);
		xMax = Handles.FreeMoveHandle(xMax, Quaternion.identity, handleSize.Clamp(HandleUtility.GetHandleSize(xMax)), Vector3.zero, Handles.SphereCap);

		Vector3 yMin = new Vector3(bounds.center.x, bounds.yMin, 0f);
		yMin = Handles.FreeMoveHandle(yMin, Quaternion.identity, handleSize.Clamp(HandleUtility.GetHandleSize(yMin)), Vector3.zero, Handles.SphereCap);

		Vector3 yMax = new Vector3(bounds.center.x, bounds.yMax, 0f);
		yMax = Handles.FreeMoveHandle(yMax, Quaternion.identity, handleSize.Clamp(HandleUtility.GetHandleSize(yMax)), Vector3.zero, Handles.SphereCap);

		bounds.xMin = xMin.x;
		bounds.xMax = xMax.x;
		bounds.yMin = yMin.y;
		bounds.yMax = yMax.y;
		Validate(m_boundsProp.rectValue, ref bounds);

		// Apply changes to the serialized object - always do this in the end of OnInspectorGUI.
		if(m_boundsProp.rectValue != bounds) {
			m_boundsProp.rectValue = bounds;
			serializedObject.ApplyModifiedProperties();
		}
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Detects changed in _newBounds and validates them so the bounds have min 
	/// size 1x1 and always positive width and height.
	/// </summary>
	/// <param name="_oldBounds">Old bounds.</param>
	/// <param name="_newBounds">New bounds. Will be modified if needed.</param>
	private void Validate(Rect _oldBounds, ref Rect _newBounds) {
		// Min size 1x1, always positive width and height
		// Find out which value has changed
		if(_oldBounds.xMin != _newBounds.xMin) {
			_newBounds.xMin = Mathf.Min(_newBounds.xMin, _newBounds.xMax - 1f);
		}

		if(_oldBounds.xMax != _newBounds.xMax) {
			_newBounds.xMax = Mathf.Max(_newBounds.xMin + 1f, _newBounds.xMax);
		}

		if(_oldBounds.yMin != _newBounds.yMin) {
			_newBounds.yMin = Mathf.Min(_newBounds.yMin, _newBounds.yMax - 1f);
		}

		if(_oldBounds.yMax != _newBounds.yMax) {
			_newBounds.yMax = Mathf.Max(_newBounds.yMin + 1f, _newBounds.yMax);
		}
	}
}