// LeaderboardGenerator.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 25/04/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System.IO;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Auxiliar tools class to be able to generate leaderboards.
/// </summary>
public static class LeaderboardGenerator {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Cache names lists
	private static string[] s_firstNames = null;
	private static string[] s_lastNames = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Generate a random player name.
	/// </summary>
	/// <returns>The generated name.</returns>
	public static string GenerateRandomName() {
		// Load names files?
		if(s_firstNames == null || s_lastNames == null) {
			ReloadPools();
		}

		// Generate the name and return
		return s_firstNames.GetRandomValue() + " " + s_lastNames.GetRandomValue();
	}

	/// <summary>
	/// Reload name pools.
	/// </summary>
	public static void ReloadPools() {
		s_firstNames = File.ReadAllLines(StringUtils.SafePath(Application.dataPath + "/HDLiveEventsTest/first_names.txt"));
		s_lastNames = File.ReadAllLines(StringUtils.SafePath(Application.dataPath + "/HDLiveEventsTest/last_names.txt"));
	}
}