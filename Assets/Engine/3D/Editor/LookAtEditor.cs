// LookAtPointEditor.cs
// 
// Created by Alger Ortín Castellví on 29/05/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System;
using UnityEditor.SceneManagement;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom editor for the lookAt point script.
/// </summary>
[CustomEditor(typeof(LookAt))]
[CanEditMultipleObjects]
public class LookAtPointEditor : Editor {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Target objects and properties
	private LookAt m_target;
	private LookAt[] m_targets;
	private SerializedProperty m_lookAtLocalProp = null;
	private SerializedProperty m_lookAtObjectProp = null;
	private SerializedProperty m_editModeProp = null;

	// Internal editor stuff
	private Vector3[] m_points = new Vector3[2];
	private bool m_restoreMoveTool = false;

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Editor enabled.
	/// </summary>
	public void OnEnable() {
		// Cast targets
		m_target = target as LookAt;
		m_targets = new LookAt[targets.Length];
		for(int i = 0; i < targets.Length; i++) {
			m_targets[i] = targets[i] as LookAt;
		}

		// Serialized properties
		m_lookAtLocalProp = serializedObject.FindProperty("m_lookAtPointLocal");
		m_lookAtObjectProp = serializedObject.FindProperty("m_lookAtObject");
		m_editModeProp = serializedObject.FindProperty("m_editMode");

		// Init internal vars
		m_restoreMoveTool = false;
	}

	/// <summary>
	/// Editor disabled.
	/// </summary>
	public void OnDisable() {
		// Restore previous tool, provided another tool hasn't been selected while editing this object
		if(m_restoreMoveTool && Tools.current == Tool.View) {
			Tools.current = Tool.Move;
		}
	}

	/// <summary>
	/// Updates stuff on the inspector.
	/// </summary>
	public override void OnInspectorGUI() {
		// Try to stick to serialized properties, since they automatically manage
		// multi-object editing, undo/redo, etc.
		// Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
		serializedObject.Update();

		// In this case, a single property (m_lookAtPointLocal) can be edited by 4 different ways:
		// Global Coords, Local Coords, Relative Coords and Scene Handlers
		EditorGUILayout.LabelField("Point in the 3D space where the object is looking to, different coordinate systems", CustomEditorStyles.commentLabelLeft);
		EditorGUILayout.PrefixLabel("LookAt Point");
		EditorGUI.showMixedValue = m_lookAtLocalProp.hasMultipleDifferentValues;
		EditorGUI.indentLevel++;

		// Local coords: use a mix between serialized property and getter/setter
		GUI.changed = false;
		EditorGUILayout.PropertyField(m_lookAtLocalProp, new GUIContent("Local"));
		if(GUI.changed) {
			serializedObject.ApplyModifiedProperties();
			Undo.RecordObjects(m_targets, "LookAtPoint localPos changed");
			for(int i = 0; i < m_targets.Length; i++) {
				m_targets[i].lookAtPointLocal = m_targets[i].lookAtPointLocal;	// Just to use the setter with the new value
			}
			SceneView.RepaintAll();
			m_target.Update();
		}

		// Relative coords: use getter/setter
		GUI.changed = false;
		Vector3 newRelativePos = EditorGUILayout.Vector3Field("Relative", m_target.lookAtPointRelative);	// Show first object's value
		if(GUI.changed) {
			Undo.RecordObjects(m_targets, "LookAtPoint relativePos changed");
			for(int i = 0; i < m_targets.Length; i++) {
				m_targets[i].lookAtPointRelative = newRelativePos;
			}
			SceneView.RepaintAll();
			m_target.Update();
		}

		// Global coords: use getter/setter
		GUI.changed = false;
		Vector3 newGlobalPos = EditorGUILayout.Vector3Field("Global", m_target.lookAtPointGlobal);	// Show first object's value
		if(GUI.changed) {
			Undo.RecordObjects(m_targets, "LookAtPoint globalPos changed");
			for(int i = 0; i < m_targets.Length; i++) {
				m_targets[i].lookAtPointGlobal = newGlobalPos;
			}
			SceneView.RepaintAll();
			m_target.Update();
		}

		EditorGUI.indentLevel--;

		// Optional lookAt object
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("If lookAtObject is defined, lookAtPoint will be linked to the position of the object", CustomEditorStyles.commentLabelLeft);
		GUI.changed = false;
		EditorGUILayout.PropertyField(m_lookAtObjectProp, new GUIContent("LookAt Object"));
		if(GUI.changed) {
			serializedObject.ApplyModifiedProperties();
			Undo.RecordObjects(m_targets, "LookAtPoint lookAtObj changed");
			for(int i = 0; i < m_targets.Length; i++) {
				m_targets[i].lookAtObject = m_targets[i].lookAtObject;	// Just to use the setter with the new value
			}
			SceneView.RepaintAll();
			m_target.Update();
		}

		// Edit mode
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("How the object's position and lookAt are linked", CustomEditorStyles.commentLabelLeft);
		EditorGUILayout.PropertyField(m_editModeProp);

		// Save object
		serializedObject.ApplyModifiedProperties();
	}

	/// <summary>
	/// Updates stuff on the scene.
	/// </summary>
	public void OnSceneGUI() {
		if(target == null) return;

		// Apparently this is called for every selected object, so we don't need to manually handle multi-selection in here
		GUI.changed = false;

		// Aux vars
		LookAt t = target as LookAt;
		Vector3 lookAtOffset = Vector3.zero;
		Vector3 posOffset = Vector3.zero;
		Handles.color = Color.cyan;

		// If using the move tool, clear it since we want to use our own position handlers
		if(Tools.current == Tool.Move) {
			m_restoreMoveTool = true;
			Tools.current = Tool.View;
		}

		// Draw and get positioning handles
		if(Tools.pivotRotation == PivotRotation.Global) {
			posOffset = Handles.PositionHandle(t.transform.position, Quaternion.identity) - t.transform.position;
			lookAtOffset = Handles.PositionHandle(t.lookAtPointGlobal, Quaternion.identity) - t.lookAtPointGlobal;
		} else {
			posOffset = Handles.PositionHandle(t.transform.position, t.transform.rotation) - t.transform.position;
			lookAtOffset = Handles.PositionHandle(t.lookAtPointGlobal, t.transform.rotation) - t.lookAtPointGlobal;	// [AOC] Use object's rotation
		}

		// Draw line from the object (camera) to the lookAt for clarity
		m_points[0] = t.transform.position;
		m_points[1] = t.lookAtPointGlobal;
		Handles.DrawAAPolyLine(4f, m_points);
		Handles.ConeCap(0, t.lookAtPointGlobal, t.transform.rotation, HandleUtility.GetHandleSize(t.lookAtPointGlobal) * 0.15f);

		// If something changed, update all targets
		if(GUI.changed) {
			// Handle multi-selection
			for(int i = 0; i < m_targets.Length; i++) {
				// Aux vars
				t = m_targets[i];
				Undo.RecordObject(t, "LookAtPoint pos changed by handle");

				// Apply new offsets
				t.transform.position += posOffset;
				t.lookAtPointGlobal += lookAtOffset;

				// Based on edit mode, apply offset to the position as well
				switch(t.editMode) {
					case LookAt.EditMode.LINKED: {
						t.transform.position += lookAtOffset;
						t.lookAtPointGlobal += posOffset;
					} break;

					case LookAt.EditMode.LOOK_AT_FOLLOWS_POSITION: {
						t.lookAtPointGlobal += posOffset;
					} break;

					case LookAt.EditMode.POSITION_FOLLOWS_LOOK_AT: {
						t.transform.position += lookAtOffset;
					} break;
				}
			}

			// Refresh scene
			SceneView.RepaintAll();
			m_target.Update();
		}
	}
}