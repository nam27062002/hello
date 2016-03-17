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

	[Comment("Increase intervals for dragon's health drain.\nTODO!! Must be re-designed and implemented.")]
	[SerializeField] private List<TimeDrain> m_healthDrainIncForTime;
	public static List<TimeDrain> healthDrainIncForTime { get { return instance.m_healthDrainIncForTime; } }

	[Separator("Versioning")]
	[SerializeField] private Version m_internalVersion = new Version(0, 1, 0);
	public static Version internalVersion { get { return instance.m_internalVersion; }}

	[SerializeField] private Version m_iOSVersion = new Version(1, 0, 0);
	public static Version iOSVersion { get { return instance.m_iOSVersion; }}

	[SerializeField] private Version m_androidVersion = new Version(1, 0, 0);
	public static Version androidVersion { get { return instance.m_androidVersion; }}

	[SerializeField] private int m_androidVersionCode = 0;
	public static int androidVersionCode { 
		get { return instance.m_androidVersionCode; }
		set { instance.m_androidVersionCode = value; }
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
		DefinitionNode gameSettingsDef = Definitions.GetDefinition(Definitions.Category.SETTINGS, "gameSettings");
		float timePcCoefA = gameSettingsDef.GetAsFloat("timeToPCCoefA");
		float timePcCoefB = gameSettingsDef.GetAsFloat("timeToPCCoefB");

		// Just apply Hadrian's formula
		double pc = timePcCoefA * _time.TotalMinutes + timePcCoefB;
		pc = Math.Round(pc, MidpointRounding.AwayFromZero);
		return Mathf.Max(1, (int)pc);	// At least 1
	}
}