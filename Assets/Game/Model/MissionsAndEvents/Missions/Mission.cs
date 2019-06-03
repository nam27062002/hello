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
using System.Globalization;

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
	public const float SECONDS_SKIPPED_WITH_AD = 15f * 60f;	// [AOC] MAGIC NUMBER! 15min. Should be on content probably.

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

    public enum RewardBonusType {
        SOFT_CURRENCY = 0,
        GOLDEN_FRAGMENTS,
        HARD_CURRENCY,

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
	private Difficulty m_difficulty = Difficulty.COUNT;
	public Difficulty difficulty {
		get { return m_difficulty; }
		set { m_difficulty = value; }
	}

	// Objective
	private MissionObjective m_objective = null;
	public MissionObjective objective { get { return m_objective; }}

    // Economy
    private Metagame.Reward m_reward;
    private float m_rewardScaleFactor = 1f;
    private float m_removePCFactor = 1f;

    public Metagame.Reward reward { get { return m_reward; } set { m_reward = value; updated = true; }}
	public int removeCostPC { get { return ComputeRemoveCostPC(); }}
	public int skipCostPC { get { return ComputeSkipCostPC(); }}

    public bool updated { get; set; }

	// State
	private State m_state = State.ACTIVE;
	public State state { get { return m_state; }}

	// Cooldown
	private DateTime m_cooldownStartTimestamp = new DateTime();
	public DateTime cooldownStartTimestamp { get { return m_cooldownStartTimestamp; }}
	public TimeSpan cooldownDuration { get { return new TimeSpan(0, MissionManager.GetCooldownPerDifficulty(difficulty), 0); }}
	public TimeSpan cooldownElapsed { get { return GameServerManager.SharedInstance.GetEstimatedServerTime() - m_cooldownStartTimestamp; }}
	public TimeSpan cooldownRemaining { get { return cooldownDuration - cooldownElapsed; }}
	public float cooldownProgress { get { return Mathf.InverseLerp(0f, (float)cooldownDuration.TotalSeconds, (float)cooldownElapsed.TotalSeconds); }}

	// Did player use Ads or HC to skip time?
	private bool m_skipTimeWithAds;
	private bool m_skipTimeWithHC;
	private bool m_cooldownNotified;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialize the mission with the given parameters
	/// </summary>
	/// <param name="_missionDef">Mission definition.</param>
	/// <param name="_targetValue">Target value.</param>
	/// <param name="_singleRun">Is it a single run mission?</param>
    public void InitWithParams(DefinitionNode _missionDef, DefinitionNode _typeDef, long _targetValue, bool _singleRun, float _removePCFactor) {
		// Store definitions
		m_def = _missionDef;
		m_typeDef = _typeDef;

		// Destroy current objective (if any)
		if(m_objective != null) {
			m_objective.Clear();
			m_objective = null;	// GC will take care of it
		}

		// Create and initialize new objective
		m_objective = new MissionObjective(this, m_def, m_typeDef, _targetValue, _singleRun);
		m_objective.OnObjectiveComplete.AddListener(OnObjectiveComplete);

        m_removePCFactor = _removePCFactor;

		m_skipTimeWithAds = false;
		m_skipTimeWithHC = false;
		m_cooldownNotified = false;
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
		m_typeDef = null;

		m_skipTimeWithAds = false;
		m_skipTimeWithHC = false;
		m_cooldownNotified = false;
	}

    public void EnableTracker(bool _enable) {
        if (_enable && m_state == State.ACTIVE) {
            m_objective.enabled = true;
        } else {
            m_objective.enabled = false;
        }
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
				m_cooldownStartTimestamp = GameServerManager.SharedInstance.GetEstimatedServerTime();
			} break;

			case State.ACTIVE: {
			} break;

			case State.ACTIVATION_PENDING: {
					if (!m_cooldownNotified) {
						HDTrackingManager.Instance.Notify_Missions(this, HDTrackingManager.EActionsMission.new_wait);
						m_cooldownNotified = true;
					}
			} break;
		}

		// Change state
		State oldState = m_state;
		m_state = _newState;

		// Broadcast messages
		switch(oldState) {
			case State.LOCKED: Messenger.Broadcast<Mission>(MessengerEvents.MISSION_UNLOCKED, this);	break;
			case State.COOLDOWN: Messenger.Broadcast<Mission>(MessengerEvents.MISSION_COOLDOWN_FINISHED, this);	break;
		}
		Messenger.Broadcast<Mission, State, State>(MessengerEvents.MISSION_STATE_CHANGED, this, oldState, _newState);
	}

	/// <summary>
	/// Skip the cooldown timer a given amount of seconds.
	/// Mission state wont change, even if cooldown is completed.
	/// </summary>
	/// <param name="_seconds">Time to skip. Use -1 for the whole cooldown duration.</param>
	public void SkipCooldownTimer(float _seconds, bool _useAd, bool _useHC) {
		// Nothing to do if mission is not on cooldown
		if(state != Mission.State.COOLDOWN) return;

		// Full cooldown completion?
		if(_seconds < 0) {
			_seconds = (float)cooldownRemaining.TotalSeconds;
		}

		m_skipTimeWithAds = _useAd;
		m_skipTimeWithHC = _useHC;

		// Do it!
		m_cooldownStartTimestamp = m_cooldownStartTimestamp.AddSeconds(-_seconds);	// Simulate that cooldown started earlier than it actually did

		if((GameServerManager.SharedInstance.GetEstimatedServerTime() - m_cooldownStartTimestamp).TotalMinutes >= MissionManager.GetCooldownPerDifficulty(m_difficulty)) {
			if (_useAd || _useHC) {
				if (m_skipTimeWithAds && m_skipTimeWithHC) {
					HDTrackingManager.Instance.Notify_Missions(this, HDTrackingManager.EActionsMission.new_mix);
					m_cooldownNotified = true;
				} else if (m_skipTimeWithAds) {
					HDTrackingManager.Instance.Notify_Missions(this, HDTrackingManager.EActionsMission.new_ad);
					m_cooldownNotified = true;
				} else if (m_skipTimeWithHC) {
					HDTrackingManager.Instance.Notify_Missions(this, HDTrackingManager.EActionsMission.new_pay);
					m_cooldownNotified = true;
				}
			}
		}
	}

	//------------------------------------------------------------------//
	// INTERNAL METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Compute the PC cost of removing this mission (skipping it).
	/// Cost is computed dynamically based on MissionManager coeficients and a formula
	/// depending on amount of unlocked dragons, etc.
	/// </summary>
	/// <returns>The cost of skipping this mission.</returns>
	private int ComputeRemoveCostPC() {
		// [AOC] Formula defined in the missionsDragonRelativeMetrics table		
        float costPC = m_removePCFactor * MissionManager.GetRemoveMissionPCCoefA(m_difficulty) + MissionManager.GetRemoveMissionPCCoefB(m_difficulty);
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
	// DEBUG															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Skip the specified amount of seconds on the cooldown timer.
	/// </summary>
	/// <param name="_seconds">Seconds.</param>
	public void DEBUG_SkipCooldownTimer(float _seconds) {
		// Nothing to do if mission is not on cooldown
		if(state != Mission.State.COOLDOWN) return;

		// Full cooldown completion?
		if(_seconds < 0) {
			_seconds = (float)cooldownRemaining.TotalSeconds;
		}

		// Do it!
		m_cooldownStartTimestamp = m_cooldownStartTimestamp.AddSeconds(-_seconds);	// Simulate that cooldown started earlier than it actually did
	}

	//------------------------------------------------------------------//
	// PERSISTENCE														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Load state from a persistence object.
	/// </summary>
	/// <param name="_data">The data object loaded from persistence.</param>
	/// <returns>Whether the mission was successfully loaded</returns>
    public bool Load(SimpleJSON.JSONNode _data, float _removePCCost) {
		// Read values from persistence object
		// [AOC] Protection in case mission skus change
		DefinitionNode missionDef = MissionManager.GetDef(_data["sku"]);
		if(missionDef == null) return false;

        DefinitionNode typeDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.MISSION_TYPES, missionDef.Get("type"));
        if (typeDef == null) return false;

        // Initialize with sotred data
        InitWithParams(
			missionDef,
            typeDef,
			_data["targetValue"].AsLong, 
			_data["singleRun"].AsBool,
            _removePCCost
		);

		// Restore state
		m_state = (State)_data["state"].AsInt;
		if(m_state == State.ACTIVATION_PENDING) {
			return false;	// Activation pending state should never be persisted! Return error to generate a new mission
		}

		// Restore objective
		if(m_objective != null) {
			m_objective.tracker.InitValue(_data["currentValue"].AsLong);
			m_objective.enabled = false;
		}

		// [AOC] If mission is active but objective is already completed, something went wrong
		//		 Generate a new mission if that's the case.
		//		 We have some weird cases where mission is marked as active but current value is >= than target (specially with ftux missions)
		if(m_state == State.ACTIVE && m_objective.isCompleted) {
			return false;
		}

		// Restore cooldown timestamp
		m_cooldownStartTimestamp = DateTime.Parse(_data["cooldownStartTimestamp"], CultureInfo.InvariantCulture);
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
		data.Add("state", ((int)m_state).ToString(CultureInfo.InvariantCulture));

		// Objective progress
		if(m_objective != null) {
			data.Add("currentValue", m_objective.currentValue.ToString(CultureInfo.InvariantCulture));
			data.Add("targetValue", m_objective.targetValue.ToString(CultureInfo.InvariantCulture));
			data.Add("singleRun", m_objective.singleRun.ToString(CultureInfo.InvariantCulture));
		}

		// Cooldown timestamp
		data.Add("cooldownStartTimestamp", m_cooldownStartTimestamp.ToString(CultureInfo.InvariantCulture));
		
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
		DebugUtils.Log("<color=green>MISSION COMPLETED!</color>\n" + m_def.sku + " | " + m_objective.currentValue + "/" + m_objective.targetValue + " | " + m_difficulty);
		Messenger.Broadcast<Mission>(MessengerEvents.MISSION_COMPLETED, this);
	}
}