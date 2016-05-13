// EggManager.cs
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
/// Global manager of eggs.
/// Has its own asset in the Resources/Singletons folder with all the required parameters.
/// </summary>
public class EggManager : SingletonMonoBehaviour<EggManager> {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public static readonly int INVENTORY_SIZE = 3;

	/// <summary>
	/// Auxiliar class for persistence load/save.
	/// </summary>
	[Serializable]
	public class SaveData {
		public Egg.SaveData[] inventory = new Egg.SaveData[INVENTORY_SIZE];
		public Egg.SaveData incubatingEgg = null;
		public DateTime incubationEndTimestamp = DateTime.UtcNow;
	}

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Settings
	[SerializeField] private ProbabilitySet m_rewardDropRate = new ProbabilitySet();

	// Inventory
	[SerializeField] private Egg[] m_inventory = new Egg[INVENTORY_SIZE];
	public static Egg[] inventory {
		get { return instance.m_inventory; }
	}

	public static bool isInventoryEmpty {
		get {
			for(int i = 0; i < INVENTORY_SIZE; i++) {
				if(inventory[i] != null) return false;	// Filled slot, inventory not empty
			}
			return true;
		}
	}

	public static bool isInventoryFull {
		get {
			for(int i = 0; i < INVENTORY_SIZE; i++) {
				if(inventory[i] == null) return false;	// Empty slot, inventory not full
			}
			return true;
		}
	}

	// Incubator
	[SerializeField] private Egg m_incubatingEgg = null;
	public static Egg incubatingEgg {
		get { return instance.m_incubatingEgg; }
	}

	[SerializeField] private DateTime m_incubationEndTimestamp;
	public static DateTime incubationEndTimestamp { get { return instance.m_incubationEndTimestamp; }}
	public static DateTime incubationStartTimestamp { get { return incubationEndTimestamp - incubationDuration; }}
	public static TimeSpan incubationDuration { get { return new TimeSpan(0, 0, isIncubating ? (int)(incubatingEgg.def.GetAsFloat("incubationMinutes") * 60f) : 0); }}
	public static TimeSpan incubationElapsed { get { return DateTime.UtcNow - incubationStartTimestamp; }}
	public static TimeSpan incubationRemaining { get { return incubationEndTimestamp - DateTime.UtcNow; }}
	public static float incubationProgress { get { return isIncubating ? Mathf.InverseLerp(0f, (float)incubationDuration.TotalSeconds, (float)incubationElapsed.TotalSeconds) : 0f; }}

	public static bool isIncubatorAvailable {
		get { return incubatingEgg == null; }
	}

	public static bool isIncubating {
		get { return incubatingEgg != null && incubatingEgg.state == Egg.State.INCUBATING; }
	}

	public static bool isReadyForCollection {
		// True if we have an egg incubating and timer has finished
		get { return incubatingEgg != null && incubatingEgg.state == Egg.State.READY; }
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	public void Awake() {
		
	}

	/// <summary>
	/// Initialize manager from definitions.
	/// Requires definitions to be loaded into the DefinitionsManager.
	/// </summary>
	public static void InitFromDefinitions() {
		// Check requirements
		Debug.Assert(DefinitionsManager.ready, "Definitions Manager must be ready before invoking this method.");

		// Initialize reward drop rate table based on definitions
		// Perform a double loop since every time we add a new element the probabilities 
		// are readjusted - therefore we need to first add all the elements and then 
		// define the probabilities for each one
		instance.m_rewardDropRate = new ProbabilitySet();
		List<DefinitionNode> rewardDefs = DefinitionsManager.GetDefinitions(DefinitionsCategory.EGG_REWARDS);
		for(int i = 0; i < rewardDefs.Count; i++) {
			instance.m_rewardDropRate.AddElement(rewardDefs[i].sku);
		}
		for(int i = 0; i < rewardDefs.Count; i++) {
			instance.m_rewardDropRate.SetProbability(i, rewardDefs[i].GetAsFloat("droprate"));
		}
	}

	/// <summary>
	/// Object enabled.
	/// </summary>
	public void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<Egg>(GameEvents.EGG_COLLECTED, OnEggCollected);
	}

	/// <summary>
	/// Object disabled.
	/// </summary>
	public void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<Egg>(GameEvents.EGG_COLLECTED, OnEggCollected);
	}

	/// <summary>
	/// Monobehaviour update implementation.
	/// </summary>
	public void Update() {
		// Check for incubator deadline
		if(isIncubating) {
			if(DateTime.UtcNow >= m_incubationEndTimestamp) {
				// Incubation done!
				m_incubatingEgg.ChangeState(Egg.State.READY);

				// Dispatch game event
				Messenger.Broadcast<Egg>(GameEvents.EGG_INCUBATION_ENDED, m_incubatingEgg);
			}
		}
	}

	//------------------------------------------------------------------//
	// PUBLIC UTILS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Add a new egg to the first empty slot in the inventory. 
	/// If the inventory is full, egg won't be added.
	/// </summary>
	/// <returns>The inventory position where the new egg was added. <c>-1</c> if the egg couldn't be added (inventory full).</returns>
	/// <param name="_newEgg">The egg to be added to the inventory.</param>
	public static int AddEggToInventory(Egg _newEgg) {
		for(int i = 0; i < INVENTORY_SIZE; i++) {
			// Is the slot empty?
			if(instance.m_inventory[i] == null) {
				// Yes!! Store egg
				instance.m_inventory[i] = _newEgg;
				_newEgg.ChangeState(Egg.State.STORED);

				// Dispatch game event
				Messenger.Broadcast<Egg, int>(GameEvents.EGG_ADDED_TO_INVENTORY, _newEgg, i);

				// Return slot idnex
				return i;
			}
		}

		// No empty slots were found, egg couldn't be added
		return -1;
	}

	/// <summary>
	/// Puts an egg into the incubator, provided incubator is empty.
	/// Resets incubation timer and sets Egg state to INCUBATING.
	/// If the egg is in the inventory, removes it, freeing its slot.
	/// Nothing will happen if the incubator is full or the target egg is not valid or is already incubating/collected.
	/// </summary>
	/// <returns>Whether the operation was successful or not.</returns>
	/// <param name="_targetEgg">The egg to be moved to the incubator.</param>
	public static bool PutEggToIncubator(Egg _targetEgg) {
		// Prechecks
		if(_targetEgg == null) return false;
		if(!isIncubatorAvailable) return false;
		if(inventory.IndexOf(_targetEgg) < 0) return false;

		// Move egg
		instance.m_incubatingEgg = _targetEgg;

		// If required, empty inventory slot
		int slotIdx = inventory.IndexOf(_targetEgg);
		if(slotIdx >= 0) {
			instance.m_inventory[slotIdx] = null;
		}

		// Reset incubation timer
		float incubationMinutes = _targetEgg.def.GetAsFloat("incubationMinutes");
		instance.m_incubationEndTimestamp = DateTime.UtcNow.AddMinutes(incubationMinutes);

		// Change egg logic state
		_targetEgg.ChangeState(Egg.State.INCUBATING);

		// Dispatch game event
		Messenger.Broadcast<Egg>(GameEvents.EGG_INCUBATION_STARTED, incubatingEgg);

		return true;
	}

	/// <summary>
	/// Compute the cost in PC to skip the incubation timer (only if an egg is incubating).
	/// </summary>
	/// <returns>The cost in PC of skipping the incubation timer. 0 if no egg is incubating or incubation has finished.</returns>
	public static int GetIncubationSkipCostPC() {
		// If no egg is incubating or incubation has finished, return 0.
		if(incubatingEgg == null || isReadyForCollection) return 0;

		// Skip is free during the tutorial
		if(!UserProfile.IsTutorialStepCompleted(TutorialStep.EGG_INCUBATOR_SKIP_TIMER)) return 0;

		// Just use standard time/pc formula
		return GameSettings.ComputePCForTime(incubationRemaining);
	}

	/// <summary>
	/// Skip the incubation timer, provided an egg is incubating and timer hasn't already finished.
	/// </summary>
	/// <returns><c>true</c>, if incubation was skiped, <c>false</c> otherwise.</returns>
	public static bool SkipIncubation() {
		// Skip if there is no egg incubating
		if(!isIncubating) return false;

		// Incubation done!
		instance.m_incubatingEgg.ChangeState(Egg.State.READY);

		// Dispatch game event
		Messenger.Broadcast(GameEvents.EGG_INCUBATION_ENDED, instance.m_incubatingEgg);

		return true;
	}

	/// <summary>
	/// Generate a random reward respecting drop chances.
	/// </summary>
	/// <returns>The definition of the reward to be given, as defined in the EGG_REWARDS definitions category.</returns>
	public static DefinitionNode GenerateReward() {
		// Probabiliy set makes it easy for us!
		string rewardSku = instance.m_rewardDropRate.GetWeightedRandomElement().label;
		return DefinitionsManager.GetDefinition(DefinitionsCategory.EGG_REWARDS, rewardSku);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// An egg has been collected.
	/// </summary>
	/// <param name="_egg">Egg.</param>
	private void OnEggCollected(Egg _egg) {
		// If it's the one in the incubator, clear incubator
		if(_egg == incubatingEgg) {
			// Clear incubator
			m_incubatingEgg = null;

			// Notify game
			Messenger.Broadcast(GameEvents.EGG_INCUBATOR_CLEARED);

			// If tutorial wasn't completed, do it now
			if(!UserProfile.IsTutorialStepCompleted(TutorialStep.EGG_INCUBATOR_SKIP_TIMER)) {
				UserProfile.SetTutorialStepCompleted(TutorialStep.EGG_INCUBATOR_SKIP_TIMER, true);
				PersistenceManager.Save();
			}
		}
	}

	//------------------------------------------------------------------//
	// PERSISTENCE														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Load state from a persistence object.
	/// </summary>
	/// <param name="_data">The data object loaded from persistence.</param>
	public static void Load(SaveData _data) {
		// Inventory
		for(int i = 0; i < INVENTORY_SIZE; i++) {
			// In case INVENTORY_SIZE changes (if persisted is bigger, just ignore remaining data, if lower fill new slots with null)
			if(i < _data.inventory.Length) {
				// Either create new egg, delete egg or update existing egg
				if(inventory[i] == null && _data.inventory[i] != null) {			// Create new egg?
					instance.m_inventory[i] = Egg.CreateFromSaveData(_data.inventory[i]);
				} else if(inventory[i] != null && _data.inventory[i] == null) {	// Delete egg?
					instance.m_inventory[i] = null;
				} else if(inventory[i] != null && _data.inventory[i] != null) {	// Update egg?
					instance.m_inventory[i].Load(_data.inventory[i]);
				}
			} else {
				instance.m_inventory[i] = null;
			}
		}

		// Incubator - same 3 cases
		if(incubatingEgg == null && _data.incubatingEgg != null) {			// Create new egg?
			instance.m_incubatingEgg = Egg.CreateFromSaveData(_data.incubatingEgg);
		} else if(incubatingEgg != null && _data.incubatingEgg == null) {	// Delete egg?
			instance.m_incubatingEgg = null;
		} else if(incubatingEgg != null && _data.incubatingEgg != null) {	// Update egg?
			instance.m_incubatingEgg.Load(_data.incubatingEgg);
		}

		// Incubator timer
		instance.m_incubationEndTimestamp = _data.incubationEndTimestamp;
	}

	/// <summary>
	/// Create and return a persistence save data object initialized with the data.
	/// </summary>
	/// <returns>A new data object to be stored to persistence by the PersistenceManager.</returns>
	public static SaveData Save() {
		// Create new object, initialize and return it
		SaveData data = new SaveData();

		// Inventory
		for(int i = 0; i < INVENTORY_SIZE; i++) {
			if(inventory[i] != null) {
				data.inventory[i] = inventory[i].Save();
			}
		}

		// Incubator
		if(incubatingEgg != null) {
			data.incubatingEgg = incubatingEgg.Save();
		}

		// Incubator timer
		data.incubationEndTimestamp = instance.m_incubationEndTimestamp;

		return data;
	}
}