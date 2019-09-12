// Egg.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 15/02/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Single Egg object.
/// </summary>
[Serializable]
public class Egg {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string SKU_STANDARD_EGG = "egg_standard";
	public const string SKU_PREMIUM_EGG = "egg_premium";

	public const string PREFAB_PATH = "UI/Metagame/Eggs/";

	// Respect indices for the animation controller!!
	public enum State {
		INIT = 0,				// 0 Init state
		STORED,					// 1 Egg is in storage, waiting to be moved to the incubation slot
		READY_FOR_INCUBATION,	// 2 Egg is in the incubation slot
		INCUBATING,				// 3 Egg is incubating
		READY,					// 4 Egg has finished incubation period and is ready to be collected
		OPENING,				// 5 Egg is being opened
		COLLECTED,				// 6 Egg reward has been collected
		SHOWROOM				// 7 Egg for display only, state is not relevant
	};

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Data
	private DefinitionNode m_def = null;
	public DefinitionNode def {
		get { return m_def; }
	}

	private Metagame.RewardEgg m_rewardData;
	public Metagame.RewardEgg rewardData {
		get { return m_rewardData; }
	}

	// Logic
	private State m_state = State.INIT;
	public State state { 
		get { return m_state; }
	}

	// Notification purposes
	private bool m_isNew = true;
	public bool isNew {
		get { return m_isNew; }
		set { m_isNew = value; }
	}

	// Debug Testing
	private bool m_testMode = false;
	public bool testMode {
		get { return m_testMode; }
		set { m_testMode = value; }
	}

	// Incubation management
	[SerializeField] private DateTime m_incubationEndTimestamp;
	public DateTime incubationEndTimestamp { get { return m_incubationEndTimestamp; }}
	public DateTime incubationStartTimestamp { get { return incubationEndTimestamp - incubationDuration; }}
	public TimeSpan incubationElapsed { get { return GameServerManager.SharedInstance.GetEstimatedServerTime() - incubationStartTimestamp; }}
	public TimeSpan incubationRemaining { get { return incubationEndTimestamp - GameServerManager.SharedInstance.GetEstimatedServerTime(); }}
	public float incubationProgress { get { return isIncubating ? Mathf.InverseLerp(0f, (float)incubationDuration.TotalSeconds, (float)incubationElapsed.TotalSeconds) : 0f; }}
	public bool isIncubating { get { return state == Egg.State.INCUBATING; }}

    //
    protected EggStateChanged m_eggStateChanged = new EggStateChanged();

	public TimeSpan incubationDuration { 
		get {
			// Cheat support!
			int seconds = 0;
			if(isIncubating) {
				switch(CPGachaTest.incubationTime) {
					case CPGachaTest.IncubationTime.DEFAULT: 		seconds = (int)(def.GetAsFloat("incubationMinutes") * 60f);	break;
					case CPGachaTest.IncubationTime.SECONDS_10: 	seconds = 10; 	break;
					case CPGachaTest.IncubationTime.SECONDS_30: 	seconds = 30; 	break;
					case CPGachaTest.IncubationTime.SECONDS_60: 	seconds = 60; 	break;
				}
			}
			return new TimeSpan(0, 0, isIncubating ? seconds : 0); 
		}
	}

	//------------------------------------------------------------------------//
	// FACTORY METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Create an egg by its sku.
	/// </summary>
	/// <returns>The new egg. Null if the egg couldn't be created.</returns>
	/// <param name="_eggSku">The sku of the egg in the EGGS definitions category.</param>
	public static Egg CreateFromSku(string _eggSku) {
		// Egg can't be created if definitions are not loaded
		Debug.Assert(ContentManager.ready, "Definitions not yet loaded!");

		// Find and validate definition
		DefinitionNode eggDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.EGGS, _eggSku);

		// Create and return new egg
		return CreateFromDef(eggDef);
	}

	/// <summary>
	/// Create and initialize an egg with a given persistence json.
	/// </summary>
	/// <returns>The newly created egg. Null if the egg couldn't be created.</returns>
	/// <param name="_data">The json data to be used to initialize the new egg.</param>
	public static Egg CreateFromSaveData(SimpleJSON.JSONNode _data) {
		// Check params
		if(_data == null) return null;

		// Create a new egg using the persistence data sku
		Egg newEgg = CreateFromSku(_data["sku"]);

		// Load persistence object into the new egg and return it
		if(newEgg != null) {
			newEgg.Load(_data);
		}
		return newEgg;
	}

	/// <summary>
	/// Create an egg using a given definition from the EGGS category.
	/// </summary>
	/// <returns>The newly created egg. Null if definition was not valid.</returns>
	/// <param name="_def">The definition to be used.</param>
	public static Egg CreateFromDef(DefinitionNode _def) {
		// Check params
		if(_def == null) return null;

		// Create and return egg
		Egg newEgg = new Egg();
		newEgg.m_def = _def;
		return newEgg;
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// Private, use factory methods to create new eggs.
	/// </summary>
	private Egg() {
        m_eggStateChanged.egg = this;
	}

	/// <summary>
	/// Change the egg's state.
	/// Should only be called from the EggManager.
	/// </summary>
	/// <param name="_newState">The state to go to.</param>
	public void ChangeState(State _newState) {
		// [AOC] TODO!! Check state changes restrictions.
		// Perform actions before leaving a state
		switch(m_state) {
			// Incubating
			case State.INCUBATING: {
				// Dispatch game event
				Messenger.Broadcast<Egg>(MessengerEvents.EGG_INCUBATION_ENDED, this);
			} break;
		}

		// If leaving a state other than INIT, clear "isNew" flag (probably the user has performed an action on the egg, so it's no longer new!)
		if(m_state != State.INIT) {
			isNew = false;
		}

		// Change state
		State oldState = m_state;
		m_state = _newState;

		// Perfirn actions upon entering a new state
		switch(m_state) {
			// Incubating
			case State.INCUBATING: {
				// Reset incubation timer
				// Max between this and the reference timer
				long t = GameServerManager.SharedInstance.GetEstimatedServerTimeAsLong();
				if ( t < UsersManager.currentUser.incubationTimeReference)
					t = UsersManager.currentUser.incubationTimeReference;
				DateTime dt = TimeUtils.TimestampToDate( t );
				UsersManager.currentUser.incubationTimeReference = t;

				m_incubationEndTimestamp = dt.Add(incubationDuration);

				// Dispatch game event
				Messenger.Broadcast<Egg>(MessengerEvents.EGG_INCUBATION_STARTED, this);
           	} break;

			// Opening
			case State.OPENING: {
				// If no reward was generated, do it now
				// Save Time as min time for start incubating again!
				long t = GameServerManager.SharedInstance.GetEstimatedServerTimeAsLong();
				if ( t < UsersManager.currentUser.incubationTimeReference)
					t = UsersManager.currentUser.incubationTimeReference;

				UsersManager.currentUser.incubationTimeReference = t;

				GenerateReward();
			} break;
			case State.READY:{
			}break;
		}

        // Broadcast game event
        m_eggStateChanged.from = oldState;
        m_eggStateChanged.to = _newState;
        Broadcaster.Broadcast(BroadcastEventType.EGG_STATE_CHANGED, m_eggStateChanged);

		// Save persistence
		if ( m_state != State.SHOWROOM )
		{
			// [AOC] A bit of an overkill, try to improve it on the future
			if(!m_testMode) PersistenceFacade.instance.Save_Request();
		}
		
	}

	/// <summary>
	/// Compute the cost in PC to skip the incubation timer (only if an egg is incubating).
	/// </summary>
	/// <returns>The cost in PC of skipping the incubation timer. 0 if no egg is incubating or incubation has finished.</returns>
	public int GetIncubationSkipCostPC() {
		// If egg is not incubating, return 0.
		if(!isIncubating) return 0;

		// Skip is free during the tutorial
		if(!m_testMode) {
			if(!UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.EGG_INCUBATOR)) return 0;
		}

		// Just use standard time/pc formula
		return GameSettings.ComputePCForTime(incubationRemaining);
	}

	/// <summary>
	/// Skip the incubation timer, provided the egg is incubating and timer hasn't already finished.
	/// </summary>
	/// <returns><c>true</c>, if incubation was skiped, <c>false</c> otherwise.</returns>
	public bool SkipIncubation() {
		// Skip if there is no egg incubating
		if(!isIncubating) return false;

		// Incubation done!
		ChangeState(Egg.State.READY);

		return true;
	}

	public void SetReward(Metagame.RewardEgg _reward) {
		m_rewardData = _reward;
	}

	/// <summary>
	/// Generates a reward for this particular egg.
	/// Will be ignored if the egg already has a reward.
	/// </summary>
	public void GenerateReward() {
		if(m_rewardData != null) return;
		m_rewardData = Metagame.Reward.CreateTypeEgg(m_def.sku, "") as Metagame.RewardEgg;
		m_rewardData.egg = this;
	}

	/// <summary>
	/// Hatches the egg and gives the player a newly generated reward.
	/// Only if the egg is in the OPENING state.
	/// </summary>
	public void Collect() {
		m_rewardData.Collect();

		// Change state
		ChangeState(State.COLLECTED);

		if(!testMode) {
			// Remove it from the inventory (if appliable)
			EggManager.RemoveEggFromInventory(this);

			// If it's a standard egg, mark tutorial as completed
			if(def.sku == SKU_STANDARD_EGG) {
				UsersManager.currentUser.SetTutorialStepCompleted(TutorialStep.EGG_INCUBATOR, true);
			}

			// Increase collected eggs counter
			UsersManager.currentUser.eggsCollected++;

	        // Save persistence
	        PersistenceFacade.instance.Save_Request();
		}

        // Notify game
        Messenger.Broadcast<Egg>(MessengerEvents.EGG_OPENED, this);
	}

	//------------------------------------------------------------------------//
	// PERSISTENCE															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Load state from a json object.
	/// </summary>
	/// <param name="_data">The json loaded from persistence.</param>
	public void Load(SimpleJSON.JSONNode _data) {
		// Check requirements
		Debug.Assert(ContentManager.ready, "Definitions not yet loaded!");

		// Def
		m_def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.EGGS, _data["sku"]);

		// State
		m_state = (State)_data["state"].AsInt;
		m_isNew = _data["isNew"].AsBool;

		// Special case: temporal states shouldn't be persisted (only happens in case of crash)
		if(m_state == State.OPENING) {
			m_state = State.READY;
		}

		// Incubating timestamp
		m_incubationEndTimestamp = DateTime.Parse(_data["incubationEndTimestamp"]);
	}

	/// <summary>
	/// Create and return a persistence save data object initialized with the data.
	/// </summary>
	/// <returns>A new data object to be stored to persistence by the PersistenceManager.</returns>
	public SimpleJSON.JSONNode Save() {
		// Create new object, initialize and return it
		SimpleJSON.JSONClass data = new SimpleJSON.JSONClass();

		// Egg sku
		data.Add("sku",m_def.sku);

		// State
		data.Add("state", ((int)m_state).ToString());
		data.Add("isNew",m_isNew.ToString());

		// Incubating timestamp
		data.Add("incubationEndTimestamp", m_incubationEndTimestamp.ToString());

		return data;
	}
}