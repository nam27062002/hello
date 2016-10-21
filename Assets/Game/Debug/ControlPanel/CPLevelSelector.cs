// CPLevelSelector.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 29/09/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public class CPLevelSelector : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[SerializeField] private Dropdown m_dropdown = null;

	// Internal
	private List<DefinitionNode> m_levelDefs = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
		if(isActiveAndEnabled) {
			LoadLevels();
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Reload the level list.
	/// </summary>
	private void LoadLevels() {
		// Skip if already loaded
		if(m_levelDefs != null) return;

		// If the current screen is NOT the loading screen, we're good, everything is loaded
		if(GameSceneManager.currentScene == LoadingSceneController.NAME) return;

		// Refresh the levels list
		m_levelDefs = new List<DefinitionNode>();
		DefinitionsManager.SharedInstance.GetDefinitions(DefinitionsCategory.LEVELS, ref m_levelDefs);

		// Update dropdown
		m_dropdown.ClearOptions();
		int selectedIdx = 0;
		for(int i = 0; i < m_levelDefs.Count; i++) {
			// Add the option
			m_dropdown.options.Add(new Dropdown.OptionData(m_levelDefs[i].GetLocalized("tidName")));

			// Is it the current level?
			if(m_levelDefs[i].sku == UsersManager.currentUser.currentLevel) {
				selectedIdx = i;
			}
		}

		// Set selection
		m_dropdown.value = selectedIdx;

		// [AOC] Dropdown seems to be bugged -_-
		m_dropdown.captionText.text = m_dropdown.options[m_dropdown.value].text;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// A new level has been picked on the control panel.
	/// </summary>
	public void OnValueChanged(int _newValue) {
		// Ignore if we're not ready
		if(m_levelDefs == null) return;

		// If different than current one, update and save persistence
		_newValue = Mathf.Clamp(_newValue, 0, m_levelDefs.Count - 1);
		string newLevelSku = m_levelDefs[_newValue].sku;
		if(newLevelSku != UsersManager.currentUser.currentLevel) {
			UsersManager.currentUser.currentLevel = newLevelSku;
			PersistenceManager.Save();
		}
	}
}