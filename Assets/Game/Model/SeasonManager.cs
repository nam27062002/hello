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
		// Load active season from content
		m_activeSeasonDef = DefinitionsManager.SharedInstance.GetDefinitionByVariable(
			DefinitionsCategory.SEASONS, 
			"active", 
			"true"
		);
	}
}