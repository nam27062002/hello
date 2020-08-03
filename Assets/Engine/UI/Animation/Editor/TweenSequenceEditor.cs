// TweenSequenceEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 06/09/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom editor for the TweenSequence class.
/// </summary>
[CustomEditor(typeof(TweenSequence), true)]	// True to be used by heir classes as well
public class TweenSequenceEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	TweenSequence m_targetTweenSequence = null;

	// GUI widgets
	private ReorderableList m_list = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetTweenSequence = target as TweenSequence;

		// Initialize elements list
		// Using the obscure UnityEditorInternal ReorderableList!
		m_list = new ReorderableList(serializedObject, serializedObject.FindProperty("m_elements"));

		// Header
		m_list.drawHeaderCallback = (Rect _rect) => {
			// Show columns info
			Rect pos = _rect;

			// Target
			pos.x += 25f;	// Move handle size (approx)
			pos.x += TweenSequenceElementPropertyDrawer.FOLDOUT_WIDTH;
			pos.width = TweenSequenceElementPropertyDrawer.TARGET_WIDTH;
			EditorGUI.LabelField(pos, "Target");

			// Type
			pos.x += pos.width;
			pos.width = TweenSequenceElementPropertyDrawer.TYPE_WIDTH;
			pos.width += TweenSequenceElementPropertyDrawer.SPACE_WIDTH;
			EditorGUI.LabelField(pos, "Type");

			// Start Time
			pos.x += pos.width;
			pos.width = TweenSequenceElementPropertyDrawer.TIME_TEXT_WIDTH;
			pos.width += 1f;	// Small space between start time and duration
			EditorGUI.LabelField(pos, "Start");

			// Duration
			pos.x += pos.width;
			pos.width = TweenSequenceElementPropertyDrawer.TIME_TEXT_WIDTH;
			pos.width += TweenSequenceElementPropertyDrawer.SPACE_WIDTH;
			EditorGUI.LabelField(pos, "Dur");

			// End Time - put at the end
			pos.x = _rect.x + _rect.width;
			pos.width = TweenSequenceElementPropertyDrawer.TIME_TEXT_WIDTH;
			pos.x -= pos.width;
			EditorGUI.LabelField(pos, "End");
		};

		// Element
		m_list.drawElementCallback = (Rect _rect, int _idx, bool _isActive, bool _isFocused) => {
			// Element's foldout arrow is drawn OUTSIDE the rectangle, so try to compensate it here.
			// Do some hardcoded adjusting before drawing the property
			_rect.x += 10f;
			_rect.width -= 10f;
			_rect.y += 2f;

			// Just do the property
			SerializedProperty p = m_list.serializedProperty.GetArrayElementAtIndex(_idx);
			EditorGUI.PropertyField(_rect, p, new GUIContent(p.displayName), true);
		};

		m_list.elementHeightCallback = (int _idx) => {
			// Using serialized properties without customization
			SerializedProperty p = m_list.serializedProperty.GetArrayElementAtIndex(_idx);
			return EditorGUI.GetPropertyHeight(p, new GUIContent(p.displayName), true);
		};

		m_list.drawElementBackgroundCallback = (Rect _rect, int _idx, bool _active, bool _focused) => {
			// Using customized serialized properties
			float margin = 2f;
			_rect.x += margin;
			_rect.width -= margin * 2f;

			// Create texture (different colors based on state)
			Texture2D tex = new Texture2D (1, 1);
			if(_active) {
				tex.SetPixel(0, 0, new Color(0.24f, 0.38f, 0.54f));		// Unity default (blue-ish)
			} else if(_focused) {
				tex.SetPixel(0, 0, new Color(0.36f, 0.36f, 0.36f));		// Unity default (brighter gray)
			} else {
				tex.SetPixel(0, 0, new Color(0.30f, 0.30f, 0.30f));		// Unity default (dark gray)
			}
			tex.Apply();
			GUI.DrawTexture(_rect, tex as Texture);
		};
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_targetTweenSequence = null;
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

		// Total duration
		// [AOC] Use a delayed float field to prevent total duration resetting to 0 when clearing the textfield (thus losing all tween element values)
		EditorGUILayout.DelayedFloatField(serializedObject.FindProperty("m_totalDuration"));

		// Elements list
		m_list.DoLayoutList();

		// Play mode tools
		// Progress slider
		// [AOC] Try to make its width match the sliders of the elements
		GUI.enabled = false;
		float progress = 0f;
		if(m_targetTweenSequence.sequence != null) {
			progress = m_targetTweenSequence.sequence.ElapsedPercentage();
		}
		EditorGUILayout.BeginHorizontal(); {
			GUILayout.Label("Sequence Progress", GUILayout.Width(220f), GUILayout.ExpandWidth(false));
			EditorGUILayout.Slider(GUIContent.none, progress, 0f, 1f);
		} EditorGUILayout.EndHorizontal();

		GUI.enabled = Application.isPlaying;
		EditorGUILayout.BeginHorizontal(GUILayout.Height(40f)); {
			// Launch button
			if(GUILayout.Button("Launch", GUILayout.ExpandHeight(true))) {
				m_targetTweenSequence.Launch();
			}

			// Recreate and launch button
			if(GUILayout.Button("Recreate and Launch", GUILayout.ExpandHeight(true))) {
				m_targetTweenSequence.Launch();
			}
		} EditorGUILayout.EndHorizontal();
		GUI.enabled = true;

		// Callback
		EditorGUILayoutExt.Separator();
		EditorGUILayout.PropertyField(serializedObject.FindProperty("OnFinished"));

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