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
	public const string SKU_GOLDEN_EGG = "egg_golden";
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

	private EggReward m_rewardData = new EggReward();	// Only valid after the egg has been collected
	public EggReward rewardData {
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

	// Incubation management
	[SerializeField] private DateTime m_incubationEndTimestamp;
	public DateTime incubationEndTimestamp { get { return m_incubationEndTimestamp; }}
	public DateTime incubationStartTimestamp { get { return incubationEndTimestamp - incubationDuration; }}
	public TimeSpan incubationDuration { get { return new TimeSpan(0, 0, isIncubating ? (int)(def.GetAsFloat("incubationMinutes") * 60f) : 0); }}
	public TimeSpan incubationElapsed { get { return DateTime.UtcNow - incubationStartTimestamp; }}
	public TimeSpan incubationRemaining { get { return incubationEndTimestamp - DateTime.UtcNow; }}
	public float incubationProgress { get { return isIncubating ? Mathf.InverseLerp(0f, (float)incubationDuration.TotalSeconds, (float)incubationElapsed.TotalSeconds) : 0f; }}
	public bool isIncubating { get { return state == Egg.State.INCUBATING; }}

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
				Messenger.Broadcast<Egg>(GameEvents.EGG_INCUBATION_ENDED, this);
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
				float incubationMinutes = def.GetAsFloat("incubationMinutes");
				m_incubationEndTimestamp = DateTime.UtcNow.AddMinutes(incubationMinutes);

				// Dispatch game event
				Messenger.Broadcast<Egg>(GameEvents.EGG_INCUBATION_STARTED, this);

                NotificationsManager.SharedInstance.ScheduleNotification("sku.not.01", LocalizationManager.SharedInstance.Localize("TID_NOTIFICATION_EGG_HATCHED"), "Action", (int)(incubationMinutes*60));


                }
                break;

			// Opening
			case State.OPENING: {
				// If no reward was generated, do it now
				GenerateReward();
			} break;
		}

		// Broadcast game event
		Messenger.Broadcast<Egg, Egg.State, Egg.State>(GameEvents.EGG_STATE_CHANGED, this, oldState, _newState);

		// Save persistence
		// [AOC] A bit of an overkill, try to improve it on the future
		PersistenceManager.Save();
	}

	/// <summary>
	/// Compute the cost in PC to skip the incubation timer (only if an egg is incubating).
	/// </summary>
	/// <returns>The cost in PC of skipping the incubation timer. 0 if no egg is incubating or incubation has finished.</returns>
	public int GetIncubationSkipCostPC() {
		// If egg is not incubating, return 0.
		if(!isIncubating) return 0;

		// Skip is free during the tutorial
		if(!UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.EGG_INCUBATOR)) return 0;

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

	/// <summary>
	/// Generates a reward for this particular egg.
	/// Will be ignored if the egg already has a reward.
	/// </summary>
	public void GenerateReward() {
		// Skip if reward was already generated
		if(m_rewardData.def != null) return; 

		// Generate the reward and init data
		// For golden eggs, reward is always a special pet!
		if(m_def.sku == SKU_GOLDEN_EGG) {
			DefinitionNode rewardDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.EGG_REWARDS, "pet_special");
			m_rewardData.InitFromDef(rewardDef);
		} else {
			m_rewardData.InitFromDef(EggManager.GenerateReward());
		}
	}

	/// <summary>
	/// Hatches the egg and gives the player a newly generated reward.
	/// Only if the egg is in the OPENING state.
	/// </summary>
	public void Collect() {
		// If no reward was generated (shouldn't happen), do it now
		if(m_rewardData.def == null) GenerateReward();

		// Apply the reward!
		switch(m_rewardData.type) {
			case "pet": {
				// Tell the pet collection to add the new pet
				// No problem if the pet is already unlocked ^^
				UsersManager.currentUser.petCollection.UnlockPet(m_rewardData.itemDef.sku);
			} break;
		}

		// Give golden fragment (if any)
		if(m_rewardData.fragments > 0) {
			// Add golden egg fragments
			// Detecting when the golden egg is completed will be controlled by the UI (to better sync animations)
			UsersManager.currentUser.goldenEggFragments += m_rewardData.fragments;
		}

		// Give coins (if any)
		if(m_rewardData.coins > 0) {
			UsersManager.currentUser.AddCoins(m_rewardData.coins);
		}

		// Change state
		ChangeState(State.COLLECTED);

		// Remove it from the inventory (if appliable)
		EggManager.RemoveEggFromInventory(this);

		// If tutorial wasn't completed, do it now
		if(!UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.EGG_INCUBATOR)) {
			UsersManager.currentUser.SetTutorialStepCompleted(TutorialStep.EGG_INCUBATOR, true);
		}

		// Increase collected eggs counter
		UsersManager.currentUser.eggsCollected++;

		// If golden egg, increase total and reset fragments counter
		if(def.sku == SKU_GOLDEN_EGG) {
			UsersManager.currentUser.goldenEggFragments -= EggManager.goldenEggRequiredFragments;	// In case we have extra fragments!
			UsersManager.currentUser.goldenEggsCollected++;
		}

		// Save persistence
		PersistenceManager.Save();

		// Notify game
		Messenger.Broadcast<Egg>(GameEvents.EGG_OPENED, this);
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

		// Reward
		if ( _data.ContainsKey("rewardSku") )
			m_rewardData.InitFromDef(DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.EGG_REWARDS, _data["rewardSku"]));
		else
			m_rewardData.InitFromDef(null);

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

		// Reward
		if(m_rewardData.def != null) 
		{
			data.Add("rewardSku", m_rewardData.def.sku);
		}

		// Incubating timestamp
		data.Add("incubationEndTimestamp", m_incubationEndTimestamp.ToString());

		return data;
	}
}