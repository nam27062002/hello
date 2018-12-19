// SeasonTriggerInstanciator.cs
// Hungry Dragon
// 
// Created by Miguel Ángel Linares on 18/12/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

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
/// Custom editor for the SeasonTriggerInstanciator class.
/// </summary>
[CustomEditor(typeof(SeasonTriggerInstanciator), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class SeasonTriggerInstanciatorEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	SeasonTriggerInstanciator m_targetSeasonTrigger = null;

	// Aux vars
	List<string> m_seasonSkus = null;
	private List<string> seasonSkus {
		get { 
			if(m_seasonSkus == null) {
				// If definitions are not loaded, do it now
				if(!ContentManager.ready) ContentManager.InitContent(true, false);
				m_seasonSkus = new List<string>();
				m_seasonSkus.AddRange(DefinitionsManager.SharedInstance.GetSkuList(DefinitionsCategory.SEASONS));	// Returns a reference!! We're modifying the list by adding "none", so make a copy instead
				if(m_seasonSkus.Count == 0) {	// Should never be empty!
					m_seasonSkus = null;
				} else {
					m_seasonSkus.Insert(0, SeasonManager.NO_SEASON_SKU);
				}
			}
			return m_seasonSkus;
		}
	}

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetSeasonTrigger = target as SeasonTriggerInstanciator;
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_targetSeasonTrigger = null;
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

		// Loop through all serialized properties and work with special ones
		SerializedProperty p = serializedObject.GetIterator();
		p.Next(true);	// To get first element
		do {
			// Properties requiring special treatment
			// Unity's "script" property
			if(p.name == "m_Script") {
				// Draw the property, disabled
				bool wasEnabled = GUI.enabled;
				GUI.enabled = false;
				EditorGUILayout.PropertyField(p, true);
				GUI.enabled = wasEnabled;
			}

			// Seasons dictionary
			else if(p.name == "m_seasonsData") {
				// Something went wrong!
				if(seasonSkus == null) {
					EditorGUILayout.HelpBox("Season Definitions could not be loaded!", MessageType.Error, true);
					continue;
				}

				// Go season by season
				for(int i = 0; i < seasonSkus.Count; ++i) {
					DoSeason(seasonSkus[i], p);
				}

				// Purge unknown seasons from the list (shouldn't happen)
				// Reverse loop, we're removing stuff from the array!
				for(int i = p.arraySize - 1; i >= 0; --i) {
					string sku = p.GetArrayElementAtIndex(i).FindPropertyRelative("sku").stringValue;
					if(!m_seasonSkus.Contains(sku)) {
						p.DeleteArrayElementAtIndex(i);
					}
				}
			}

			// Properties we don't want to show
			else if(p.name == "m_ObjectHideFlags") {
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
	/// Do the layout for a single season.
	/// </summary>
	/// <param name="_seasonSku">Season sku.</param>
	/// <param name="_listProperty">List property.</param>
	private void DoSeason(string _seasonSku, SerializedProperty _listProperty) {
		// Aux vars
		SerializedProperty p = null;

		// Get the property matching this sku
		SerializedProperty seasonProp = null;
		bool found = false;
		for(int i = 0; i < _listProperty.arraySize; ++i) {
			seasonProp = _listProperty.GetArrayElementAtIndex(i);
			p = seasonProp.FindPropertyRelative("sku"); 
			if(p.stringValue == _seasonSku) {
				found = true;
				break;
			}
		}

		// If the list didn't have any entry for the target season, create one!
		if(!found) {
			// For convenience, use the same index as the definitions list when possible
			int idx = Mathf.Min(m_seasonSkus.IndexOf(_seasonSku), _listProperty.arraySize);
			_listProperty.InsertArrayElementAtIndex(idx);
			seasonProp = _listProperty.GetArrayElementAtIndex(idx);

			// Set sku
			p = seasonProp.FindPropertyRelative("sku");
			p.stringValue = _seasonSku;
		}

		// Finally! Draw season property
		SerializedProperty targetsProp = seasonProp.FindPropertyRelative("targets");

		// Foldable prefix
		seasonProp.isExpanded = EditorGUILayout.Foldout(seasonProp.isExpanded, _seasonSku + " (" + targetsProp.arraySize + " objects)");
		if(seasonProp.isExpanded) {
			// Indent in
			EditorGUI.indentLevel++;

			// Show target objects list
			SerializedProperty targetProp = null;
			for(int i = 0; i < targetsProp.arraySize; ++i) {
				// Get property
				targetProp = targetsProp.GetArrayElementAtIndex(i);

				// Join object ref, toggle and remove button in a single line
				EditorGUILayout.BeginHorizontal(); {
					// InstanciateRoot
					p = targetProp.FindPropertyRelative("root");
					p.objectReferenceValue = EditorGUILayout.ObjectField(p.objectReferenceValue, typeof(Transform), true);

					// Resource
					// GUILayout.Space(-15f);	// Remove useless space
					p = targetProp.FindPropertyRelative("resource");
					EditorGUILayout.PropertyField(p, GUIContent.none, true);

					// Remove button
					GUI.backgroundColor = Color.red;
					if(GUILayout.Button("-", GUILayout.Width(20f))) {
						// Remove and move index back
						targetsProp.DeleteArrayElementAtIndex(i);
						i--;
					}
					GUI.backgroundColor = Color.white;
				} EditorGUILayout.EndHorizontal();
			}

			// Add new element button
			EditorGUILayout.BeginHorizontal(); {
				// Simulate indentation
				GUI.backgroundColor = Color.green;
				GUILayout.Space(EditorGUI.indentLevel * 15f);
				if(GUILayout.Button("+")) {
					targetsProp.InsertArrayElementAtIndex(targetsProp.arraySize);
				}
				GUI.backgroundColor = Color.white;
			} EditorGUILayout.EndHorizontal();

			// Indent out
			EditorGUI.indentLevel--;
		}
	}
}