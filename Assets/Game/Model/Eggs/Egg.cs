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
	public static readonly string SKU_STANDARD_EGG = "egg_standard";

	public enum State {
		INIT,		// Init state
		STORED,		// Egg is in storage, waiting for incubation
		INCUBATING,	// Egg is in the incubator
		READY,		// Egg has finished incubation period and is ready to be collected
		OPENING,	// Egg is being opened
		COLLECTED,	// Egg reward has been collected
		SHOWROOM	// Egg for display only, state is not relevant
	};

	public struct EggReward {
		public string type;		// Reward type, matches rewardDefinitions "type" property.
		public string value;	// Typically a sku: disguiseSku, petSku
		public long coins;		// Coins to be given instead of the reward. Only if bigger than 0.
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Data
	private DefinitionNode m_def = null;
	public DefinitionNode def {
		get { return m_def; }
	}

	private DefinitionNode m_rewardDef = null;	// Only valid after the egg has been collected
	public DefinitionNode rewardDef {
		get { return m_rewardDef; }
	}

	private EggReward m_rewardData = new EggReward();
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
		if(!UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.EGG_INCUBATOR_SKIP_TIMER)) return 0;

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
	/// Hatches the egg and gives the player a newly generated reward.
	/// Only if the egg is in the OPENING state.
	/// </summary>
	public void Collect() {
		// Generate the reward
		m_rewardDef = EggManager.GenerateReward();

		// Initialize the reward data
		m_rewardData.type = m_rewardDef.GetAsString("type");
		m_rewardData.value = "";
		m_rewardData.coins = 0;

		// Apply the reward
		switch(m_rewardData.type) {
			case "suit": {
				// Pick a random dragon to give the disguise to
				string dragonSku = DefinitionsManager.SharedInstance.GetSkuList(DefinitionsCategory.DRAGONS).GetRandomValue();

				// Get a random disguise of the target rarity
				string rarity = rewardDef.sku;
				rarity = rarity.Replace("suit_", "");
				//string disguise = Wardrobe.GetRandomDisguise(m_def.GetAsString("dragonSku"), rarity);		// [AOC] Deprecated!! Eggs no longer belong to a single dragon
				string disguise = Wardrobe.GetRandomDisguise(dragonSku, rarity);

				// [AOC] TEMP!! While we have no content, if no disguise was found of the given dragon and rarity, try again with dragon_crocodile which has placeholder content for all rarities
				if(disguise.Equals("")) {
					dragonSku = "dragon_crocodile";
					disguise = Wardrobe.GetRandomDisguise(dragonSku, rarity);
				}

				// Initialize reward data based on obtained disguise
				if(disguise.Equals("")) {
					// The target dragon has no disguises (probably missing content)
					m_rewardData.value = "missing";
				} else {
					// We got a disguise!
					m_rewardData.value = disguise;

					// Level up the disguise
					bool leveled = UsersManager.currentUser.wardrobe.LevelUpDisguise(disguise);

					// If the disguise is max leveled, give coins instead
					if(!leveled) {
						// Give coins
						m_rewardData.coins = (long)UsersManager.currentUser.wardrobe.GetDisguiseValue(disguise);
						UsersManager.currentUser.AddCoins(m_rewardData.coins);
					}
				}
			} break;

			case "pet": {
				// [AOC] TODO!!
			} break;
		}

		// Change state
		ChangeState(State.COLLECTED);

		// Remove it from the inventory (if appliable)
		EggManager.RemoveEggFromInventory(this);

		// If tutorial wasn't completed, do it now
		if(!UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.EGG_INCUBATOR_SKIP_TIMER)) {
			UsersManager.currentUser.SetTutorialStepCompleted(TutorialStep.EGG_INCUBATOR_SKIP_TIMER, true);
		}

		// Save persistence
		PersistenceManager.Save();

		// Notify game
		Messenger.Broadcast<Egg>(GameEvents.EGG_OPENED, this);
	}

	/// <summary>
	/// Create an instance of this egg's prefab.
	/// </summary>
	/// <returns>The newly created instance, <c>null</c> if the instance couldn't be created.</returns>
	public EggController CreateView() {
		// Load the prefab for this egg as defined in the definition
		GameObject prefabObj = Resources.Load<GameObject>(def.GetAsString("prefabPath"));
		Debug.Assert(prefabObj != null, "The prefab defined to egg " + def.sku + " couldn't be found");

		// Create a new instance - will automatically be added to the InstanceManager.player property
		GameObject newInstance = GameObject.Instantiate<GameObject>(prefabObj);

		// Access to the EggController component and initialize it with this data
		EggController newEgg = newInstance.GetComponent<EggController>();
		newEgg.eggData = this;

		// Return the newly created instance
		return newEgg;
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

		// Reward
		if ( _data.ContainsKey("rewardSku") )
			m_rewardDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.EGG_REWARDS, _data["rewardSku"]);
		else
			m_rewardDef = null;

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
		if(m_rewardDef != null) 
		{
			data.Add("rewardSku", m_rewardDef.sku);
		}

		// Incubating timestamp
		data.Add("incubationEndTimestamp", m_incubationEndTimestamp.ToString());

		return data;
	}
}