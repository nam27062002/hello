// EmojiManagerEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 24/04/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom editor for the EmojiManager class.
/// </summary>
[CustomEditor(typeof(EmojiManager), true)] // True to be used by heir classes as well
public class EmojiManagerEditor : Editor {
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// GUI widgets
	private ReorderableList m_list = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Initialize elements list
		// Using the obscure UnityEditorInternal ReorderableList!
		m_list = new ReorderableList(serializedObject, serializedObject.FindProperty("m_emojiDatabase"));

		// Header
		m_list.drawHeaderCallback = (Rect _rect) => {
			// Show columns info
			Rect pos = _rect;

			// Target
			pos.x += 25f;   // Move handle size (approx)
			pos.width -= 25f;
			EditorGUI.LabelField(pos, "Emojis List");
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

			// Different colors based on state
			if(_active) {
				GUI.color = new Color(0.24f, 0.38f, 0.54f);     // Unity default (blue-ish)
			} else if(_focused) {
				GUI.color = new Color(0.36f, 0.36f, 0.36f);     // Unity default (brighter gray)
			} else {
				GUI.color = new Color(0.30f, 0.30f, 0.30f);     // Unity default (dark gray)
			}
			GUI.DrawTexture(_rect, Texture2D.whiteTexture);
			GUI.color = Color.white;
		};
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {

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

		// Entries list
		m_list.DoLayoutList();

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

// ===========================================================================//
// ===========================================================================//
// ===========================================================================//

/// <summary>
/// Custom editor for the TweenSequenceElement class.
/// </summary>
[CustomPropertyDrawer(typeof(EmojiManager.EmojiData))]
public class EmojiDataPropertyDrawer : ExtendedPropertyDrawer {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public EmojiDataPropertyDrawer() {
		// Nothing to do
	}

	//------------------------------------------------------------------//
	// PARENT IMPLEMENTATION											//
	//------------------------------------------------------------------//
	/// <summary>
	/// A change has occurred in the inspector, draw the property and store new values.
	/// Use the m_pos member as position reference and update it using AdvancePos() method.
	/// </summary>
	/// <param name="_property">The property we're drawing.</param>
	/// <param name="_label">The label of the property.</param>
	override protected void OnGUIImpl(SerializedProperty _property, GUIContent _label) {
		// Define some measures
		m_pos.height = EditorGUIUtility.singleLineHeight;
		float previewWidth = m_pos.width * 0f; // * 0.2f;	// Not supported by Unity :(
		float keyWidth = (m_pos.width - previewWidth) * 0.5f;
		float unicodeWidth = (m_pos.width - previewWidth) * 0.5f;

		// Aux vars
		Rect pos = m_pos;
		SerializedProperty p = null;

		// Reset indentation and label size (no indentation within the widget, m_pos already contains global indentation)
		EditorGUIUtility.labelWidth = 80f;
		int indentLevelBackup = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;

		// Key
		pos.width = keyWidth;
		p = _property.FindPropertyRelative("key");
		EditorGUI.PropertyField(pos, p, GUIContent.none, true);

		// Unicode
		pos.x += pos.width;
		pos.width = unicodeWidth;
		p = _property.FindPropertyRelative("unicodeHexCode");
		EditorGUI.PropertyField(pos, p, GUIContent.none, true);

		// Preview
		// [AOC] Unfortunately, Unity Editor GUI doesn't support emoji display for now :( :(
		/*
		pos.x += pos.width;
		pos.width = previewWidth;
		p = _property.FindPropertyRelative("unicodeHexCode");
		EditorGUI.SelectableLabel(pos, EmojiManager.UnicodeToString(p.stringValue));
		*/

		// Next line
		AdvancePos();

		// Restore indentation and label size
		EditorGUIUtility.labelWidth = 0f;
		EditorGUI.indentLevel = indentLevelBackup;
	}

	/// <summary>
	/// Optionally override to give a custom label for this property field.
	/// </summary>
	/// <returns>The new label for this property.</returns>
	/// <param name="_property">The property we're drawing.</param>
	/// <param name="_defaultLabel">The default label for this property.</param>
	override protected GUIContent GetLabel(SerializedProperty _property, GUIContent _defaultLabel) {
		return _defaultLabel;
	}
}