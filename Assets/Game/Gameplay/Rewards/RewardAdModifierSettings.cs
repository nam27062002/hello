// RewardAdModifierSettings.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 31/08/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple class to store all settings related to the end of game reward multiplier
/// by watching an Ad.
/// </summary>
public class RewardAdModifierSettings {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// State
	private bool m_initialized = false;
	public bool isInitialized {
		get { return m_initialized; }
	}

	private bool m_enabled = false;
	private int m_minRuns = 0;
	public bool isEnabled {
		// Consider all conditions
		get { 
			return 
				m_enabled &&												// Feature enabled
				m_minRuns <= UsersManager.currentUser.gamesPlayed &&		// FTUX
				GameSceneController.mode != SceneController.Mode.TOURNAMENT	// Not in tournament either
			;
		}
	}

	// Multipliers
	private float m_spawnersCoinsMultiplier = 1f;
	public float spawnersCoinsMultiplier {
		get { return ChooseValue(m_spawnersCoinsMultiplier, 1f); }
	}

	private float m_survivalBonusCoinsMultiplier = 1f;
	public float survivalBonusCoinsMultiplier {
		get { return ChooseValue(m_survivalBonusCoinsMultiplier, 1f); }
	}

	private float m_watchAdCoinsMultiplier = 1f;
	public float watchAdCoinsMultiplier {
		get { return ChooseValue(m_watchAdCoinsMultiplier, 1f); }
	}

	// Other settings
	private bool m_freeForVip = false;
	public bool freeForVip {
		get { return m_freeForVip; }
	}

	// Data
	private DefinitionNode m_settingsDef = null;
	public DefinitionNode settingsDef {
		get { return m_settingsDef; }
	}
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public RewardAdModifierSettings() {
		// Put default values
		Reset();
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~RewardAdModifierSettings() {

	}

	/// <summary>
	/// Read the content to initialize the settings.
	/// </summary>
	public void InitFromDefinitions() {
		// Reset values to default
		Reset();

		// Get definition
		m_settingsDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SETTINGS, "rewardAdModifierSettings");

		// Initialize values
		m_enabled = m_settingsDef.GetAsBool("enabled", m_enabled);
		m_minRuns = m_settingsDef.GetAsInt("minRuns", m_minRuns);

		m_spawnersCoinsMultiplier = m_settingsDef.GetAsFloat("spawnersCoinsMultiplier", m_spawnersCoinsMultiplier);
		m_survivalBonusCoinsMultiplier = m_settingsDef.GetAsFloat("survivalBonusCoinsMultiplier", m_survivalBonusCoinsMultiplier);
		m_watchAdCoinsMultiplier = m_settingsDef.GetAsFloat("watchAdCoinsMultiplier", m_watchAdCoinsMultiplier);

		m_freeForVip = m_settingsDef.GetAsBool("freeForVip", m_freeForVip);
	}

	/// <summary>
	/// Reset to default values.
	/// </summary>
	public void Reset() {
		// Add here any new value added to the settings!
		m_settingsDef = null;

		m_enabled = false;
		m_minRuns = 0;

		m_spawnersCoinsMultiplier = 1f;
		m_survivalBonusCoinsMultiplier = 1f;
		m_watchAdCoinsMultiplier = 1f;

		m_freeForVip = false;
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Simple aux function to choose a value to return if the feature is enabled 
	/// and a different value if not enabled.
	/// </summary>
	/// <param name="_valueIfEnabled">The value to return if feature is enabled.</param>
	/// <param name="_valueIfNotEnabled">The value to return if feature is not enabled.</param>
	/// <returns>The chosen value.</returns>
	private T ChooseValue<T>(T _valueIfEnabled, T _valueIfNotEnabled) {
		return isEnabled ? _valueIfEnabled : _valueIfNotEnabled;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}