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
using TMPro;

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
	[SerializeField] private TMP_Dropdown m_dropdown = null;

	// Internal
	private List<LevelData> m_levelDatas = null;
	
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
		if(m_levelDatas != null) return;

		// If the current screen is NOT the loading screen, we're good, everything is loaded
		if(GameSceneManager.currentScene == LoadingSceneController.NAME) return;

		// Refresh the levels list
		m_levelDatas = new List<LevelData>();
		List<string> levelSkus = DefinitionsManager.SharedInstance.GetSkuList(DefinitionsCategory.LEVELS);

		// Update dropdown
		m_dropdown.ClearOptions();
		int selectedIdx = 0;
		for(int i = 0; i < levelSkus.Count; i++) {
			// Load level data
			LevelData data = LevelManager.GetLevelData(levelSkus[i]);
			if(data == null) continue;
			m_levelDatas.Add(data);

			// Add the option
			m_dropdown.options.Add(new TMP_Dropdown.OptionData(data.debugName));

			// Is it the current level?
			if(levelSkus[i] == UsersManager.currentUser.currentLevel) {
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
		if(m_levelDatas == null) return;

		// If different than current one, update and save persistence
		_newValue = Mathf.Clamp(_newValue, 0, m_levelDatas.Count - 1);
		string newLevelSku = m_levelDatas[_newValue].def.sku;
		if(newLevelSku != UsersManager.currentUser.currentLevel) {
			UsersManager.currentUser.currentLevel = newLevelSku;
			PersistenceManager.Save();
		}
	}
}