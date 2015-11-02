// GameSettings.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 30/04/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.
//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

/// <summary>
/// Global setup of the game.
/// </summary>
public class GameSettings : SingletonScriptableObject<GameSettings> {
	// Add here any global setup variable such as quality, server ip, debug enabled, ...

	[Separator("Gameplay")]
	[Comment("Name of the dragon instance on the scene")]
	[SerializeField] private string m_playerName = "Player";
	public static string playerName { get { return instance.m_playerName; }}

	[Comment("Percentage of maxHealth where to trigger the starving warning")]
	[SerializeField] [Range(0, 1)] private float m_healthWarningThreshold = 0.2f;
	public static float healthWarningThreshold { get { return instance.m_healthWarningThreshold; }}

	[Comment("Minimum amount of energy required to boost")]
	[SerializeField] private float m_energyRequiredToBoost = 25f;
	public static float energyRequiredToBoost { get { return instance.m_energyRequiredToBoost; }}

	[Separator("Versioning")]
	[SerializeField] private Version m_internalVersion = new Version(0, 1, 0);
	public static Version internalVersion { get { return instance.m_internalVersion; }}

	[SerializeField] private Version m_iOSVersion = new Version(1, 0, 0);
	public static Version iOSVersion { get { return instance.m_iOSVersion; }}

	[SerializeField] private Version m_androidVersion = new Version(1, 0, 0);
	public static Version androidVersion { get { return instance.m_androidVersion; }}
}