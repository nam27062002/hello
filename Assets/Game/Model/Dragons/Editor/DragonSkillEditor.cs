// DragonSkillEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 17/09/2015.
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
/// Custom editor for the DragonSkill class.
/// Each skill must be foldable.
/// Skill type can't be change and will be used as label.
/// Unlock prices is an array of fixed length (6) with custom labels for each level, but we want to allow folding it.
/// </summary>
[CustomPropertyDrawer(typeof(DragonSkill))]
public class DragonSkillEditor : ExtendedPropertyDrawer {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Default constructor
	/// </summary>
	public DragonSkillEditor() {
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

		// If unfolded, draw children
		if(_property.isExpanded) {
			// Indent in
			EditorGUI.indentLevel++;
			
			// Iterate through all the children of the property
			_property.NextVisible(true);	// Enter to the first level of depth
			int targetDepth = _property.depth;
			while(_property.depth == targetDepth) {		// Only direct children, not brothers or grand-children (the latter will be drawn by default if using the default EditorGUI.PropertyField)
				// Draw property as default except for the ones we want to customize
				// Type: don't display it - not allowed to be changed
				if(_property.name == "m_type") {
					// Just do nothing
				}

				// Unlock prices: fixed length (6) with custom labels for each level, allow folding it
				else if(_property.name == "m_unlockPrices") {
					// Draw property label + foldout widget
					m_pos.height = EditorStyles.largeLabel.lineHeight;	// Advance pointer just the size of the label
					_property.isExpanded = EditorGUI.Foldout(m_pos, _property.isExpanded, _property.displayName);
					AdvancePos();

					// If unfolded, draw array entries
					if(_property.isExpanded) {
						// Indentation in
						EditorGUI.indentLevel++;

						// Aux vars
						SerializedProperty priceProp = null;
						GUIContent label = new GUIContent();

						// Little extra: we will show a warning if the price is lower than any of the previous levels
						GUIStyle defaultStyle = new GUIStyle(EditorStyles.numberField);
						int maxPrice = -1;
						bool errorDetected = false;

						// Iterate through all prices list
						// Actually, first level is always unlocked, so skip it
						for(int i = 1; i < _property.arraySize; i++) {
							// Get price property for this level
							priceProp = _property.GetArrayElementAtIndex(i);

							// Change color if price is lower than the previous level
							// To do this, we must overwrite default editor style
							// Once an error has been detected, print the rest of levels as error as well 
							if(!errorDetected) {
								if(maxPrice > priceProp.intValue || priceProp.intValue <= 0) {
									EditorStyles.numberField.normal.textColor = new Color(1f, 0.5f, 0f);
									EditorStyles.numberField.active.textColor = new Color(1f, 0.5f, 0f);
									EditorStyles.numberField.focused.textColor = new Color(1f, 0.5f, 0f);
								}
								maxPrice = Mathf.Max(maxPrice, priceProp.intValue);
							}
							
							// Draw it using our custom label
							label.text = "Level " + (i);
							m_pos.height = EditorGUI.GetPropertyHeight(priceProp);
							EditorGUI.PropertyField(m_pos, priceProp, label);
							AdvancePos();
						}

						// Restore default editor style
						EditorStyles.numberField.normal.textColor = defaultStyle.normal.textColor;
						EditorStyles.numberField.active.textColor = defaultStyle.active.textColor;
						EditorStyles.numberField.focused.textColor = defaultStyle.active.textColor;
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

	/// <summary>
	/// Optionally override to give a custom label for this property field.
	/// </summary>
	/// <returns>The new label for this property.</returns>
	/// <param name="_property">The property we're drawing.</param>
	/// <param name="_defaultLabel">The default label for this property.</param>
	override protected GUIContent GetLabel(SerializedProperty _property, GUIContent _defaultLabel) {
		// Use the skill type as label
		SerializedProperty typeProp = _property.FindPropertyRelative("m_type");
		string typeName = typeProp.enumDisplayNames[typeProp.enumValueIndex];
		return new GUIContent(typeName);
	}
}