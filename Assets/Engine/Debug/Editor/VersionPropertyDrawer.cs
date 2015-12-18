// VersionPropertyDrawer.cs
// 
// Created by Alger Ortín Castellví on 29/10/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom property drawer for the Version class.
/// </summary>
[CustomPropertyDrawer(typeof(Version))]
public class VersionPropertyDrawer : ExtendedPropertyDrawer {
	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// A change has occurred in the inspector, draw the property and store new values.
	/// Use the m_pos member as position reference and update it using AdvancePos() method.
	/// </summary>
	/// <param name="_property">The property we're drawing.</param>
	/// <param name="_label">The label of the property.</param>
	protected override void OnGUIImpl(SerializedProperty _property, GUIContent _label) {
		// Label
		m_pos = EditorGUI.PrefixLabel(m_pos, _label);

		// Aux vars
		// Draw all three version fields in an horizontal layout
		SerializedProperty p;
		float dotWidth = EditorStyles.label.CalcSize(new GUIContent(".")).x;
		float fieldWidth = (m_pos.width - 2 * dotWidth)/3;	// 3 columns (major/minor/patch), 2 dots in between
		m_pos.height = EditorStyles.numberField.lineHeight + EditorStyles.numberField.margin.vertical;

		// Major
		m_pos.width = fieldWidth;
		p = _property.FindPropertyRelative("m_major");
		p.intValue = EditorGUI.IntField(m_pos, p.intValue);
		m_pos.x += m_pos.width;

		// Dot
		m_pos.width = dotWidth;
		EditorGUI.LabelField(m_pos, ".");
		m_pos.x += m_pos.width;

		// Minor
		m_pos.width = fieldWidth;
		p = _property.FindPropertyRelative("m_minor");
		p.intValue = EditorGUI.IntField(m_pos, p.intValue);
		m_pos.x += m_pos.width;

		// Dot
		m_pos.width = dotWidth;
		EditorGUI.LabelField(m_pos, ".");
		m_pos.x += m_pos.width;

		// Patch
		m_pos.width = fieldWidth;
		p = _property.FindPropertyRelative("m_patch");
		p.intValue = EditorGUI.IntField(m_pos, p.intValue);
		m_pos.x += m_pos.width;

		AdvancePos();
	}
}