// ShowHideAnimatorEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 26/02/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom editor for the ShowHideAnimator class.
/// </summary>
[CustomEditor(typeof(ShowHideAnimator), true)]
[CanEditMultipleObjects]
public class ShowHideAnimatorEditor : Editor {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	private static readonly string INFO = 
		"- Tween type determines the \"show\" direction. \"hide\" will be the reversed tween.\n" +
		"- To use CUSTOM, add as many DOTweenAnimation components as desired to the target object with the id's \"show\" and \"hide\".\n" +
		"- Use IDLE to delay the instant show/hide of the object (for example when waiting for other animations to finish).\n" +
		"- Use the \"value\" parameter to tune the animation (e.g. offset for move tweens, scale factor for the scale tweens, initial alpha for fade tweens).\n" +
		"- All tween-related parameters will be ignored if an animator is defined.\n" +
		"- Talk to the programming team if you wish to add a different tween type or extra parameters.\n";

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Extra info field
	private static bool s_infoExpanded = false;

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Draw the inspector.
	/// </summary>
	public override void OnInspectorGUI() {
		// Default inspector
		DrawDefaultInspector();

		// Update the serialized object - always do this in the beginning of OnInspectorGUI.
		serializedObject.Update();

		// Foldable info
		if(GUILayout.Button(s_infoExpanded ? "HIDE" : "HELP", GUILayout.Width(50f))) {
			s_infoExpanded = !s_infoExpanded;
		}
		//s_infoExpanded = EditorGUILayout.Foldout(s_infoExpanded, "MORE INFO");
		if(s_infoExpanded) {
			EditorGUILayout.HelpBox(INFO, MessageType.Info);
		}

		// Test buttons
		EditorGUILayoutExt.Separator(new SeparatorAttribute("Debug Tools (only during play mode)", 15f));
		bool wasEnabled = GUI.enabled;
		GUI.enabled = Application.isPlaying;
		EditorGUILayout.BeginHorizontal(); {
			// Current state
			if((target as ShowHideAnimator).visible) {
				GUILayout.Label("STATE VISIBLE");
			} else {
				GUILayout.Label("STATE HIDDEN");
			}

			// Show
			if(GUILayout.Button("Show")) {
				for(int i = 0; i < targets.Length; i++) {
					(targets[i] as ShowHideAnimator).Show();
				}
			}

			// Hide
			if(GUILayout.Button("Hide")) {
				for(int i = 0; i < targets.Length; i++) {
					(targets[i] as ShowHideAnimator).Hide();
				}
			}

			// Toggle
			if(GUILayout.Button("Toggle")) {
				for(int i = 0; i < targets.Length; i++) {
					(targets[i] as ShowHideAnimator).Toggle();
				}
			}
		} EditorGUILayoutExt.EndHorizontalSafe();
		GUI.enabled = wasEnabled;

		EditorGUILayout.Space();

		// Apply changes to the serialized object - always do this in the end of OnInspectorGUI.
		serializedObject.ApplyModifiedProperties();
	}

	/// <summary>
	/// The scene is being refreshed.
	/// </summary>
	public void OnSceneGUI() {
		// Scene-related stuff
	}
}