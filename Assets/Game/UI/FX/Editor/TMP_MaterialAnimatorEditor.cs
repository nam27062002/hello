// TMP_MaterialAnimatorEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 04/03/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom editor for the TMP_MaterialAnimator class.
/// </summary>
[CustomEditor(typeof(TMP_MaterialAnimator), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class TMP_MaterialAnimatorEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private static GUIStyle s_headerStyle = null;

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	TMP_MaterialAnimator m_targetTMP_MaterialAnimator = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetTMP_MaterialAnimator = target as TMP_MaterialAnimator;
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_targetTMP_MaterialAnimator = null;
	}

	/// <summary>
	/// Draw the inspector.
	/// </summary>
	public override void OnInspectorGUI() {
		// Make sure we have valid styles
		if(s_headerStyle == null) {
			s_headerStyle = new GUIStyle("ShurikenModuleTitle");
			s_headerStyle.fixedHeight = 25f;
		}

		// Instead of modifying script variables directly, it's advantageous to use the SerializedObject and 
		// SerializedProperty system to edit them, since this automatically handles private fields, multi-object 
		// editing, undo, and prefab overrides.

		// Update the serialized object - always do this in the beginning of OnInspectorGUI.
		serializedObject.Update();

		// Unity's "script" property - draw disabled
		EditorGUI.BeginDisabledGroup(true);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"), true);
		EditorGUI.EndDisabledGroup();

		// Aux vars
		SerializedProperty currentGroupProp = null;

		// Loop through all serialized properties and work with special ones
		SerializedProperty p = serializedObject.GetIterator();
		p.Next(true);	// To get first element
		do {
			// Properties we don't want to show
			if(p.name == "m_ObjectHideFlags"
			|| p.name == "m_Script"
			|| p.name.Contains("Toggle")) { // See below
				// If they have a Header attribute attached to it, draw it
				object[] attributes = p.GetAttributes<HeaderAttribute>();
				if(attributes.Length > 0) {
					// If current group is expanded, draw a space for breathing
					if(currentGroupProp != null && currentGroupProp.isExpanded) {
						GUILayout.Space(10f);
					}

					// Do header
					DrawHeader(p, attributes[0] as HeaderAttribute);

					// Store current group
					currentGroupProp = p.Copy();
				}
			}

			// Rest of properties - only if not in a group or current group is expanded
			else if(currentGroupProp == null || currentGroupProp.isExpanded) {
				// Each property in the TMP_MaterialAnimator should have a "Toggle" property with the same name
				// If so, do fancy stuff
				SerializedProperty toggleProp = serializedObject.FindProperty(p.name + "Toggle");
				if(toggleProp != null) {
					// Toggle and property in the same line
					EditorGUILayout.BeginHorizontal(); {
						// No indentation within the horizontal group
						int indentLevelBckp = EditorGUI.indentLevel;
						EditorGUI.indentLevel = 0;

						// If within a group, simulate indentation by adding a space
						if(currentGroupProp != null) {
							GUILayout.Space(10f);
						}

						// Toggle
						toggleProp.boolValue = GUILayout.Toggle(toggleProp.boolValue, GUIContent.none, GUILayout.Width(10f));

						// Value - disable if toggled off
						EditorGUI.BeginDisabledGroup(!toggleProp.boolValue);
						EditorGUILayout.PropertyField(p, true);
						EditorGUI.EndDisabledGroup();

						// Restore indentation
						EditorGUI.indentLevel = indentLevelBckp;
					} EditorGUILayoutExt.EndHorizontalSafe();
				} else {
					// Default property display
					EditorGUILayout.PropertyField(p, true);
				}
			}
		} while(p.NextVisible(false));		// Only direct children, not grand-children (will be drawn by default if using the default EditorGUI.PropertyField)

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
	/// Custom header attribute drawing method.
	/// </summary>
	/// <param name="_p">The owner property of the attribute.</param>
	/// <param name="_header">The header to be done.</param>
	private void DrawHeader(SerializedProperty _p, HeaderAttribute _header) {
		// Customize text
		string text = Color.gray.Tag(_p.isExpanded ? "▼ " : "▶ ") + _header.header; // Add expanded arrow (a bit darker than actual text)
		text = "<size=12><b>" + text + "</b></size>";	// Make it a bit bigger! (and bold)

		// Draw header as a toggle so it's clickable
		_p.isExpanded = GUILayout.Toggle(_p.isExpanded, text, s_headerStyle/*, GUILayout.Height(30f)*/);
	}
}