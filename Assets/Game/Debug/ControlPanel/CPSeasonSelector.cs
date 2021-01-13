// CPSeasonSelector.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 28/11/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

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
public class CPSeasonSelector : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string DEFAULT_SEASON_SKU = "default";	// Default behaviour (no override)

	//------------------------------------------------------------------------//
	// STATIC PROPERTIES													  //
	//------------------------------------------------------------------------//
	public const string FORCED_SEASON_SKU = "CPSeasonSelector.FORCED_SEASON_SKU";
	public static string forcedSeasonSku {
		get { return Prefs.GetStringPlayer(FORCED_SEASON_SKU, DEFAULT_SEASON_SKU); }
		set { Prefs.SetStringPlayer(FORCED_SEASON_SKU, value); }
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[SerializeField] private TMP_Dropdown m_dropdown = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Load season skus
		List<string> seasonSkus = new List<string>();	// Create a new copy since DefinitionsManager returns a list reference and we want to modify the list
		seasonSkus.AddRange(DefinitionsManager.SharedInstance.GetSkuList(DefinitionsCategory.SEASONS));

		// Add none
		seasonSkus.Insert(0, SeasonManager.NO_SEASON_SKU);

		// Add default
		seasonSkus.Insert(0, DEFAULT_SEASON_SKU);

		// Initialize dropdown
		m_dropdown.ClearOptions();
		string selectedSku = forcedSeasonSku;
		int selectedIdx = 0;
		for(int i = 0; i < seasonSkus.Count; i++) {
			// Add the option
			m_dropdown.options.Add(new TMP_Dropdown.OptionData(seasonSkus[i]));

			// Is it the current option?
			if(seasonSkus[i] == selectedSku) {
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
		// Update pref
		forcedSeasonSku = m_dropdown.options[_newValue].text;

		// Update manager
		SeasonManager.instance.RefreshActiveSeason();
	}
}