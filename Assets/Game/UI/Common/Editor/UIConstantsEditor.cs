// UIConstantsEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 15/02/2017.
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
/// Custom editor for the UIConstants class.
/// </summary>
[CustomEditor(typeof(UIConstants))]	// True to be used by heir classes as well
public class UIConstantsEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private const string CAT_TMP_SHORTCUTS = "TMP Shortcuts";
	private const string CAT_COLORS = "Colors";
	private const string CAT_RARITIES = "Rarities";
	private const string CAT_ASSET_PATHS = "Asset paths in Resources";
	private const string CAT_POWER_COLORS = "Power Colors";
	private const string CAT_ANIMATION_RESULTS = "Results Screen Anim Setup";
	private const string CAT_ANIMATION_OPEN_EGG = "Open Egg Screen Anim Setup";
	private const string CAT_OTHERS = "Others";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	private UIConstants m_targetUIConstants = null;

	// Properties by category
	private Dictionary<string, List<SerializedProperty>> m_categorizedProperties = new Dictionary<string, List<SerializedProperty>>();

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetUIConstants = target as UIConstants;

		// Clear and intiialize dictionary
		// Loop through all serialized properties and parse properties names to determine their category
		m_categorizedProperties.Clear();
		SerializedProperty p = serializedObject.GetIterator();
		p.Next(true);	// To get first element
		do {
			// Properties we don't want to show
			if(p.name == "m_ObjectHideFlags"	// Unity's hide flags
				|| p.name == "m_Script") {		// Unity's "script" property
				// Do nothing
			}

			// Parse names
			// Order is relevant in case of conflict! (i.e. "Color" and "PowerColor")
			else if(p.name.Contains("m_tmp")) {
				AddToCategory(CAT_TMP_SHORTCUTS, p);
			} else if(p.name.Contains("m_powerColor")) {
				AddToCategory(CAT_POWER_COLORS, p);
			} else if(p.name.Contains("m_rarity")) {
				AddToCategory(CAT_RARITIES, p);
			} else if(p.name.Contains("Path")) {
				AddToCategory(CAT_ASSET_PATHS, p);
			} else if(p.name.Contains("m_results")) {
				AddToCategory(CAT_ANIMATION_RESULTS, p);
			} else if(p.name.Contains("m_openEgg")) {
				AddToCategory(CAT_ANIMATION_OPEN_EGG, p);
			} else if(p.name.Contains("Color")) {
				AddToCategory(CAT_COLORS, p);
			}

			// Unidentified property
			else {
				AddToCategory(CAT_OTHERS, p);
			}
		} while(p.NextVisible(false));		// Only direct children, not grand-children (will be drawn by default if using the default EditorGUI.PropertyField)
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_targetUIConstants = null;
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

		// Just do all the categories in the desired order
		DoCategory(CAT_COLORS);
		DoCategory(CAT_RARITIES);
		DoCategory(CAT_TMP_SHORTCUTS);
		DoCategory(CAT_ASSET_PATHS);
		DoCategory(CAT_POWER_COLORS);
		DoCategory(CAT_ANIMATION_OPEN_EGG);
		DoCategory(CAT_ANIMATION_RESULTS);
		DoCategory(CAT_OTHERS);

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
	// INTERNAL																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Adds a property to a category. If category list is not initialized, do it.
	/// </summary>
	/// <param name="_category">Target cateegory.</param>
	/// <param name="_p">Property to be added.</param>
	private void AddToCategory(string _category, SerializedProperty _p) {
		// Is category initialized?
		if(!m_categorizedProperties.ContainsKey(_category)) {
			// No, do it!
			m_categorizedProperties[_category] = new List<SerializedProperty>();
		}

		// Add property to the target list
		// [AOC] Copy, since _p comes from an iterator and we want the reference to the property, not the iterator!
		m_categorizedProperties[_category].Add(_p.Copy());
	}

	/// <summary>
	/// Do the OnGUI() call for the target category.
	/// </summary>
	/// <param name="_category">Category to be displayed.</param>
	private void DoCategory(string _category) {
		// Make sure category is valid and not empty
		List<SerializedProperty> props = null;
		if(!m_categorizedProperties.TryGetValue(_category, out props)) return;
		if(props.Count == 0) return;

		// Category foldout
		bool expanded = EditorPrefs.GetBool("UIConstants." + _category, false);		// Collapsed by default
		EditorGUILayoutExt.Separator();
		expanded = EditorGUILayout.Foldout(expanded, _category);
		EditorPrefs.SetBool("UIConstants." + _category, expanded);

		// Do properties!
		if(expanded) {
			EditorGUI.indentLevel++;
			for(int i = 0; i < props.Count; i++) {
				// Properties requiring special treatment
				if(props[i].name == "m_rarityColors"
				|| props[i].name == "m_rarityIcons") {
					// Fixed length arrays!
					EditorGUILayoutExt.FixedLengthArray(
						props[i], 
						(SerializedProperty _p, int _idx) => {
							EditorGUILayout.PropertyField(_p, new GUIContent(((EggReward.Rarity)_idx).ToString()), true);
						}, 
						(int)EggReward.Rarity.COUNT
					);
				}

				// Default property display
				else {
					EditorGUILayout.PropertyField(props[i], true);
				}
			}
			EditorGUI.indentLevel--;
		}
	}
}