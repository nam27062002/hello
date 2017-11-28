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

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	private DefinitionNode m_activeSeasonDef = null;
	public static DefinitionNode activeSeasonDef {
		get { return instance.m_activeSeasonDef; }
	}

	public static string activeSeason {
		get { return instance.m_activeSeasonDef == null ? NO_SEASON_SKU : instance.m_activeSeasonDef.sku; }
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
				m_activeSeasonDef = DefinitionsManager.SharedInstance.GetDefinitionByVariable(
					DefinitionsCategory.SEASONS, 
					"active", 
					"true"
				);
			} break;

			// No season
			case NO_SEASON_SKU: {
				m_activeSeasonDef = null;
			} break;

			// Some other season
			default: {
				// Directly retrieve target season def from content
				m_activeSeasonDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SEASONS, forcedSeasonSku);

				// If selected season wasn't found, reset cheat
				if(m_activeSeasonDef == null) {
					CPSeasonSelector.forcedSeasonSku = CPSeasonSelector.DEFAULT_SEASON_SKU;
					RefreshActiveSeason();
				}
			} break;
		}
	}
}