// MenuDragonLoaderEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 14/02/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom editor for the MenuDragonLoader class.
/// </summary>
[CustomEditor(typeof(MenuDragonLoader), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class MenuDragonLoaderEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private const string EQUIPPED_DISGUISE_NAME = "EQUIPPED";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	MenuDragonLoader m_targetMenuDragonLoader = null;

	// Important properties
	SerializedProperty m_modeProp = null;
	SerializedProperty m_dragonSkuProp = null;
	SerializedProperty m_disguiseSkuProp = null;

	// Internal
	private string[] m_dragonSkus = new string[0];
	private string[] m_disguiseSkus = new string[0];

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetMenuDragonLoader = target as MenuDragonLoader;

		// Get references to important properties
		m_modeProp = serializedObject.FindProperty("m_mode");
		m_dragonSkuProp = serializedObject.FindProperty("m_dragonSku");
		m_disguiseSkuProp = serializedObject.FindProperty("m_disguiseSku");

		// If definitions are not loaded, do it now
		if(!ContentManager.ready){
			ContentManager.InitContent(true);
		}

		// Cache some important data
		m_dragonSkus = DefinitionsManager.SharedInstance.GetSkuList(DefinitionsCategory.DRAGONS).ToArray();
		GetDisguiseSkus(m_dragonSkuProp.stringValue);
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Lose references to important properties
		m_modeProp = null;
		m_dragonSkuProp = null;
		m_disguiseSkuProp = null;

		// Clear target object
		m_targetMenuDragonLoader = null;
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

			// Dragon section
			else if(p.name == "m_mode") {
				// Do the property
				EditorGUILayout.PropertyField(p, true);

				// Only in manual mode, show dragon sku and disguise sku fields
				if(p.enumValueIndex == (int)MenuDragonLoader.Mode.MANUAL) {
					// Dragon sku list
					// Find out selected index
					int oldIdx = m_dragonSkus.IndexOf(m_dragonSkuProp.stringValue);

					// Display list and store new index
					int newIdx = EditorGUILayout.Popup(m_dragonSkuProp.displayName, oldIdx, m_dragonSkus);

					// Special case: if nothing is selected, select first option
					if(newIdx < 0 && m_dragonSkus.Length > 0) newIdx = 0;

					// If dragon has changed, store new value and update disguises list
					if(oldIdx != newIdx && m_dragonSkus.Length > 0) {
						// Update property
						m_dragonSkuProp.stringValue = m_dragonSkus[newIdx];

						// Load disguises list for the new dragon
						GetDisguiseSkus(m_dragonSkus[newIdx]);
					}

					// Skins list (for the selected dragon)
					// Find out selected index
					oldIdx = m_disguiseSkus.IndexOf(m_disguiseSkuProp.stringValue);
					if(oldIdx < 0) oldIdx = m_disguiseSkus.Length - 1;	// If not found, treat it as if the loader is using the currently equipped disguise (property value "", last index)

					// Display list and store new index
					newIdx = EditorGUILayout.Popup(m_disguiseSkuProp.displayName, oldIdx, m_disguiseSkus);

					// If selected disguise has changed, store new value
					if(oldIdx != newIdx) {
						// Special case: if "equipped" option was selected, store it as empty string
						if(m_disguiseSkus[newIdx] == EQUIPPED_DISGUISE_NAME) {
							m_disguiseSkuProp.stringValue = string.Empty;
						} else {
							m_disguiseSkuProp.stringValue = m_disguiseSkus[newIdx];
						}
					}
				}
			}

			// Placeholder dragon sku, show only in current/selected modes
			else if(p.name == "m_placeholderDragonSku") {
				// Draw a separator (first item of the "Debug" section
				EditorGUILayoutExt.Separator(new SeparatorAttribute("Debug"));

				// Only if mode is not manual
				if(m_modeProp.enumValueIndex != (int)MenuDragonLoader.Mode.MANUAL) {
					// Default property rendering
					EditorGUILayout.PropertyField(p, true);
				}
			}

			// Properties we don't want to show or that are already processed
			else if(p.name == "m_ObjectHideFlags"
				|| p.name == "m_dragonSku"
				|| p.name == "m_disguiseSku") {
				// Do nothing
			}

			// Default property display
			else {
				EditorGUILayout.PropertyField(p, true);
			}
		} while(p.NextVisible(false));		// Only direct children, not grand-children (will be drawn by default if using the default EditorGUI.PropertyField)

		// Apply changes to the serialized object - always do this in the end of OnInspectorGUI.
		serializedObject.ApplyModifiedProperties();

		// Force loading the dragon
		if(GUILayout.Button("Load Dragon", GUILayout.Height(50))) {
			for(int i = 0; i < targets.Length; i++) {
				// If the game is not running, we don't have any data on current dragon/skin, so load a placeholder one manually instead
				((MenuDragonLoader)targets[i]).RefreshDragon();
			}
		}
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
	/// Initialize the disguise skus list with the disguises for the given dragon.
	/// </summary>
	/// <param name="_dragonSku">Sku of the dragon whose disguises we want.</param>
	private void GetDisguiseSkus(string _dragonSku) {
		// If definitions are not loaded, do it now
		if(!ContentManager.ready){
			ContentManager.InitContent(true);
		}

		// Get all disguises linked to the requested dragon
		List<DefinitionNode> disguiseDefs = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.DISGUISES, "dragonSku", _dragonSku);

		// Resize and initialize array
		if(disguiseDefs != null) {
			m_disguiseSkus = new string[disguiseDefs.Count + 1];
			for(int i = 0; i < disguiseDefs.Count; i++) {
				m_disguiseSkus[i] = disguiseDefs[i].sku;
			}
		} else {
			m_disguiseSkus = new string[1];
		}

		// Always add the "EQUIPPED" option at the end
		m_disguiseSkus[m_disguiseSkus.Length - 1] = EQUIPPED_DISGUISE_NAME;
	}
}