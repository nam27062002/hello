﻿// Egg.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 15/02/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Single Egg object.
/// </summary>
[Serializable]
public class Egg {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public enum State {
		INIT,		// Init state
		STORED,		// Egg is in storage, waiting for incubation
		INCUBATING,	// Egg is in the incubator
		READY,		// Egg has finished incubation period and is ready to be collected
		OPENING,	// Egg is being opened
		COLLECTED,	// Egg has been collected
		SHOWROOM	// Egg for display only, state is not relevant
	};

	public struct EggReward {
		public string type;		// Reward type, matches rewardDefinitions "type" property.
		public string value;	// Typically a sku: disguiseSku, petSku, specialDragonSku.
		public long coins;		// Coins to be given instead of the reward. Only if bigger than 0.
	}

	/// <summary>
	/// Auxiliar class for persistence load/save.
	/// </summary>
	[Serializable]
	public class SaveData {
		public string sku = "";
		public State state = State.INIT;
		public string rewardSku = "";	// [AOC] CHECK!! Probably no need to persist the reward, since it's instantly consumed
		public bool isNew = true;
	}

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
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

	//------------------------------------------------------------------//
	// FACTORY METHODS													//
	//------------------------------------------------------------------//
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
	/// Create and initialize an egg associated to a specific dragon.
	/// Will look through the known egg definitions looking for the first one to match
	/// the given dragon.
	/// </summary>
	/// <returns>The new egg. Null if the egg couldn't be created.</returns>
	/// <param name="_dragonSku">The sku of the dragon the new egg should be associated to.</param>
	public static Egg CreateByDragon(string _dragonSku) {
		// Egg can't be created if definitions are not loaded
		Debug.Assert(ContentManager.ready, "Definitions not yet loaded!");

		// Find an egg definition associated to the given dragon sku
		DefinitionNode eggDef = DefinitionsManager.SharedInstance.GetDefinitionByVariable(DefinitionsCategory.EGGS, "dragonSku", _dragonSku);

		// Create and return new egg
		return CreateFromDef(eggDef);
	}

	/// <summary>
	/// Create and initialize an egg with a given persistence data object.
	/// </summary>
	/// <returns>The newly created egg. Null if the egg couldn't be created.</returns>
	/// <param name="_data">The persistence data to be used to initialize the new egg.</param>
	public static Egg CreateFromSaveData(SaveData _data) {
		// Check params
		if(_data == null) return null;

		// Create a new egg using the persistence data sku
		Egg newEgg = CreateFromSku(_data.sku);

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

	/// <summary>
	/// Create an egg using a random definition picked from the EGGS category.
	/// </summary>
	/// <returns>The newly created egg. Null if no definition could be picked.</returns>
	/// <param name="_onlyOwnedDragons">Whether to restrict the random selection to eggs related to owned dragons only.</param>
	public static Egg CreateRandom(bool _onlyOwnedDragons = true) {
		// Use random def getter
		return CreateFromDef(GetRandomDef(_onlyOwnedDragons));
	}

	/// <summary>
	/// Get a random definition picked from the EGGS category.
	/// </summary>
	/// <returns>The randomly selected definition. Null if no definition could be picked.</returns>
	/// <param name="_onlyOwnedDragons">Whether to restrict the random selection to eggs related to owned dragons only.</param>
	public static DefinitionNode GetRandomDef(bool _onlyOwnedDragons = true) {
		// Get all egg definitions
		List<DefinitionNode> eggDefs = new List<DefinitionNode>();
		DefinitionsManager.SharedInstance.GetDefinitions(DefinitionsCategory.EGGS, ref eggDefs);

		// Filter those whose linked dragon is not valid
		// If required, select only those whose dragon is owned
		List<DefinitionNode> selectedDefs = new List<DefinitionNode>(eggDefs.Capacity);
		DragonData dragonData = null;
		for(int i = 0; i < eggDefs.Count; i++) {
			dragonData = DragonManager.GetDragonData(eggDefs[i].Get("dragonSku"));
			if(dragonData == null) continue;
			if(_onlyOwnedDragons && !dragonData.isOwned) continue;
			selectedDefs.Add(eggDefs[i]);
		}

		// Pick a random egg from the definitions set
		if(selectedDefs.Count > 0) {
			return selectedDefs.GetRandomValue();
		}
		return null;
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
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
		// [AOC] TODO!! Perform specific actions when changing state?
		//				Check state changes restrictions.
		State oldState = m_state;
		m_state = _newState;

		// Broadcast game event
		Messenger.Broadcast<Egg, Egg.State, Egg.State>(GameEvents.EGG_STATE_CHANGED, this, oldState, _newState);
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
				// Get a random disguise of the target rarity
				string rarity = rewardDef.sku;
				rarity = rarity.Replace("suit_", "");
				string disguise = Wardrobe.GetRandomDisguise(m_def.GetAsString("dragonSku"), rarity);

				// Initialize reward data based on obtained disguise
				if(disguise.Equals("")) {
					// The target dragon has no disguises (probably missing content)
					m_rewardData.value = "missing";
				} else {
					// We got a disguise!
					m_rewardData.value = disguise;

					// Level up the disguise
					bool leveled = Wardrobe.LevelUpDisguise(disguise);

					// If the disguise is max leveled, give coins instead
					if(!leveled) {
						// Give coins
						m_rewardData.coins = (long)Wardrobe.GetDisguiseValue(disguise);
						UserProfile.AddCoins(m_rewardData.coins);
					}
				}
			} break;

			case "pet": {
				// [AOC] TODO!!
			} break;

			case "dragon": {
				// [AOC] TODO!!
			} break;
		}

		// Save persistence
		PersistenceManager.Save();

		// Change state
		ChangeState(State.COLLECTED);

		// Notify game
		Messenger.Broadcast<Egg>(GameEvents.EGG_COLLECTED, this);
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

	//------------------------------------------------------------------//
	// PERSISTENCE														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Load state from a persistence object.
	/// </summary>
	/// <param name="_data">The data object loaded from persistence.</param>
	public void Load(SaveData _data) {
		// Check requirements
		Debug.Assert(ContentManager.ready, "Definitions not yet loaded!");

		// Def
		m_def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.EGGS, _data.sku);

		// State
		m_state = _data.state;
		m_isNew = _data.isNew;

		// Reward
		m_rewardDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.EGG_REWARDS, _data.rewardSku);
	}

	/// <summary>
	/// Create and return a persistence save data object initialized with the data.
	/// </summary>
	/// <returns>A new data object to be stored to persistence by the PersistenceManager.</returns>
	public SaveData Save() {
		// Create new object, initialize and return it
		SaveData data = new SaveData();

		// Egg sku
		data.sku = m_def.sku;

		// State
		data.state = m_state;
		data.isNew = m_isNew;

		// Reward
		if(m_rewardDef != null) {
			data.rewardSku = m_rewardDef.sku;
		}

		return data;
	}
}