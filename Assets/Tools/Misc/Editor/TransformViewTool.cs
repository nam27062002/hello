// MonoBehaviourTemplateEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 20/10/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple editor tool to display all the transform values on the selected object.
/// </summary>
public class TransformViewTool : EditorWindow {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Shows the window.
	/// </summary>
	public static void ShowWindow() {
		TransformViewTool window = (TransformViewTool)EditorWindow.GetWindow(typeof(TransformViewTool));
		window.minSize = new Vector2(100f, 160f);
	}

	/// <summary>
	/// Update the inspector window.
	/// </summary>
	private void OnGUI() {
		// We need a selection
		Transform t = Selection.activeTransform;
		if(t == null) {
			EditorGUILayout.HelpBox("No object selected", MessageType.Error);
		} else {
			// We have issues with precision when assigning local and global stuff at the same time, so disable edition for now
			EditorGUI.BeginChangeCheck();
			GUI.enabled = false;

			// Position
			EditorGUILayoutExt.Vector3Field("Local Position", t.localPosition);
			EditorGUILayoutExt.Vector3Field("Global Position", t.position);

			// Rotation
			EditorGUILayout.Space();
			EditorGUILayoutExt.Vector3Field("Local Euler", t.localEulerAngles);
			EditorGUILayoutExt.Vector3Field("Global Euler", t.eulerAngles);
			EditorGUILayoutExt.QuaternionField("Local Rotation", t.localRotation);
			EditorGUILayoutExt.QuaternionField("Global Rotation", t.rotation);

			// Scale
			EditorGUILayout.Space();
			EditorGUILayoutExt.Vector3Field("Local Scale", t.localScale);
			EditorGUILayoutExt.Vector3Field("Lossy Scale", t.lossyScale);

			// Only apply changes to selection if we actually detect a change
			GUI.enabled = true;
			if(EditorGUI.EndChangeCheck()) {
				Selection.activeTransform.CopyFrom(t);
			}
		}
	}

	/// <summary>
	/// The current selection has changed.
	/// </summary>
	private void OnInspectorUpdate() {
		// Repaint the window
		Repaint();
	}
}