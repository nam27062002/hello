// PersistenceProfileEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 21/09/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom editor for a persistence profile object.
/// </summary>
[CustomPropertyDrawer(typeof(PersistenceProfile))]
public class PersistenceProfileEditor : ExtendedPropertyDrawer {
	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// A change has occurred in the inspector, draw the property and store new values.
	/// Use the m_pos member as position reference and update it using AdvancePos() method.
	/// </summary>
	/// <param name="_property">The property we're drawing.</param>
	/// <param name="_label">The label of the property.</param>
	override protected void OnGUIImpl(SerializedProperty _property, GUIContent _label) {
		// We will just display a dropdown list with all the existing profiles plus an extra option to open the profile editing window
		GameObject[] profilePrefabs = Resources.LoadAll<GameObject>(PersistenceProfile.RESOURCES_FOLDER);

		// Get current selection
		PersistenceProfile currentSelectedPrefab = _property.objectReferenceValue as PersistenceProfile;

		// Compose the list
		// CONVENTION: option 0 is "None", option N-1 is "Profile Manager"
		string[] labels = new string[profilePrefabs.Length + 2];	// Extra option to open the editor window and to select none
		int currentSelectedIdx = 0;	// We will look to the previously selected profile during the loop, if not found default to none
		for(int i = 0; i < profilePrefabs.Length; i++) {
			// Init label (1 index offset, first option is special)
			labels[i+1] = profilePrefabs[i].name;

			// Is it the current value?
			if(currentSelectedPrefab != null && currentSelectedPrefab.gameObject.name == profilePrefabs[i].name) {
				currentSelectedIdx = i + 1;
			}
		}

		// Special options:
		labels[0] = "None";
		labels[labels.Length - 1] = "Profile Manager...";

		// Draw the dropdown
		m_pos.height = EditorStyles.popup.lineHeight * 1.1f;	// Add some extra margin
		int newSelection = EditorGUI.Popup(m_pos, _label.text, currentSelectedIdx, labels);
		AdvancePos();

		// Check new selected value
		// a) "None" selected
		if(newSelection == 0) {
			// Clear object reference and return
			_property.objectReferenceValue = null;
		}

		// b) "Profile Manager"
		else if(newSelection == labels.Length - 1) {
			// Open profile manager window with the current profile selected
			PersistenceProfilesEditorWindow.m_selectedProfile = labels[currentSelectedIdx];
			HungryDragonEditorMenu.ShowPersistenceProfilesWindow();
		}

		// c) Different profile than the current one
		else if(newSelection != currentSelectedIdx) {
			// Store the new profile
			_property.objectReferenceValue = profilePrefabs[newSelection - 1];	// Remember we have a +1 offset
		}
	}
}

/// <summary>
/// Custom editor for the persistence manager SaveData object.
/// </summary>
/*
[CustomPropertyDrawer(typeof(PersistenceManager.SaveData))]
public class PersistenceManagerSaveDataEditor : ExtendedPropertyDrawer {
	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// A change has occurred in the inspector, draw the property and store new values.
	/// Use the m_pos member as position reference and update it using AdvancePos() method.
	/// </summary>
	/// <param name="_property">The property we're drawing.</param>
	/// <param name="_label">The label of the property.</param>
	override protected void OnGUIImpl(SerializedProperty _property, GUIContent _label) {
		// Skip label and global foldout
		// Iterate through all the children of the property
		_property.NextVisible(true);	// Enter to the first level of depth
		int targetDepth = _property.depth;
		while(_property.depth == targetDepth) {		// Only direct children, not brothers or grand-children (the latter will be drawn by default if using the default EditorGUI.PropertyField)
			// Draw property as default except for the ones we want to customize
			// Dragons save data: fixed length with custom labels for each level, allow folding it
			if(_property.name == "dragons") {
				// Draw array without allowing resize
				float height = EditorGUILayoutExt.FixedLengthArray(m_pos, _property, DrawDragonSaveData, 10);
				AdvancePos(height);
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

	/// <summary>
	/// Auxiliar method to add some customization to the way we render dragon save data.
	/// </summary>
	/// <returns>The height taken to render the skill property.</returns>
	/// <param name="_pos">Where to draw the property.</param>
	/// <param name="_property">The skill level property to be rendered.</param>
	/// <param name="_idx">The index of the skill level property within the array.</param>
	private float DrawDragonSaveData(Rect _pos, SerializedProperty _property, int _idx) {
		// Make sure each dragon has the right ID assigned
		// [AOC] Tricky: The serialized property expects the real index of the enum entry [0..N-1], we can't use 
		//		 the DragonId vaule directly since it's [-1..N]. Luckily, C# gives us the tools to do so via the enum name.
		// [AOC] Remove from now, DragonId exists no more
		//SerializedProperty idProp = _property.FindPropertyRelative("id");
		//idProp.enumValueIndex = Array.IndexOf(idProp.enumNames, ((DragonId)_idx).ToString());

		// Default property drawer
		_pos.height = EditorGUI.GetPropertyHeight(_property);
		EditorGUI.PropertyField(_pos, _property);
		
		return _pos.height;
	}
}
*/

/// <summary>
/// Custom editor for the DragonData SaveData object.
/// </summary>
[CustomPropertyDrawer(typeof(DragonData.SaveData))]
public class DragonDataSaveDataEditor : ExtendedPropertyDrawer {
	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// A change has occurred in the inspector, draw the property and store new values.
	/// Use the m_pos member as position reference and update it using AdvancePos() method.
	/// </summary>
	/// <param name="_property">The property we're drawing.</param>
	/// <param name="_label">The label of the property.</param>
	override protected void OnGUIImpl(SerializedProperty _property, GUIContent _label) {
		// Draw property label + foldout widget
		// Use dragon sku as foldout label
		m_pos.height = EditorStyles.largeLabel.lineHeight;	// Advance pointer just the size of the label
		SerializedProperty idProp = _property.FindPropertyRelative("sku");
		_property.isExpanded = EditorGUI.Foldout(m_pos, _property.isExpanded, idProp.stringValue);
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

				//Debug.Log(_property.name);
				if(_property.name == "putHereThePropertyYouDontWantToShow") {
					// Do ntohing
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

			// Indent back out
			EditorGUI.indentLevel--;
		}
	}
}