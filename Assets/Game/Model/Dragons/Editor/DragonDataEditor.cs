 // DragonDataEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 14/09/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom editor for the DragonData class.
/// </summary>
//[CustomPropertyDrawer(typeof(DragonData))]
public class DragonDataEditor : ExtendedPropertyDrawer {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Configure separators
	// [AOC] Key is the name of the property before which the separator should be placed. Value is the text of the separator.
	private static readonly Dictionary<string, string> m_separators = new Dictionary<string, string> {
		{ "m_id", "General Data" },
		{ "m_skills", "Skills" },
		{ "m_unlockPriceCoins", "Progression" },
		{ "m_healthRange", "Level-dependant Stats" },
		{ "m_healthDrainPerSecond", "Constant Stats"} 
	};

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Default constructor
	/// </summary>
	public DragonDataEditor() {
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
		// Draw a big, nice title showing the dragon ID
		// Do it by overriding the default label style
		GUIStyle style = new GUIStyle(EditorStyles.helpBox);
		style.fontStyle = FontStyle.Bold;
		style.fontSize = 15;
		style.fixedHeight = style.lineHeight * 1.5f;
		style.alignment = TextAnchor.MiddleCenter;

		m_pos.height = style.fixedHeight;
		SerializedProperty idProp = _property.FindPropertyRelative("m_id");
		EditorGUI.LabelField(m_pos, idProp.enumDisplayNames[idProp.enumValueIndex], style);
		AdvancePos();

		// Iterate through all the children of the property
		_property.NextVisible(true);	// Enter to the first level of depth
		int targetDepth = _property.depth;
		float propertyHeight = 0;
		bool drawDefault = true;
		while(_property.depth == targetDepth) {		// Only direct children, not brothers or grand-children (the latter will be drawn by default if using the default EditorGUI.PropertyField)
			// Draw property as default except for the ones we want to customize
			drawDefault = true;

			// If the property is a section header, draw a separator first
			if(m_separators.ContainsKey(_property.name)) {
				// Draw a separator
				propertyHeight = EditorGUILayoutExt.Separator(m_pos, new SeparatorAttribute(m_separators[_property.name], 30f));
				AdvancePos(propertyHeight);
			}

			// Properties requiring special treatment
			// ID can't be changed, so don't do anything else
			if(_property.name == "m_id") {
				drawDefault = false;
			}

			// Skills
			else if(_property.name == "m_skills") {
				// Skills is an array of fixed length (4), but we will display each skill as an individual property - since we don't want to allow changing its size or order
				// Each skill must be foldable
				// Skill type can't be change and will be used as label
				// Unlock prices is another array of fixed length (6) with custom labels for each level, but in this case we want to allow folding it
				SerializedProperty skillProp = null;
				for(int i = 0; i < _property.arraySize; i++) {
					// Get skill property
					skillProp = _property.GetArrayElementAtIndex(i);
					
					// Draw it using its own custom property drawer
					m_pos.height = EditorGUI.GetPropertyHeight(skillProp);
					EditorGUI.PropertyField(m_pos, skillProp, true);
					AdvancePos();
				}

				drawDefault = false;
			}

			// Properties without any special treatment
			// Draw property using the default property drawer
			if(drawDefault) {
				m_pos.height = EditorGUI.GetPropertyHeight(_property);
				EditorGUI.PropertyField(m_pos, _property, true);
				AdvancePos();
			}

			// Move to next property
			_property.NextVisible(false);
		}
	}
}