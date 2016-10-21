// TiledPrefabEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 12/08/2016.
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
/// Custom editor for the TiledPrefab class.
/// Required since Unity doesn't allow us to call Destroy/DestroyImmediate during the OnValidate() event.
/// </summary>
[CustomEditor(typeof(PrefabLayout), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class PrefabLayoutEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	PrefabLayout m_targetPrefabLayout = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetPrefabLayout = target as PrefabLayout;

		// Make sure it's updated
		m_targetPrefabLayout.Refresh();

		// Subscribe to the Undo event
		#if UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5
		Undo.undoRedoPerformed += OnUndoRedo;
		#endif
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_targetPrefabLayout = null;

		// Unsubscribe from the Undo event
		#if UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5
		Undo.undoRedoPerformed -= OnUndoRedo;
		#endif
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

		// If a change occurs, refresh layout
		EditorGUI.BeginChangeCheck();

		// Loop through all serialized properties and work with special ones
		SerializedProperty p = serializedObject.GetIterator();
		p.NextVisible(true);	// To get first element
		do {
			// Properties requiring special treatment
			if(p.name == "m_setup") {
				// Foldable
				p.isExpanded = EditorGUILayout.Foldout(p.isExpanded, "Setup");
				if(p.isExpanded) {
					// Indent in
					EditorGUI.indentLevel++;

					// We should always have 3 elements in the property (XYZ)
					string[] labels = new string[] {"X", "Y", "Z"};
					p.arraySize = labels.Length;

					// Draw the 3 elements (X, Y, Z)
					for(int i = 0; i < labels.Length; i++) {
						// Custom property drawer will do it!
						EditorGUILayout.PropertyField(p.GetArrayElementAtIndex(i), new GUIContent(labels[i]));
					}

					// Indent back out
					EditorGUI.indentLevel--;
				}
			}

			// Debug
			else if(p.name == "m_debugColors") {
				// Foldable
				p.isExpanded = EditorGUILayout.Foldout(p.isExpanded, "Debug");
				if(p.isExpanded) {
					// Indent in
					EditorGUI.indentLevel++;

					// Get the flags property as well
					SerializedProperty gizmoTypesProp = serializedObject.FindProperty("m_debugGizmoTypes");

					// We should have exactly 4 values
					string[] labels = {"Total Bounds", "Section Bounds", "Padding", "Auto Padding"};
					p.arraySize = labels.Length;
					gizmoTypesProp.arraySize = labels.Length;

					// Draw them!
					SerializedProperty arrayElementProp;
					for(int i = 0; i < labels.Length; i++) {
						EditorGUILayout.BeginHorizontal(); {
							// Label
							EditorGUILayout.PrefixLabel(labels[i]);
							int indentLevelBackup = EditorGUI.indentLevel;
							EditorGUI.indentLevel = 0;

							// Gizmo type selector
							arrayElementProp = gizmoTypesProp.GetArrayElementAtIndex(i);
							EditorGUILayout.PropertyField(arrayElementProp, GUIContent.none, GUILayout.Width(60f));

							// Gizmo color picker
							arrayElementProp = p.GetArrayElementAtIndex(i);
							arrayElementProp.colorValue = EditorGUILayout.ColorField(arrayElementProp.colorValue);

							// Restore indentation
							EditorGUI.indentLevel = indentLevelBackup;
						} EditorGUILayout.EndHorizontal();
					}

					// Indent back out
					EditorGUI.indentLevel--;
				}
			}

			// To skip
			else if(p.name == "m_debugGizmoTypes") {
				// Skip
			}

			// Default property display
			else {
				EditorGUILayout.PropertyField(p, true);
			}
		} while(p.NextVisible(false));		// Only direct children, not grand-children (will be drawn by default if using the default EditorGUI.PropertyField)

		// Apply changes to the serialized object - always do this in the end of OnInspectorGUI.
		serializedObject.ApplyModifiedProperties();

		// If a change occurs, refresh layout
		if(EditorGUI.EndChangeCheck()) {
			m_targetPrefabLayout.Refresh();
		}

		// Error messages
		// Prefab is not set
		SerializedProperty prefabProp = serializedObject.FindProperty("m_sectionPrefab");
		GameObject sectionPrefab = (GameObject)prefabProp.objectReferenceValue;
		if(sectionPrefab == null) {
			EditorGUILayout.HelpBox("Section prefab not set.", MessageType.Error);
		} else {
			// Prefab doesn't have a MeshFilter component
			Renderer renderer = sectionPrefab.GetComponentInChildren<Renderer>();
			if(renderer == null) {
				EditorGUILayout.HelpBox("Section prefab must have a Renderer component.", MessageType.Error);
			}
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// And Undo/Redo action has been triggered
	/// </summary>
	private void OnUndoRedo() {
		// Update the layout, regardless of whether the undo/redo is related to this object
		m_targetPrefabLayout.Refresh();
	}
}