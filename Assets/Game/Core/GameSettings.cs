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
public class GameSettings : Singleton<GameSettings> {
	// Add here any global setup variable such as quality, server ip, debug enabled, ...

	[Header("Gameplay")]
	[Tooltip("Name of the dragon instance on the scene")]
	[SerializeField] private string m_playerName = "Player";
	public static string playerName { get { return instance.m_playerName; }}

	[Tooltip("Percentage of maxHealth where to trigger the starving warning")]
	[SerializeField] [Range(0, 1)] private float m_healthWarningThreshold = 0.2f;
	public static float healthWarningThreshold { get { return instance.m_healthWarningThreshold; }}

	[Tooltip("Minimum amount of energy required to boost")]
	[SerializeField] private float m_energyRequiredToBoost = 25f;
	public static float energyRequiredToBoost { get { return instance.m_energyRequiredToBoost; }}
}