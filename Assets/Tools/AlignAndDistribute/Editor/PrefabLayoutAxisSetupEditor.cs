// PrefabLayoutAxisSetupEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 19/08/2016.
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
/// Custom editor for the MonoBehaviourTemplate class.
/// </summary>
[CustomPropertyDrawer(typeof(PrefabLayout.AxisSetup))]
public class PrefabLayoutAxisSetupEditor : ExtendedPropertyDrawer {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	private static GUIStyle s_titleBackgroundStyle = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public PrefabLayoutAxisSetupEditor() {
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
		// If title style was not created, do it now
		if(s_titleBackgroundStyle == null) {
			s_titleBackgroundStyle = new GUIStyle();
			s_titleBackgroundStyle.normal.background = Texture2DExt.Create(2, 2, new Color(0.65f, 0.65f, 0.65f));
			s_titleBackgroundStyle.margin = EditorStyles.helpBox.margin;
			s_titleBackgroundStyle.padding = EditorStyles.helpBox.padding;
		}

		// Label background
		m_pos.height = EditorStyles.label.lineHeight + EditorStyles.label.margin.vertical + 6f;
		Rect pos = m_pos;
		pos.height = m_pos.height - 6f;
		pos.y += m_pos.height/2f - pos.height/2f;
		GUI.Box(pos, "", s_titleBackgroundStyle);

		// Draw label
		pos = m_pos;	// Reset pos
		pos.x += 4f;
		pos.height -= 6f;
		pos.y += m_pos.height/2f - pos.height/2f;
		EditorGUI.LabelField(pos, _label.text);

		// Move on
		AdvancePos();

		// Draw all children properties
		// Aux vars
		int baseDepth = _property.depth;
		int rootIndent = EditorGUI.indentLevel;

		// Iterate through all the children of the property
		bool loop = _property.Next(true);	// Enter to the first level of depth
		while(loop) {
			// Set indentation
			EditorGUI.indentLevel = rootIndent + _property.depth;

			// Properties requiring special treatment
			if(_property.name == "numSections") {
				// Aux positioning vars
				m_pos.height = GUI.skin.horizontalSlider.lineHeight + GUI.skin.horizontalSlider.margin.vertical;
				pos = m_pos;

				// Label
				pos = EditorGUI.PrefixLabel(pos, new GUIContent("Num Sections"));
				int indentLevelBackup = EditorGUI.indentLevel;
				EditorGUI.indentLevel = 0;

				// Auto Toggle
				//SerializedProperty autoProp = _property.serializedObject.FindProperty("autoSections");
				SerializedProperty autoProp = m_rootProperty.FindPropertyRelative("autoSections");
				float contentTotalWidth = pos.width;
				pos.width = 50f;
				autoProp.boolValue = EditorGUI.ToggleLeft(pos, "Auto", autoProp.boolValue);

				// Sections number slider
				pos.x += pos.width;
				pos.width = contentTotalWidth - pos.width;
				GUI.enabled = !autoProp.boolValue;
				_property.intValue = EditorGUI.IntSlider(pos, _property.intValue, 1, 100);
				GUI.enabled = true;

				// Go on
				EditorGUI.indentLevel = indentLevelBackup;
				AdvancePos();
			}

			// To skip
			else if(_property.name == "autoSections") {
				// Skip
			}

			// Default property display
			else {
				DrawAndAdvance(_property);
			}

			// Move to next property
			// Only direct children, not grand-children (will be drawn by default if using the default EditorGUI.PropertyField)
			loop = _property.Next(false);

			// If within an array, Next() will give the next element of the array, which will be already be drawn by itself afterwards, so we don't want it - check depth to prevent it
			if(_property.depth <= baseDepth) loop = false;
		}
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