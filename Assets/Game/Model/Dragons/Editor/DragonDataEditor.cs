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

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom editor for the DragonData class.
/// </summary>
[CustomPropertyDrawer(typeof(DragonData))]
public class DragonDataEditor : ExtendedPropertyDrawer {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//

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
		while(_property.depth == targetDepth) {		// Only direct children, not brothers or grand-children (the latter will be drawn by default if using the default EditorGUI.PropertyField)
			// Draw property as default except for the ones we want to customize
			// ID and start of the General Data section
			if(_property.name == "m_id") {
				// Draw a separator
				propertyHeight = EditorGUILayoutExt.Separator(m_pos, new SeparatorAttribute("General Data", 30f));
				AdvancePos(propertyHeight);

				// ID can't be changed, so don't do anything else
			}

			// Start of the Progression section
			else if(_property.name == "m_progression") {
				// Draw a separator first
				propertyHeight = EditorGUILayoutExt.Separator(m_pos, new SeparatorAttribute("Progression", 30f));
				AdvancePos(propertyHeight);

				// Default property drawing
				m_pos.height = EditorGUI.GetPropertyHeight(_property);
				EditorGUI.PropertyField(m_pos, _property);
				AdvancePos();
			}

			// Start of the Level-dependant Stats section
			else if(_property.name == "m_healthRange") {
				// Draw a separator first
				propertyHeight = EditorGUILayoutExt.Separator(m_pos, new SeparatorAttribute("Level-dependant Stats", 30f));
				AdvancePos(propertyHeight);
				
				// Default property drawing
				m_pos.height = EditorGUI.GetPropertyHeight(_property);
				EditorGUI.PropertyField(m_pos, _property);
				AdvancePos();
			}

			// Start of the Constant Stats section
			else if(_property.name == "m_healthDrainPerSecond") {
				// Draw a separator first
				propertyHeight = EditorGUILayoutExt.Separator(m_pos, new SeparatorAttribute("Constant Stats", 30f));
				AdvancePos(propertyHeight);
				
				// Default property drawing
				m_pos.height = EditorGUI.GetPropertyHeight(_property);
				EditorGUI.PropertyField(m_pos, _property);
				AdvancePos();
			}

			// Skills
			else if(_property.name == "m_skills") {
				// Draw a separator first
				propertyHeight = EditorGUILayoutExt.Separator(m_pos, new SeparatorAttribute("Skills", 30f));
				AdvancePos(propertyHeight);

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
			}

			// Default
			else {
				m_pos.height = EditorGUI.GetPropertyHeight(_property);
				EditorGUI.PropertyField(m_pos, _property, true);
				AdvancePos();
			}

			// Move to next property
			_property.NextVisible(false);
		}
	}
}