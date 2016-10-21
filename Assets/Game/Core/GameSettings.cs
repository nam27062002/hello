// GameSettings.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 30/04/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public struct TimeDrain {
	public float seconds;
	public float drainIncrement;
};

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Global setup of the game.
/// </summary>
public class GameSettings : SingletonScriptableObject<GameSettings> {

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Add here any global setup variable such as quality, server ip, debug enabled, ...
	[Comment("Name of the dragon instance on the scene")]
	[Separator("Gameplay")]
	[SerializeField] private string m_playerName = "Player";
	public static string playerName { get { return instance.m_playerName; }}

	[Separator("Versioning")]
	[Comment("Used by the development team, QC, etc. to identify each build internally.\nFormat X.Y.Z where:\n    - X: Development Stage [1..4] (1 - Preproduction, 2 - Production, 3 - Soft Launch, 4 - Worldwide Launch)\n    - Y: Sprint Number [1..N]\n    - Z: Build Number [1..N] within the sprint, increased by 1 for each new build")]
	[SerializeField] private Version m_internalVersion = new Version(0, 1, 0);
	public static Version internalVersion { get { return instance.m_internalVersion; }}

	[Comment("Public version number displayed in the stores, used by the players to identify different application updates\nFormat X.Y where:\n    - X: Arbitrary, only changed on major game change (usually never)\n        - 0 during Preproduction/Production\n        - [1..N] at Soft/WW Launch\n    - Y: Increased for every push to the app store [1..N]", 10f)]
	[SerializeField] private string m_publicVersioniOS = "0.1";
	public static string publicVersioniOS { 
		get { return instance.m_publicVersioniOS; }
		set { instance.m_publicVersioniOS = value; }
	}

	[SerializeField] private string m_publicVersionGGP = "0.1";
	public static string publicVersionGGP { 
		get { return instance.m_publicVersionGGP; }
		set { instance.m_publicVersionGGP = value; }
	}

	[SerializeField] private string m_publicVersionAmazon = "0.1";
	public static string publicVersionAmazon { 
		get { return instance.m_publicVersionAmazon; }
		set { instance.m_publicVersionAmazon = value; }
	}

	//------------------------------------------------------------------//
	// SINGLETON STATIC METHODS											//
	//------------------------------------------------------------------//
	/// <summary>
	/// Compute the PC equivalent of a given amount of time.
	/// </summary>
	/// <returns>The amount of PC worth for <paramref name="_time"/> amount of time.</returns>
	/// <param name="_time">Amount of time to be evaluated.</param>
	public static int ComputePCForTime(TimeSpan _time) {
		// Get coeficients from definition
		DefinitionNode gameSettingsDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SETTINGS, "gameSettings");
		float timePcCoefA = gameSettingsDef.GetAsFloat("timeToPCCoefA");
		float timePcCoefB = gameSettingsDef.GetAsFloat("timeToPCCoefB");

		// Just apply Hadrian's formula
		double pc = timePcCoefA * _time.TotalMinutes + timePcCoefB;
		pc = Math.Round(pc, MidpointRounding.AwayFromZero);
		return Mathf.Max(1, (int)pc);	// At least 1
	}
}