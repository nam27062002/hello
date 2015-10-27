// DebugSettings.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 23/09/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

/// <summary>
/// Global setup of the game.
/// </summary>
public class DebugSettings : SingletonScriptableObject<DebugSettings> {
	// Add here any global debug variable such as invincibility, infinite fire, debug profile...

	[Header("Gameplay")]
	// Invulnerable
	[SerializeField] private bool m_invulnerability = false;
	public static bool invulnerability { get { return instance.m_invulnerability; }}

	[SerializeField] private bool m_inifinteFire = false;
	public static bool infiniteFire { get { return instance.m_inifinteFire; }}
}