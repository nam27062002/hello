// EntitySkuListAttributeEditor.cs
// 
// Created by Alger Ortín Castellví on 14/12/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Drawer for the EntitySkuList custom attribute.
/// Simpler version of the SkuListAttributeEditor since we know exactly the type of definition we're editing so we don't 
/// have to deal with generics and reflection black magic.
/// </summary>
[CustomPropertyDrawer(typeof(EntitySkuListAttribute))]
public class EntitySkuListAttributeEditor : ExtendedPropertyDrawer {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Data that mus be kept between opening the sku list popup and capturing its result
	private List<string> m_skus = null;
	private SerializedProperty m_targetProperty = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// A change has occurred in the inspector, draw the property and store new values.
	/// Use the m_pos member as position reference and update it using AdvancePos() method.
	/// </summary>
	/// <param name="_property">The property we're drawing.</param>
	/// <param name="_label">The label of the property.</param>
	override protected void OnGUIImpl(SerializedProperty _property, GUIContent _label) {
		// Check field type
		if(_property.propertyType != SerializedPropertyType.String) {
			m_pos.height = EditorStyles.largeLabel.lineHeight;
			EditorGUI.LabelField(m_pos, _label.text, "ERROR! EntitySkuList attribute can only be applied to string properties!");
			AdvancePos();
			return;
		}

		// Obtain the attribute
		EntitySkuListAttribute attr = attribute as EntitySkuListAttribute;

		// If definitions are not loaded, do it now
		if(!ContentManager.ready) ContentManager.InitContent(true);

		// Get the definitions and sort them by category
		List<string> categorySkus = DefinitionsManager.SharedInstance.GetSkuList(DefinitionsCategory.ENTITY_CATEGORIES);
		Dictionary<string, List<DefinitionNode>> defsByCategory = new Dictionary<string, List<DefinitionNode>>();
		for(int i = 0; i < categorySkus.Count; i++) {
			defsByCategory.Add(categorySkus[i], DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.ENTITIES, "category", categorySkus[i]));
		}

		// Create the skus list
		m_skus = new List<string>();

		// Insert "NONE" option at the beginning
		if(attr.m_allowNullValue) {
			m_skus.Add("---- NONE");
		}

		// Iterate defs and add them to the skus list
		foreach(KeyValuePair<string, List<DefinitionNode>> kvp in defsByCategory) {
			if (kvp.Value.Count > 0) {
				// Add a separator for each category
				m_skus.Add(SelectionPopupWindow.SECTION + kvp.Key);

				// Add each individual entity sku
				for(int i = 0; i < kvp.Value.Count; i++) {
					m_skus.Add(kvp.Value[i].sku);
				}
			}
		}

		// Find out current selected value
		// If current value was not found, force it to first value if "NONE" allowed or first non-category value if not allowed
		int selectedIdx = m_skus.FindIndex(_sku => _property.stringValue.Equals(_sku, StringComparison.Ordinal));
		if(selectedIdx < 0) {
			if(attr.m_allowNullValue) {
				// First option should be "NONE", we're good
				selectedIdx = 0;
			} else {
				// Find out first non-section option
				for(int i = 0; i < m_skus.Count; i++) {
					if(!m_skus[i].Contains(SelectionPopupWindow.SECTION)) {
						selectedIdx = i;
						break;
					}
				}
				OnSkuSelected(selectedIdx);
			}
		}
		
		// Display the property
		// Show button using the popup style with the current value
		m_pos.height = EditorStyles.popup.lineHeight + 5;	// [AOC] Default popup field height + some margin
		Rect contentPos = EditorGUI.PrefixLabel(m_pos, _label);
		if(GUI.Button(contentPos, m_skus[selectedIdx], EditorStyles.popup)) {
			m_targetProperty = _property;
			SelectionPopupWindow.Show(m_skus.ToArray(), OnSkuSelected);
		}

		// Leave room for next property drawer
		AdvancePos();
	}

	/// <summary>
	/// A sku has been selected in the selection popup.
	/// </summary>
	/// <param name="_selectedIdx">The index of the newly selected option.</param>
	private void OnSkuSelected(int _selectedIdx) {
		// Store new value - "NONE" will be stored as "" when allowed
		EntitySkuListAttribute attr = attribute as EntitySkuListAttribute;
		if(attr.m_allowNullValue && _selectedIdx == 0) {
			m_targetProperty.stringValue = "";
		} else {
			m_targetProperty.stringValue = m_skus[_selectedIdx];	// Options array match the definition skus, so no need to do anything else :)
		}
		m_targetProperty.serializedObject.ApplyModifiedProperties();
	}
}

