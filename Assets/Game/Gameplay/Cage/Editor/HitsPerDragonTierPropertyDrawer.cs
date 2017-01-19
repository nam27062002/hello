// MonoBehaviourTemplateEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on DD/MM/2017.
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
/// Custom editor for the MonoBehaviourTemplate class.
/// </summary>
[CustomPropertyDrawer(typeof(HitsPerDragonTier))]
public class HitsPerDragonTierPropertyDrawer : ExtendedPropertyDrawer {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public HitsPerDragonTierPropertyDrawer() {
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
		// Draw property label + foldout widget
		m_pos.height = EditorStyles.largeLabel.lineHeight;	// Advance pointer just the size of the label
		_property.isExpanded = EditorGUI.Foldout(m_pos, _property.isExpanded, _label);
		AdvancePos();

		SerializedProperty keyProperty = _property.FindPropertyRelative("m_keyList");
		SerializedProperty valueProperty = _property.FindPropertyRelative("m_valueList");

		// check size
		if (keyProperty.arraySize != (int)DragonTier.COUNT) {
			keyProperty.arraySize = (int)DragonTier.COUNT;
			for (int i = 0; i < keyProperty.arraySize; i++) {
				keyProperty.GetArrayElementAtIndex(i).enumValueIndex = i;
			}
		}

		if (valueProperty.arraySize != (int)DragonTier.COUNT) {
			valueProperty.arraySize = (int)DragonTier.COUNT;
		}

		EditorGUI.indentLevel++;
		for (int i = 0; i < keyProperty.arraySize; i++) {
			SerializedProperty key = keyProperty.GetArrayElementAtIndex(i);
			SerializedProperty value = valueProperty.GetArrayElementAtIndex(i);

			m_pos.height = EditorGUI.GetPropertyHeight(value);
			EditorGUI.PropertyField(m_pos, value, new GUIContent(key.enumDisplayNames[key.enumValueIndex]), true);
			AdvancePos();

			/*key.isExpanded = EditorGUI.Foldout(m_pos, key.isExpanded, "bla " + i);
			AdvancePos();
			if (key.isExpanded) {
				EditorGUI.indentLevel++;
				DrawAndAdvance(value);
				EditorGUI.indentLevel--;
			}*/
		}
		EditorGUI.indentLevel--;
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