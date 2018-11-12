// RewardSceneControllerEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 21/11/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom editor for the RewardSceneController class.
/// </summary>
[CustomEditor(typeof(RewardSceneController), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class RewardSceneControllerEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	RewardSceneController m_targetRewardSceneController = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetRewardSceneController = target as RewardSceneController;
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_targetRewardSceneController = null;
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

			else if(p.name == "m_rarityFXSetup") {
				// Fixed length array, rarity as element label
				int size = (int)Metagame.Reward.Rarity.COUNT;
				p.arraySize = size;

				// Group in a foldout
				p.isExpanded = EditorGUILayout.Foldout(p.isExpanded, p.displayName);
				if(p.isExpanded) {
					// Show them nicely formatted
					EditorGUI.indentLevel++;
					for(int i = 0; i < size; ++i) {
						// Default drawer, using rarity name as label
						EditorGUILayout.PropertyField(
							p.GetArrayElementAtIndex(i), 
							new GUIContent(((Metagame.Reward.Rarity)i).ToString()),
							true
						);
					}
					EditorGUI.indentLevel--;
				}
			}

			else if(p.name == "m_goldenFragmentsRewardsSetup") {
				// Fixed length array, rarity as element label
				int size = (int)Metagame.Reward.Rarity.COUNT;	// Special rewards don't give golden fragments!
				p.arraySize = size;

				// Group in a foldout
				p.isExpanded = EditorGUILayout.Foldout(p.isExpanded, p.displayName);
				if(p.isExpanded) {
					// Show them nicely formatted
					EditorGUI.indentLevel++;
					for(int i = 0; i < size; ++i) {
						// Default drawer, using rarity name as label
						EditorGUILayout.PropertyField(
							p.GetArrayElementAtIndex(i), 
							new GUIContent(((Metagame.Reward.Rarity)i).ToString()),
							true
						);
					}
					EditorGUI.indentLevel--;
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
	/// The scene is being refreshed.
	/// </summary>
	public void OnSceneGUI() {
		// Scene-related stuff
	}
}