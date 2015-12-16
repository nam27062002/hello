// Mission.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 30/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Single mission object.
/// </summary>
[Serializable]
public class Mission {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Auxiliar serializable class to save/load to persistence.
	/// </summary>
	[Serializable]
	public class SaveData {
		public string sku = "";		// Mission def's sku
		public float currentValue = 0f;	// Objective's current value - only relevant for long-term missions, but save it always anyway
	}

	/// <summary>
	/// Missions shall be grouped by difficulty.
	/// </summary>
	public enum Difficulty {
		EASY,
		MEDIUM,
		HARD,

		COUNT
	}

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	[SerializeField] private MissionDef m_def = null;
	public MissionDef def { get { return m_def; }}

	private MissionObjective m_objective = null;
	public MissionObjective objective { get { return m_objective; }}

	public int rewardCoins { get { return ComputeRewardCoins(); }}
	public int removeCostPC { get { return ComputeRemoveCostPC(); }}
	public int skipCostPC { get { return ComputeSkipCostPC(); }}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialize this mission using the given definition.
	/// </summary>
	/// <param name="_def">The definition to be used.</param>
	public void InitFromDefinition(MissionDef _def) {
		// Store a reference to the definition
		m_def = _def;

		// Destroy current objective (if any)
		if(m_objective != null) {
			m_objective.Clear();
			m_objective = null;	// GC will take care of it
		}

		// Create and initialize new objective
		m_objective = MissionObjective.Create(this);
		m_objective.OnObjectiveComplete += OnObjectiveComplete;
	}

	/// <summary>
	/// Leave the mission ready for garbage collection.
	/// </summary>
	public void Clear() {
		if(m_objective != null) {
			m_objective.Clear();
			m_objective = null;
		}
		
		m_def = null;
	}

	/// <summary>
	/// Compute the coins rewarded by completing the mission at the current game state.
	/// Reward is computed dynamically based on MissionManager.maxRewardPerDifficulty and a formula
	/// depending on amount of unlocked dragons, etc.
	/// Reward doesn't depend on the type of mission, just its difficulty.
	/// </summary>
	/// <returns>The amount of coins to be given upon completing the mission.</returns>
	private int ComputeRewardCoins() {
		// [AOC] Formula defined in the missionsDragonRelativeMetrics table
		int ownedDragons = DragonManager.GetDragonsByLockState(DragonData.LockState.OWNED).Count;
		int totalDragons = DragonManager.GetDragonsByLockState(DragonData.LockState.ANY).Count;
		float multiplier = (1f/(float)totalDragons) * (float)ownedDragons;
		return (int)((float)MissionManager.maxRewardPerDifficulty[(int)def.difficulty] * multiplier);
	}

	/// <summary>
	/// Compute the PC cost of removing this mission (skipping it).
	/// Cost is computed dynamically based on MissionManager coeficients and a formula
	/// depending on amount of unlocked dragons, etc.
	/// </summary>
	/// <returns>The cost of skipping this mission.</returns>
	private int ComputeRemoveCostPC() {
		// [AOC] Formula defined in the missionsDragonRelativeMetrics table
		int ownedDragons = DragonManager.GetDragonsByLockState(DragonData.LockState.OWNED).Count;
		float costPC = (float)ownedDragons * MissionManager.removeMissionPCCoefA + MissionManager.removeMissionPCCoefB;
		return (int)System.Math.Round(costPC, MidpointRounding.AwayFromZero);	// [AOC] Unity's Mathf round methods round to the even number when .5, we want to round to the upper number instead -_-
	}

	/// <summary>
	/// Compute the PC cost of skipping this mission's cooldown timer.
	/// Cost is computed dynamically based purely on remaining time and global
	/// time cost formula.
	/// </summary>
	/// <returns>The cost of skipping this mission.</returns>
	private int ComputeSkipCostPC() {
		// [AOC] TODO!!
		return 0;
	}

	//------------------------------------------------------------------//
	// PERSISTENCE														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Load state from a persistence object.
	/// </summary>
	/// <param name="_data">The data object loaded from persistence.</param>
	public void Load(SaveData _data) {
		// Read values from persistence object
		InitFromDefinition(MissionManager.GetDef(_data.sku));

		// Restore progress
		if(m_objective != null) m_objective.currentValue = _data.currentValue;
	}
	
	/// <summary>
	/// Create and return a persistence save data object initialized with the data.
	/// </summary>
	/// <returns>A new data object to be stored to persistence by the PersistenceManager.</returns>
	public SaveData Save() {
		// Create new object, initialize and return it
		SaveData data = new SaveData();
		
		// Mission sku
		if(m_def != null) data.sku = m_def.sku;

		// Objective progress
		if(m_objective != null) data.currentValue = m_objective.currentValue;
		
		return data;
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// The objective of this mission has been completed.
	/// </summary>
	private void OnObjectiveComplete() {
		// Dispatch global game event
		Messenger.Broadcast<Mission>(GameEvents.MISSION_COMPLETED, this);
	}
}