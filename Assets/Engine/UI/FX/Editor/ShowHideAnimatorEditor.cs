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
	// Info field
	private static readonly string INFO = 
		"- Tween type determines the \"show\" direction. \"hide\" will be the reversed tween.\n" +
		"- Use NONE to just activate/deactivate the gameObject.\n" +
		"- Use IDLE to delay the instant show/hide of the object (for example when waiting for other animations to finish).\n" +
		"- To use CUSTOM, add as many DOTweenAnimation components as desired and link them to the corresponding arrays.\n" +
		"- To use ANIMATOR, an animator with the following triggers must be assigned: \"show\", \"hide\", \"instantShow\" and \"instantHide\".\n" +
		"- Use the \"value\" parameter to tune the animation (e.g. offset for move tweens, scale factor for the scale tweens, initial alpha for fade tweens).\n" +
		"- Talk to the programming team if you wish to add a different tween type or extra parameters.\n";

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Casted target
	private ShowHideAnimator m_targetAnimator = null;

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetAnimator = target as ShowHideAnimator;
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_targetAnimator = null;
	}

	/// <summary>
	/// Draw the inspector.
	/// </summary>
	public override void OnInspectorGUI() {
		// Update the serialized object - always do this in the beginning of OnInspectorGUI.
		serializedObject.Update();

		// Animation type selector
		// Add extra comments in some special types
		SerializedProperty p = DoProperty("m_tweenType");
		switch((ShowHideAnimator.TweenType)p.enumValueIndex) {
			case ShowHideAnimator.TweenType.NONE: {
				EditorGUILayout.LabelField("Use NONE to just activate/deactivate the gameObject", CustomEditorStyles.commentLabelLeft);
			} break;

			case ShowHideAnimator.TweenType.CUSTOM: {
				EditorGUILayout.LabelField("To use CUSTOM, add as many DOTweenAnimation components as desired and link them to the corresponding arrays.", CustomEditorStyles.commentLabelLeft);
			} break;

			case ShowHideAnimator.TweenType.ANIMATOR: {
				EditorGUILayout.LabelField("To use ANIMATOR, an animator with the following triggers must be assigned: \"show\", \"hide\", \"instantShow\" and \"instantHide\"", CustomEditorStyles.commentLabelLeft);
			} break;

			case ShowHideAnimator.TweenType.IDLE: {
				EditorGUILayout.LabelField("Use IDLE to delay the instant show/hide of the object (for example when waiting for other animations to finish).", CustomEditorStyles.commentLabelLeft);
			} break;
		}

		// Depending on select animation mode, show different setup parameters
		EditorGUILayout.Space();
		switch(m_targetAnimator.tweenType) {
			case ShowHideAnimator.TweenType.ANIMATOR: {
				DoProperty("m_animator");
				DoProperty("m_tweenDelay");
				DoProperty("m_ignoreTimeScale");
			} break;

			case ShowHideAnimator.TweenType.CUSTOM: {
				DoProperty("m_showTweens");
				DoProperty("m_hideTweens");
			} break;

			case ShowHideAnimator.TweenType.NONE: {

			} break;

			case ShowHideAnimator.TweenType.IDLE: {
				DoProperty("m_tweenDuration");
				DoProperty("m_tweenDelay");
				DoProperty("m_ignoreTimeScale");
			} break;

			default: {
				DoProperty("m_tweenDuration");
				DoProperty("m_tweenValue");
				DoProperty("m_tweenEase");
				DoProperty("m_tweenDelay");
				DoProperty("m_ignoreTimeScale");
			} break;
		}

		// Events
		// Use the expanded attribute of the "OnShowPreAnimation" property so setting is stored per object
		EditorGUILayout.Space();
		SerializedProperty onShowPreAnimationProp = serializedObject.FindProperty("OnShowPreAnimation");
		onShowPreAnimationProp.isExpanded = EditorGUILayout.Foldout(onShowPreAnimationProp.isExpanded, "Events");
		if(onShowPreAnimationProp.isExpanded) {
			EditorGUI.indentLevel++;
			DoProperty("OnShowPreAnimation");
			DoProperty("OnShowPreAnimationAfterDelay");
			DoProperty("OnShowPostAnimation");
			DoProperty("OnHidePreAnimation");
			DoProperty("OnHidePostAnimation");
			EditorGUI.indentLevel--;
		}
		
		// Foldable info
		EditorGUILayout.Space();
		bool infoExpanded = EditorPrefs.GetBool("ShowHideAnimatorEditor.InfoExpanded", false);
		if(GUILayout.Button(infoExpanded ? "HIDE" : "HELP", GUILayout.Width(50f))) {
			infoExpanded = !infoExpanded;
			EditorPrefs.SetBool("ShowHideAnimatorEditor.InfoExpanded", infoExpanded);
		}
		if(infoExpanded) {
			EditorGUILayout.HelpBox(INFO, MessageType.Info);
		}

		// Test buttons
		EditorGUILayout.Space();
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

	//------------------------------------------------------------------//
	// INTERNAL METHODS													//
	//------------------------------------------------------------------//
	private SerializedProperty DoProperty(string _id) {
		SerializedProperty p = serializedObject.FindProperty(_id);
		if(p != null) EditorGUILayout.PropertyField(p, true);
		return p;
	}
}