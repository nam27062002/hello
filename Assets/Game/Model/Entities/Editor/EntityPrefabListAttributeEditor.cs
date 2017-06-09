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
using System.IO;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Drawer for the EntitySkuList custom attribute.
/// Simpler version of the SkuListAttributeEditor since we know exactly the type of definition we're editing so we don't 
/// have to deal with generics and reflection black magic.
/// </summary>
[CustomPropertyDrawer(typeof(EntityPrefabListAttribute))]
public class EntityPrefabListAttributeEditor : ExtendedPropertyDrawer {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Data that mus be kept between opening the sku list popup and capturing its result
	private List<string> m_prefabNames = null;
	private SerializedProperty m_targetProperty = null;

	string m_prefabsPath =  Application.dataPath + "/Resources/" + IEntity.ENTITY_PREFABS_PATH;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	void BuildPrefabList() {
		// Create the prefab name list
		m_prefabNames = new List<string>();
		string[] files = Directory.GetFiles(m_prefabsPath, "*.prefab", SearchOption.AllDirectories);
		foreach (string f in files) 
		{
			string name = f.Substring(m_prefabsPath.Length);
			name = name.Substring(0, name.Length - (".prefab").Length);
			name = name.Replace("\\", "/");	// [AOC] Unify dir separator character between Windows and OSX!
			m_prefabNames.Add(name);
		}
	}

	/// <summary>
	/// A change has occurred in the inspector, draw the property and store new values.
	/// Use the m_pos member as position reference and update it using AdvancePos() method.
	/// </summary>
	/// <param name="_property">The property we're drawing.</param>
	/// <param name="_label">The label of the property.</param>
	override protected void OnGUIImpl(SerializedProperty _property, GUIContent _label) {
		m_targetProperty = _property;

		// Check field type
		if (_property.propertyType != SerializedPropertyType.String) {
			m_pos.height = EditorStyles.largeLabel.lineHeight;
			EditorGUI.LabelField(m_pos, _label.text, "ERROR! EntityPrefabList attribute can only be applied to string properties!");
			AdvancePos();
			return;
		}

		// Obtain the attribute
		EntitySkuListAttribute attr = attribute as EntitySkuListAttribute;

		if (m_prefabNames == null) {
			BuildPrefabList();
		}

		// Find out current selected value
		// If current value was not found, force it to first value if "NONE" allowed or first non-category value if not allowed
		int selectedIdx = m_prefabNames.FindIndex(_sku => _property.stringValue.Equals(_sku, StringComparison.Ordinal));
		if (selectedIdx < 0) {
			selectedIdx = 0;
			OnPreafabSelected(0);
		}

		EditorGUI.BeginChangeCheck();
		m_pos.height = EditorStyles.popup.lineHeight + 5;	// [AOC] Default popup field height + some margin
		selectedIdx = EditorGUI.Popup(m_pos, "Path", selectedIdx, m_prefabNames.ToArray());
		if (EditorGUI.EndChangeCheck()) {
			OnPreafabSelected(selectedIdx);
		}

		// Leave room for next property drawer
		AdvancePos();
	}

	/// <summary>
	/// A sku has been selected in the selection popup.
	/// </summary>
	/// <param name="_selectedIdx">The index of the newly selected option.</param>
	private void OnPreafabSelected(int _selectedIdx) {
		// Store new value - "NONE" will be stored as "" when allowed
		EntitySkuListAttribute attr = attribute as EntitySkuListAttribute;
		m_targetProperty.stringValue = m_prefabNames[_selectedIdx];	// Options array match the definition skus, so no need to do anything else :)
		m_targetProperty.serializedObject.ApplyModifiedProperties();
	}
}

