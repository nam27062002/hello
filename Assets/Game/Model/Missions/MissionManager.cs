// MissionManager.cs
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
/// Global singleton manager for missions.
/// There will always be one mission of every difficulty active.
/// Has its own asset in the Resources/Singletons folder, all content must be
/// initialized there.
/// </summary>
[CreateAssetMenu]
public class MissionManager : SingletonMonoBehaviour<MissionManager> {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Auxiliar serializable class to save/load to persistence.
	/// </summary>
	[Serializable]
	public class SaveData {
		// Only dynamic data is relevant
		public Mission.SaveData[] activeMissions = new Mission.SaveData[(int)Mission.Difficulty.COUNT];
		public int[] generationIdx = new int[(int)Mission.Difficulty.COUNT];
	}

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Content
	// [AOC] TEMP!! Eventually it will be replaced by procedural generation
	[SerializeField] private MissionDefinitions m_missionDefs;
	private int[] m_generationIdx = new int[(int)Mission.Difficulty.COUNT];	// Pointing to the definition to be generated next

	// Active missions
	// [AOC] Expose it if you want to see current missions content (alternatively switch to debug inspector)
	private Mission[] m_activeMissions = new Mission[(int)Mission.Difficulty.COUNT];

	// Delegates
	// Delegate meant for objectives needing an update() call
	public delegate void OnUpdateDelegate();
	public static OnUpdateDelegate OnUpdate = delegate() { };	// Default initialization to avoid null reference when invoking. Add as many listeners as you want to this specific event by using the += syntax
	
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Scriptable object has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
	}

	/// <summary>
	/// Scriptable object has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
		// Propagate to registered listeners
		OnUpdate();
	}

	//------------------------------------------------------------------//
	// SINGLETON STATIC METHODS											//
	//------------------------------------------------------------------//
	/// <summary>
	/// Get a definition of a mission.
	/// </summary>
	/// <returns>The definition of a mission with the given sku. <c>null</c> if not found.</returns>
	/// <param name="_sku">The sku of the wanted definition.</param>
	public static MissionDef GetDef(string _sku) {
		return instance.m_missionDefs.GetDef(_sku);
	}

	/// <summary>
	/// Get a reference to the active mission with the given difficulty.
	/// If there is no mission at the requested difficulty, a new one will be generated.
	/// </summary>
	/// <returns>The mission with the given difficulty.</returns>
	/// <param name="_difficulty">The difficulty of the mission to be returned.</param>
	public static Mission GetMission(Mission.Difficulty _difficulty) {
		// If there is no mission at the given difficulty, create one
		if(instance.m_activeMissions[(int)_difficulty] == null) {
			GenerateNewMission(_difficulty);
		}

		// Done!
		return instance.m_activeMissions[(int)_difficulty];
	}

	/// <summary>
	/// Create a new mission with the given difficulty. Mission generation is completely
	/// automated.
	/// If a mission already exists at the given difficulty slot, it will be immediately terminated.
	/// </summary>
	/// <returns>The newly created mission.</returns>
	/// <param name="_difficulty">The difficulty slot where to create the new mission.</param>
	public static Mission GenerateNewMission(Mission.Difficulty _difficulty) {
		// Terminate any mission at the requested slot
		ClearMission(_difficulty);

		// Generate new mission
		// [AOC] TODO!! Automated generation
		// 		 For now let's pick a new mission definition from the content list matching the requested difficulty
		int idx = instance.m_generationIdx[(int)_difficulty];
		bool loopAllowed = true;	// Allow only one loop through all the definitions - just a security check
		MissionDef def = null;
		for( ; ; idx++) {
			// If reached the last definition but still haven't looped, do it now
			// Otherwise it means there are no definitions for the requested difficulty, throw an exception
			if(idx == instance.m_missionDefs.Length) {
				if(loopAllowed) {
					idx = 0;
					loopAllowed = false;
				} else {
					DebugUtils.Assert(false, "There are no mission definitions for the requested difficulty " + _difficulty);
					return null;
				}
			}

			// Is this mission def of the requested difficulty?
			def = instance.m_missionDefs.GetDef(idx);
			if(def != null && def.difficulty == _difficulty) {
				// Found! Break the loop
				break;
			}
		}

		// Create the new mission!
		Mission newMission = new Mission();
		newMission.InitFromDefinition(def);
		instance.m_activeMissions[(int)_difficulty] = newMission;

		// Increase generation index - loop if last mission is reached
		instance.m_generationIdx[(int)_difficulty] = (idx + 1) % instance.m_missionDefs.Length;

		// Return new mission
		return instance.m_activeMissions[(int)_difficulty];
	}

	/// <summary>
	/// Properly delete the mission at the given difficulty slot.
	/// The mission slot will be left empty, be careful with that!
	/// </summary>
	/// <param name="_difficulty">The difficulty slot to be cleared.</param>
	public static void ClearMission(Mission.Difficulty _difficulty) {
		// If there is already a mission at the requested slot, terminate it
		if(instance.m_activeMissions[(int)_difficulty] != null) {
			instance.m_activeMissions[(int)_difficulty].Clear();
			instance.m_activeMissions[(int)_difficulty] = null;	// GC will take care of it
		}
	}

	/// <summary>
	/// Process active missions:
	/// Give rewards for those completed and replace them by newly generated missions.
	/// </summary>
	public static void ProcessMissions() {
		// Check all missions
		for(int i = 0; i < (int)Mission.Difficulty.COUNT; i++) {
			// Is mission completed?
			Mission m = GetMission((Mission.Difficulty)i);
			if(m.objective.isCompleted) {
				// Give reward
				UserProfile.AddCoins(m.rewardCoins);

				// Generate new mission
				m = GenerateNewMission((Mission.Difficulty)i);

				// [AOC] TODO!! Put it on cooldown
				//m.unlockTimestamp = ;
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
		// Load generation index BEFORE missions, in case new missions have to be generated
		instance.m_generationIdx = _data.generationIdx;

		// Load missions
		for(int i = 0; i < (int)Mission.Difficulty.COUNT; i++) {
			// If there is no data for this mission, generate a new one
			if(i >= _data.activeMissions.Length || _data.activeMissions[i] == null || _data.activeMissions[i].sku == "") {
				GenerateNewMission((Mission.Difficulty)i);
			} else {
				// If the mission was not created, create an empty one now and load its data from persistence
				if(instance.m_activeMissions[i] == null) {
					instance.m_activeMissions[i] = new Mission();
				}
				
				// Load data into the target mission
				instance.m_activeMissions[i].Load(_data.activeMissions[i]);
			}
		}
	}
	
	/// <summary>
	/// Create and return a persistence save data object initialized with the data.
	/// </summary>
	/// <returns>A new data object to be stored to persistence by the PersistenceManager.</returns>
	public static SaveData Save() {
		// Create new object, initialize and return it
		SaveData data = new SaveData();
		
		// Missions
		for(int i = 0; i < (int)Mission.Difficulty.COUNT; i++) {
			data.activeMissions[i] = GetMission((Mission.Difficulty)i).Save();
		}
		
		// Generation Index
 		data.generationIdx = instance.m_generationIdx;
		
		return data;
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
}