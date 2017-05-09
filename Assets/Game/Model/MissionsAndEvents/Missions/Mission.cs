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
	/// Missions shall be grouped by difficulty.
	/// </summary>
	public enum Difficulty {
		EASY,
		MEDIUM,
		HARD,

		COUNT
	}

	/// <summary>
	/// Current state of the mission.
	/// </summary>
	public enum State {
		LOCKED,
		COOLDOWN,
		ACTIVATION_PENDING,	// Special state for when a cooldown is finished in the middle of a game
		ACTIVE,

		COUNT
	}

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Definitions - serialized to be able to debug from the inspector
	[SerializeField] private DefinitionNode m_def = null;
	public DefinitionNode def { get { return m_def; }}

	[SerializeField] private DefinitionNode m_typeDef = null;
	public DefinitionNode typeDef { get { return m_typeDef; }}

	// Data shortcuts
	public Difficulty difficulty { get { return (Difficulty)m_def.GetAsInt("difficulty"); }}

	// Objective
	private MissionObjective m_objective = null;
	public MissionObjective objective { get { return m_objective; }}

	// Economy
	public int rewardCoins { get { return ComputeRewardCoins(); }}
	public int removeCostPC { get { return ComputeRemoveCostPC(); }}
	public int skipCostPC { get { return ComputeSkipCostPC(); }}

	// State
	private State m_state = State.ACTIVE;
	public State state { get { return m_state; }}

	// Cooldown
	private DateTime m_cooldownStartTimestamp = new DateTime();
	public DateTime cooldownStartTimestamp { get { return m_cooldownStartTimestamp; }}
	public TimeSpan cooldownDuration { get { return new TimeSpan(0, MissionManager.GetCooldownPerDifficulty(difficulty), 0); }}
	public TimeSpan cooldownElapsed { get { return DateTime.UtcNow - m_cooldownStartTimestamp; }}
	public TimeSpan cooldownRemaining { get { return cooldownDuration - cooldownElapsed; }}
	public float cooldownProgress { get { return Mathf.InverseLerp(0f, (float)cooldownDuration.TotalSeconds, (float)cooldownElapsed.TotalSeconds); }}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialize this mission using the given definition.
	/// </summary>
	/// <param name="_def">The definition to be used.</param>
	public void InitFromDefinition(DefinitionNode _def) {
		// Store a reference to the definition
		m_def = _def;

		// Get the type definition as well
		m_typeDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.MISSION_TYPES, m_def.GetAsString("typeSku"));

		// Destroy current objective (if any)
		if(m_objective != null) {
			m_objective.Clear();
			m_objective = null;	// GC will take care of it
		}

		// Create and initialize new objective
		m_objective = new MissionObjective(m_def);
		m_objective.OnObjectiveComplete.AddListener(OnObjectiveComplete);
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
	/// Sets the state of the mission. Use carefully - ideally only from MissionManager.
	/// The new state wont be checked (we can go to the same state as we are, all actions will be performed).
	/// </summary>
	/// <param name="_newState">The state to change to.</param>
	public void ChangeState(State _newState) {
		// Actions to perform when leaving a specific state
		switch(m_state) {
			case State.ACTIVE: {
				// Disable objective
				m_objective.enabled = false;
			} break;
		}

		// Actions to perform when entering a specific state
		switch(_newState) {
			case State.COOLDOWN: {
				// Store timestamp
				m_cooldownStartTimestamp = DateTime.UtcNow;
			} break;

			case State.ACTIVE: {
				// Start objective
				m_objective.enabled = true;
			} break;
		}

		// Change state
		State oldState = m_state;
		m_state = _newState;

		// Broadcast messages
		switch(oldState) {
			case State.LOCKED: Messenger.Broadcast<Mission>(GameEvents.MISSION_UNLOCKED, this);	break;
			case State.COOLDOWN: Messenger.Broadcast<Mission>(GameEvents.MISSION_COOLDOWN_FINISHED, this);	break;
		}
		Messenger.Broadcast<Mission, State, State>(GameEvents.MISSION_STATE_CHANGED, this, oldState, _newState);
	}

	//------------------------------------------------------------------//
	// INTERNAL METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Compute the coins rewarded by completing the mission at the current game state.
	/// Reward is computed dynamically based on MissionManager.maxRewardPerDifficulty and a formula
	/// depending on amount of unlocked dragons, etc.
	/// Reward doesn't depend on the type of mission, just its difficulty.
	/// </summary>
	/// <returns>The amount of coins to be given upon completing the mission.</returns>
	private int ComputeRewardCoins() {
		// [AOC] Formula defined in the missionsDragonRelativeMetrics table
		/*int ownedDragons = DragonManager.GetDragonsByLockState(DragonData.LockState.OWNED).Count;
		return (int)((float)MissionManager.maxRewardPerDifficulty[(int)difficulty] * (float)ownedDragons);*/

		// [AOC] As of 17/03/2017, use max owned dragon rather than total amount of owned dragons
		int maxOwnedDragon = DragonManager.biggestOwnedDragon.def.GetAsInt("order");	// 0..N-1
		return (int)((float)MissionManager.maxRewardPerDifficulty[(int)difficulty] * (float)(maxOwnedDragon + 1));
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
		// [AOC] Standard time/PC equivalence
		return GameSettings.ComputePCForTime(cooldownRemaining);
	}

	//------------------------------------------------------------------//
	// PERSISTENCE														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Load state from a persistence object.
	/// </summary>
	/// <param name="_data">The data object loaded from persistence.</param>
	/// <returns>Whether the mission was successfully loaded</returns>
	public bool Load(SimpleJSON.JSONNode _data) {
		// Read values from persistence object
		// [AOC] Protection in case mission sku change
		DefinitionNode missionDef = MissionManager.GetDef(_data["sku"]);
		if(missionDef == null) return false;

		InitFromDefinition(missionDef);

		// Restore state
		m_state = (State)_data["state"].AsInt;

		// Restore objective
		if(m_objective != null) 
		{
			m_objective.tracker.SetValue(_data["currentValue"].AsFloat, false);
			m_objective.enabled = (m_state == State.ACTIVE);
		}

		// Restore cooldown timestamp
		m_cooldownStartTimestamp = DateTime.Parse( _data["cooldownStartTimestamp"], System.Globalization.CultureInfo.InvariantCulture);
		return true;
	}
	
	/// <summary>
	/// Create and return a persistence save data json initialized with the data.
	/// </summary>
	/// <returns>A new data json to be stored to persistence by the PersistenceManager.</returns>
	public SimpleJSON.JSONNode Save() {
		// Create new object, initialize and return it
		SimpleJSON.JSONClass data = new SimpleJSON.JSONClass();
		
		// Mission sku
		if(m_def != null) data.Add("sku", m_def.sku);

		// State
		data.Add("state",((int)m_state).ToString());

		// Objective progress
		if(m_objective != null) data.Add("currentValue", m_objective.currentValue.ToString());

		// Cooldown timestamp
		data.Add("cooldownStartTimestamp", m_cooldownStartTimestamp.ToString(System.Globalization.CultureInfo.InvariantCulture));
		
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