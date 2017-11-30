// SeasonManager.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 27/11/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple manager to centralize seasonal stuff ("christmas", "halloween", etc).
/// </summary>
public class SeasonManager : Singleton<SeasonManager> {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string NO_SEASON_SKU = "none";
	private const string ACTIVE_SEASON_CACHE_KEY = "SeasonManager.activeSeasonSku";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	private string m_activeSeason = NO_SEASON_SKU;
	public static string activeSeason {
		get { return instance.m_activeSeason; }
	}

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public SeasonManager() {
		// Initialize current season
		RefreshActiveSeason();
	}

	/// <summary>
	/// Refresh current season taking ControlPanel in account.
	/// </summary>
	public void RefreshActiveSeason() {
		string forcedSeasonSku = CPSeasonSelector.forcedSeasonSku;
		switch(forcedSeasonSku) {
			// Default behaviour
			case CPSeasonSelector.DEFAULT_SEASON_SKU: {
				// Load active season from content
				// If content is not ready, load it from cache
				if(!ContentManager.ready) {
					// Load from cache
					m_activeSeason = PlayerPrefs.GetString(ACTIVE_SEASON_CACHE_KEY, NO_SEASON_SKU);
				} else {
					// Load from content - first definition with the "active" field set to true
					DefinitionNode activeSeasonDef = DefinitionsManager.SharedInstance.GetDefinitionByVariable(
						DefinitionsCategory.SEASONS, 
						"active", 
						"true"
					);

					// Store new season
					m_activeSeason = activeSeasonDef == null ? NO_SEASON_SKU : activeSeasonDef.sku;

					// Cache for future use
					PlayerPrefs.SetString(ACTIVE_SEASON_CACHE_KEY, m_activeSeason);
				}
			} break;

			// No season
			case NO_SEASON_SKU: {
				m_activeSeason = NO_SEASON_SKU;
			} break;

			// Some other season
			default: {
				// Directly retrieve target season def from content
				DefinitionNode activeSeasonDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SEASONS, forcedSeasonSku);

				// If selected season wasn't found, reset cheat
				if(activeSeasonDef == null) {
					CPSeasonSelector.forcedSeasonSku = CPSeasonSelector.DEFAULT_SEASON_SKU;
					RefreshActiveSeason();
				} else {
					m_activeSeason = activeSeasonDef.sku;
				}
			} break;
		}
	}
}