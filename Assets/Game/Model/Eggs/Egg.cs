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
		COLLECTED	// Egg has been collected
	};

	/// <summary>
	/// Auxiliar class for persistence load/save.
	/// </summary>
	[Serializable]
	public class SaveData {
		public string sku = "";
		public State state = State.INIT;
		public string rewardSku = "";	// [AOC] CHECK!! Probably no need to persist the reward, since it's instantly consumed
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

	// Logic
	private State m_state = State.INIT;
	public State state { 
		get { return m_state; }
	}

	//------------------------------------------------------------------//
	// FACTORY METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Create an egg by its sku.
	/// </summary>
	/// <returns>The new egg. Null if the egg couldn't be created.</returns>
	/// <param name="_eggSku">The sku of the egg in the EGGS definitions category.</param>
	public static Egg CreateBySku(string _eggSku) {
		// Egg can't be created if definitions are not loaded
		Debug.Assert(Definitions.ready, "Definitions not yet loaded!");

		// Find and validate definition
		DefinitionNode eggDef = Definitions.GetDefinition(Definitions.Category.EGGS, _eggSku);

		// Create and return new egg
		return CreateByDef(eggDef);
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
		Debug.Assert(Definitions.ready, "Definitions not yet loaded!");

		// Find an egg definition associated to the given dragon sku
		DefinitionNode eggDef = Definitions.GetDefinitionByVariable(Definitions.Category.EGGS, "dragonSku", _dragonSku);

		// Create and return new egg
		return CreateByDef(eggDef);
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
		Egg newEgg = CreateBySku(_data.sku);

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
	private static Egg CreateByDef(DefinitionNode _def) {
		// Check params
		if(_def == null) return null;

		// Create and return egg
		Egg newEgg = new Egg();
		newEgg.m_def = _def;
		return newEgg;
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
		m_state = _newState;
	}

	/// <summary>
	/// Hatches the egg and gives the player a newly generated reward.
	/// Only if the egg is in the READY state.
	/// </summary>
	public void Collect() {
		// Generate the reward
		m_rewardDef = EggManager.GenerateReward();

		// Apply the reward
		switch(rewardDef.GetAsString("type")) {
			case "suit": {
				// [AOC] TODO!!
			} break;

			case "pet": {
				// [AOC] TODO!!
			} break;

			case "dragon": {
				// [AOC] TODO!!
			} break;
		}

		// Change state
		ChangeState(State.COLLECTED);

		// Notify game
		Messenger.Broadcast<Egg>(GameEvents.EGG_COLLECTED, this);
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
		Debug.Assert(Definitions.ready, "Definitions not yet loaded!");

		// Def
		m_def = Definitions.GetDefinition(Definitions.Category.EGGS, _data.sku);

		// State
		m_state = _data.state;

		// Reward
		m_rewardDef = Definitions.GetDefinition(Definitions.Category.EGG_REWARDS, _data.rewardSku);
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

		// Reward
		if(m_rewardDef != null) {
			data.rewardSku = m_rewardDef.sku;
		}

		return data;
	}
}