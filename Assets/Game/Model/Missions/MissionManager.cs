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
/// Has its own asset in the Resources/Singletons folder, all content must be
/// initialized there.
/// </summary>
[CreateAssetMenu]
public class MissionManager : SingletonMonoBehaviour<MissionManager> {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	// Number of active missions, should be fixed number
	public static readonly int NUM_MISSIONS = 3;

	/// <summary>
	/// Auxiliar serializable class to save/load to persistence.
	/// </summary>
	[Serializable]
	public class SaveData {
		// Only dynamic data is relevant
		public Mission.SaveData[] activeMissions = new Mission.SaveData[NUM_MISSIONS];
		public int generationIdx = 0;
	}

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Content
	// [AOC] TEMP!! Eventually it will be replaced by procedural generation
	[SerializeField] private MissionDefinitions m_missionDefs;
	private int m_generationIdx = 0;	// Pointing to the definition to be generated next

	// Active missions
	// [AOC] Expose it if you want to see current missions content (alternatively switch to debug inspector)
	private Mission[] m_activeMissions = new Mission[NUM_MISSIONS];

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
		return instance.m_missionDefs.GetDef<MissionDef>(_sku);
	}

	/// <summary>
	/// Get a reference to the mission with the given index.
	/// If there is no mission at the requested index, a new one will be generated.
	/// </summary>
	/// <returns>The mission with the given index. <c>null</c> if index not valid.</returns>
	/// <param name="_idx">_idx.</param>
	public static Mission GetMission(int _idx) {
		// Check index
		if(!DebugUtils.Assert(_idx >= 0 && _idx < NUM_MISSIONS, "Index not valid")) return null;

		// If there is no mission at the given index, create one
		if(instance.m_activeMissions[_idx] == null) {
			GenerateNewMission(_idx);
		}

		// Done!
		return instance.m_activeMissions[_idx];
	}

	/// <summary>
	/// Create a new mission at the given index. Mission generation is completely
	/// automated.
	/// If a mission already exists at the given index, it will be immediately terminated.
	/// </summary>
	/// <returns>The newly created mission.</returns>
	/// <param name="_idx">The slot where to create the new mission. <c>null</c> if index not valid.</param>
	public static Mission GenerateNewMission(int _idx) {
		// Check index
		if(!DebugUtils.Assert(_idx >= 0 && _idx < NUM_MISSIONS, "Index not valid")) return null;

		// Terminate any mission at the requested slot
		ClearMission(_idx);

		// Generate new mission
		// [AOC] TODO!! Automated generation
		// 		 For now let's pick a new mission from the content list
		Mission newMission = new Mission();
		newMission.InitFromDefinition(instance.m_missionDefs.GetDef<MissionDef>(instance.m_generationIdx));
		instance.m_activeMissions[_idx] = newMission;

		// Increase generation index - loop if last mission is reached
		instance.m_generationIdx = (instance.m_generationIdx + 1) % instance.m_missionDefs.Length;

		// Return new mission
		return instance.m_activeMissions[_idx];
	}

	/// <summary>
	/// Properly delete the mission at the given index.
	/// The mission slot will be left empty, be careful with that!
	/// </summary>
	/// <param name="_idx">_idx.</param>
	public static void ClearMission(int _idx) {
		// If there is already a mission at the requested slot, terminate it
		if(instance.m_activeMissions[_idx] != null) {
			instance.m_activeMissions[_idx].Clear();
			instance.m_activeMissions[_idx] = null;	// GC will take care of it
		}
	}

	/// <summary>
	/// Process active missions:
	/// Give rewards for those completed and replace them by newly generated missions.
	/// </summary>
	public static void ProcessMissions() {
		// Check all missions
		for(int i = 0; i < NUM_MISSIONS; i++) {
			// Is mission completed?
			Mission m = GetMission(i);
			if(m.objective.isCompleted) {
				// Give reward
				UserProfile.AddCoins(m.rewardCoins);

				// Generate new mission
				m = GenerateNewMission(i);

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
		for(int i = 0; i < NUM_MISSIONS; i++) {
			// If there is no data for this mission, generate a new one
			if(i >= _data.activeMissions.Length || _data.activeMissions[i] == null || _data.activeMissions[i].sku == "") {
				GenerateNewMission(i);
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
		for(int i = 0; i < NUM_MISSIONS; i++) {
			data.activeMissions[i] = GetMission(i).Save();
		}
		
		// Generation Index
 		data.generationIdx = instance.m_generationIdx;
		
		return data;
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
}