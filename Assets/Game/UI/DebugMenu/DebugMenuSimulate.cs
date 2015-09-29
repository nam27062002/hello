// DebugMenuSimulate.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 15/09/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simulate a game in the debug menu.
/// </summary>
public class DebugMenuSimulate : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public static readonly string EVENT_SIMULATION_FINISHED = typeof(DebugMenuSimulate).Name +  "_EVENT_DRAGON_CHANGED";

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Simulation parameters
	[Header("Core simulation parameters")]
	[Tooltip("Minutes, duration of the simulated game")]
	public Range m_gameLength = new Range(1f, 30f);

	[Header("Factorized parameters")]
	[Tooltip("Amount per second, linear")]
	[InfoBox("Setup these parameters with values suitable to tier 0 dragons with no evolution at all.\nAn evolution factor will be applied during the simulation based on current dragon stats.")]
	public float m_xpPerSecond = 0.125f;	// About 60mins to complete the 10 levels of tier 0 dragon

	[Tooltip("Amount per second, linear")]
	public float m_coinsPerSecond = 1f;

	[Tooltip("Amount per second, linear")]
	public float m_pcPerSecond = 0.0016f;	// Roughly 1 every 10mins

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {

	}

	/// <summary>
	/// First update
	/// </summary>
	private void Start() {

	}

	/// <summary>
	/// Simulate a full game taking in account current profile data and selected dragon.
	/// </summary>
	public void SimulateGame() {
		// Compute a random game duration and convert it to seconds
		float duration = m_gameLength.GetRandom() * 60f;

		// Compute a rewards multiplication factor based on current dragon's evolution state
		DragonData currentData = DragonManager.currentDragonData;
		DragonData tier0Data = DragonManager.GetDragonsByTier(DragonTier.TIER_0)[0];	// Should at least be one dragon of tier 0

		// Health factor -> scales with level/xp, that way xp is already computed as a factor
		float factorHealth = currentData.maxHealth/tier0Data.GetMaxHealthAtLevel(0);	// tier0.level0 factor is 1f + current value relative to tier0.level0 value

		// Skills factor
		float factorSkills = 0f;
		for(int i = 0; i < currentData.skills.Length; i++) {
			factorSkills += currentData.skills[i].value/tier0Data.skills[i].GetValueAtLevel(0);	// tier0.level0 factor is 1f + current value relative to tier0.level0 value
		}
		factorSkills /= currentData.skills.Length;

		// Global factor: combination of the 3
		float factor = (factorHealth + factorSkills)/2;

		// Compute final rewards
		float rewardXp = m_xpPerSecond * duration * factor;
		long rewardCoins = (long)(m_coinsPerSecond * duration * factor);
		long rewardPc = (long)(m_pcPerSecond * duration * factor);

		// Give rewards
		currentData.progression.AddXp(rewardXp);
		UserProfile.AddCoins(rewardCoins);
		UserProfile.AddPC(rewardPc);

		// Check for level-ups
		int levelUpCount = currentData.progression.LevelUp();

		// Save persistence
		PersistenceManager.Save();

		// Show summary popup
		DebugSimulationSummaryPopup popup = PopupManager.OpenPopup(DebugSimulationSummaryPopup.PATH).GetComponent<DebugSimulationSummaryPopup>();
		popup.Init(duration, rewardXp, rewardCoins, rewardPc, levelUpCount, factor);

		// Notify game
		Messenger.Broadcast(EVENT_SIMULATION_FINISHED);
	}
}
