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
public class UIConstantsEditor : CategorizedEditor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private const string CAT_TMP_SHORTCUTS = "TMP Shortcuts";
	private const string CAT_COLORS = "Colors";
	private const string CAT_RARITIES = "Rarities";
	private const string CAT_ASSET_PATHS = "Asset paths in Resources";
	private const string CAT_ANIMATION_SETUPS = "Animation Setups";
	private const string CAT_OTHERS = "Others";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	override protected void InitCategories() {
		// Clear and intialize categories dictionary
		m_categories[CAT_TMP_SHORTCUTS] = new Category(CAT_TMP_SHORTCUTS);
		m_categories[CAT_COLORS] = new Category(CAT_COLORS);
		m_categories[CAT_RARITIES] = new Category(CAT_RARITIES);
		m_categories[CAT_ASSET_PATHS] = new Category(CAT_ASSET_PATHS);
		m_categories[CAT_ANIMATION_SETUPS] = new Category(CAT_ANIMATION_SETUPS);
		m_categories[CAT_OTHERS] = new Category(CAT_OTHERS);

		// Loop through all serialized properties and parse properties names to determine their category
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
				m_categories[CAT_TMP_SHORTCUTS].Add(p);
			} else if(p.name.Contains("m_color")) {
				m_categories[CAT_COLORS].Add(p, "Misc");
			} else if(p.name.Contains("m_powerColor")) {
				m_categories[CAT_COLORS].Add(p, "Power Colors");
			} else if(p.name.Contains("m_petCategoryColor")) {
				m_categories[CAT_COLORS].Add(p, "Pet Category Colors");
			} else if(p.name.Contains("m_dragonStatColor")) {
				m_categories[CAT_COLORS].Add(p, "Dragon Stats Colors");
			} else if(p.name.Contains("m_dragonTierColors")) {
				m_categories[CAT_COLORS].Add(p);
			} else if(p.name.Contains("m_rarity")) {
				m_categories[CAT_RARITIES].Add(p);
			} else if(p.name.Contains("Path")) {
				m_categories[CAT_ASSET_PATHS].Add(p);
			} else if(p.name.Contains("m_results")) {
				m_categories[CAT_ANIMATION_SETUPS].Add(p, "Results Screen");
			} else if(p.name.Contains("m_openEgg")) {
				m_categories[CAT_ANIMATION_SETUPS].Add(p, "Open Egg Screen");
			} else if(p.name.Contains("m_menu")) {
				m_categories[CAT_ANIMATION_SETUPS].Add(p, "Menu");
			} else if(p.name.Contains("Gradient")) {
				m_categories[CAT_COLORS].Add(p);
			}

			// Unidentified property
			else {
				m_categories[CAT_OTHERS].Add(p);
			}
		} while(p.NextVisible(false));		// Only direct children, not grand-children (will be drawn by default if using the default EditorGUI.PropertyField)
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
		DoCategory(CAT_ANIMATION_SETUPS);
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
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Do the OnGUI() call for the target property.
	/// Override to treat some properties differently.
	/// </summary>
	/// <param name="_p">Property to be displayed.</param>
	override protected void DoProperty(SerializedProperty _p) {
		// Properties requiring special treatment
		// Rarity Arrays
		if(_p.name == "m_rarityColors"
		|| _p.name == "m_rarityTextGradients"
		|| _p.name == "m_rarityIcons") {
			// Fixed length arrays!
			EditorGUILayoutExt.FixedLengthArray(
				_p, 
				(SerializedProperty _prop, int _idx) => {
					EditorGUILayout.PropertyField(_prop, new GUIContent(((Metagame.Reward.Rarity)_idx).ToString()), true);
				}, 
				(int)Metagame.Reward.Rarity.COUNT
			);
		}

		// Dragon Tier Arrays
		else if(_p.name == "m_dragonTiersSFX"
        || _p.name == "m_dragonTierColors") {
			// Fixed length arrays!
			EditorGUILayoutExt.FixedLengthArray(
				_p,
				(SerializedProperty _prop, int _idx) => {
					EditorGUILayout.PropertyField(_prop, new GUIContent(((DragonTier)_idx).ToString()), true);
				},
				(int)DragonTier.COUNT
			);
		}

		// Special devices safe areas
		else if(_p.name == "m_safeAreas") {
			// Fixed length arrays!
			EditorGUILayoutExt.FixedLengthArray(
				_p, 
				(SerializedProperty _prop, int _idx) => {
					EditorGUILayout.PropertyField(_prop, new GUIContent(((UIConstants.SpecialDevice)_idx).ToString()), true);
				}, 
				(int)UIConstants.SpecialDevice.COUNT
			);
		}

		// Default property display
		else {
			base.DoProperty(_p);
		}
	}
}