﻿// EquipableSkuListAttributeEditor.cs
// 
// Copyright (c) 2019 Ubisoft. All rights reserved.

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
/// Drawer for the DecorationSkuList custom attribute.
/// Simpler version of the SkuListAttributeEditor since we know exactly the type of definition we're editing so we don't 
/// have to deal with generics and reflection black magic.
/// </summary>
[CustomPropertyDrawer(typeof(EquipableSkuListAttribute))]
public class EquipableSkuListAttributeEditor : ExtendedPropertyDrawer {
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
		m_targetProperty = _property;

		// Check field type
		if(_property.propertyType != SerializedPropertyType.String) {
			m_pos.height = EditorStyles.largeLabel.lineHeight;
			EditorGUI.LabelField(m_pos, _label.text, "ERROR! EntitySkuList attribute can only be applied to string properties!");
			AdvancePos();
			return;
		}

        // Obtain the attribute
        EquipableSkuListAttribute attr = attribute as EquipableSkuListAttribute;

		// If definitions are not loaded, do it now
		if(!ContentManager.ready) ContentManager.InitContent(true, false);

        // Get the definitions and sort them by category
        m_skus = new List<string>();

        // Insert "NONE" option at the beginning
		if(attr.m_allowNullValue) {
			m_skus.Add("---- NONE");
		}

        m_skus.AddRange(DefinitionsManager.SharedInstance.GetSkuList(DefinitionsCategory.EQUIPABLE));

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
			}
			OnSkuSelected(selectedIdx);
		}
		
		// Display the property
		// Show button using the popup style with the current value
		m_pos.height = EditorStyles.popup.lineHeight + 5;	// [AOC] Default popup field height + some margin
		Rect contentPos = EditorGUI.PrefixLabel(m_pos, _label);
		if(GUI.Button(contentPos, m_skus[selectedIdx], EditorStyles.popup)) {			
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
		if (m_targetProperty != null) {
            // Store new value - "NONE" will be stored as "" when allowed
            EquipableSkuListAttribute attr = attribute as EquipableSkuListAttribute;
			if(attr.m_allowNullValue && _selectedIdx == 0) {
				m_targetProperty.stringValue = "";
			} else {
				m_targetProperty.stringValue = m_skus[_selectedIdx];	// Options array match the definition skus, so no need to do anything else :)
			}
			m_targetProperty.serializedObject.ApplyModifiedProperties();
		}
	}
}

