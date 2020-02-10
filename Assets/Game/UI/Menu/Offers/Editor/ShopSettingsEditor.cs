// ShopSettingsEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 07/02/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

using System;
using System.Reflection;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom editor for the ShopSettings class.
/// </summary>
[CustomEditor(typeof(ShopSettings), true)]	// True to be used by heir classes as well
public class ShopSettingsEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	private ShopSettings m_targetShopSettings = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetShopSettings = target as ShopSettings;

		// Initialize rewards types list
		ValidateTypesList();
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_targetShopSettings = null;
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

		// Unity's "script" property - draw disabled
		EditorGUI.BeginDisabledGroup(true);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"), true);
		EditorGUI.EndDisabledGroup();

		// Loop through all serialized properties and work with special ones
		SerializedProperty p = serializedObject.GetIterator();
		p.Next(true);	// To get first element
		do {
			// Properties requiring special treatment
			if(p.name == "m_itemTypesSetup") {
				// Forced array size, so just display all properties one after another
				// Header
				EditorGUILayoutExt.Separator("Item Types Setup");

				// Loop through items and use their custom property drawer
				for(int i = 0; i < p.arraySize; ++i) {
					EditorGUILayout.PropertyField(p.GetArrayElementAtIndex(i), true);
				}
			}

			// Properties we don't want to show
			else if(p.name == "m_ObjectHideFlags" || p.name == "m_Script") {
				// Do nothing
			}

			// Default property display
			else {
				EditorGUILayout.PropertyField(p, true);
			}
		} while(p.NextVisible(false));		// Only direct children, not grand-children (will be drawn by default if using the default EditorGUI.PropertyField)

		// Apply changes to the serialized object - always do this in the end of OnInspectorGUI.
		serializedObject.ApplyModifiedProperties();
	}

	/// <summary>
	/// The scene is being refreshed.
	/// </summary>
	public void OnSceneGUI() {
		// Scene-related stuff
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Make sure the object's types list has exactly one entry per valid reward type.
	/// </summary>
	private void ValidateTypesList() {
		// Use reflection to get a list of all types inheriting from Metagame.Reward
		List<Type> derivedTypes = TypeUtil.FindAllDerivedTypes(typeof(Metagame.Reward));

		// Ignore some internal and deprecated types
		List<string> toIgnore = new List<string>(new string[] {
			Metagame.RewardMulti.TYPE_CODE,
			Metagame.RewardMultiEgg.TYPE_CODE,
			Metagame.RewardGoldenFragments.TYPE_CODE
		});

		// Filter only those containing a TYPE_CODE constant
		HashSet<string> rewardTypes = new HashSet<string>();
		for(int i = 0; i < derivedTypes.Count; ++i) {
			FieldInfo field = derivedTypes[i].GetField("TYPE_CODE", BindingFlags.Public | BindingFlags.Static);
			if(field != null) {
				// Skip if it's one of the types to ignore
				string typeCode = (string)field.GetValue(null); // No need to give an instance since it's a static field :)
				if(toIgnore.Contains(typeCode)) continue;

				// Valid type! Store it
				rewardTypes.Add(typeCode);
			}
		}

		// Validate types property
		// The list must have exactly one entry per accepted reward type
		serializedObject.Update();
		SerializedProperty p = serializedObject.FindProperty("m_itemTypesSetup");

		// Remove duplicated and unknown types
		// Reverse loop, we're removing stuff from the list!
		HashSet<string> processedTypes = new HashSet<string>();
		for(int i = p.arraySize - 1; i >= 0; --i) {
			// Valid type?
			string typeCode = p.GetArrayElementAtIndex(i).FindPropertyRelative("type").stringValue;
			if(rewardTypes.Contains(typeCode)) {
				// Yes! Mark it as processed
				if(!processedTypes.Add(typeCode)) {
					// Type was already processed! Duplicated type, remove it from the list
					p.DeleteArrayElementAtIndex(i);
				}
			} else {
				// No! Unknwon or unsupported type, remove it from the list
				p.DeleteArrayElementAtIndex(i);
			}
		}

		// Add missing types
		foreach(string typeCode in rewardTypes) {
			// If it wasn't processed in the previous loop, it's not on the list
			if(!processedTypes.Contains(typeCode)) {
				// Add it! Increasing the array size does the trick
				p.arraySize++;

				// When adding a new element to the array, last one is copied
				SerializedProperty newItemProp = p.GetArrayElementAtIndex(p.arraySize - 1);

				// Reset its values
				newItemProp.ResetValue();

				// Set type
				newItemProp.FindPropertyRelative("type").stringValue = typeCode;
			}
		}

		// Apply changes to the serialized object
		serializedObject.ApplyModifiedProperties();
	}
}