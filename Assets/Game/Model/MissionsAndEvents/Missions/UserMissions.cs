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
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
public class UserMissions {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//

	// Content
	// [AOC] TEMP!! Eventually it will be replaced by procedural generation
	private int[] m_generationIdx = new int[(int)Mission.Difficulty.COUNT];	// Pointing to the definition to be generated next

	// Active missions
	// [AOC] Expose it if you want to see current missions content (alternatively switch to debug inspector)
	private Mission[] m_missions = new Mission[(int)Mission.Difficulty.COUNT];


	// Necessary info for the user missions to work
	private int m_ownedDragons;
	public int ownedDragons {
		set { m_ownedDragons = value; }
	}
    
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// </summary>
	public void CheckActivation(bool canActivate = true) {
		for(int i = 0; i < m_missions.Length; i++) {
			// Only initialized missions
			if(m_missions[i] == null) continue;

			// Check missions in cooldown to be unlocked
			if(m_missions[i].state == Mission.State.COOLDOWN || m_missions[i].state == Mission.State.ACTIVATION_PENDING) {
				// Has enough time passed for this mission's difficulty?
				if((DateTime.UtcNow - m_missions[i].cooldownStartTimestamp).TotalMinutes >= MissionManager.GetCooldownPerDifficulty((Mission.Difficulty)i)) {
					// Yes!
					// Missions can't be activated during a game, mark them as pending
					// Are we in-game?
					if(!canActivate) {
						if(m_missions[i].state != Mission.State.ACTIVATION_PENDING) {
							m_missions[i].ChangeState(Mission.State.ACTIVATION_PENDING);
						}
					}
					else {
						m_missions[i].ChangeState(Mission.State.ACTIVE);
					}
				}
			}
		}
	}


	/// <summary>
	/// Get a reference to the active mission with the given difficulty.
	/// If there is no mission at the requested difficulty, a new one will be generated.
	/// </summary>
	/// <returns>The mission with the given difficulty.</returns>
	/// <param name="_difficulty">The difficulty of the mission to be returned.</param>
	public Mission GetMission(Mission.Difficulty _difficulty) {
		// If there is no mission at the given difficulty, create one
		if(m_missions[(int)_difficulty] == null) {
			GenerateNewMission(_difficulty);
		}

		// Done!
		return m_missions[(int)_difficulty];
	}

	//------------------------------------------------------------------//
	// PUBLIC SINGLETON METHODS											//
	//------------------------------------------------------------------//
	/// <summary>
	/// Process active missions:
	/// Give rewards for those completed and replace them by newly generated missions.
	/// </summary>
	public int ProcessMissions() {
		int coinsToReward = 0;
		// Check all missions
		for(int i = 0; i < (int)Mission.Difficulty.COUNT; i++) {
			// Is mission completed?
			Mission m = GetMission((Mission.Difficulty)i);
			if(m.state == Mission.State.ACTIVE && m.objective.isCompleted) {
				// Give reward
				coinsToReward += m.rewardCoins;

				// Generate new mission
				m = GenerateNewMission((Mission.Difficulty)i);

				// Put it on cooldown
				m.ChangeState(Mission.State.COOLDOWN);
			}

			// Is mission pending activation?
			else if(m.state == Mission.State.ACTIVATION_PENDING) {
				// Activate mission
				m.ChangeState(Mission.State.ACTIVE);
			}
		}
		return coinsToReward;
	}

	/// <summary>
	/// Removes the mission at the given difficulty slot and replaces it by a new
	/// one of equivalent difficulty.
	/// Doesn't perform any currency transaction, they must be done prior to calling
	/// this method using the Mission.removeCostPC property.
	/// </summary>
	/// <param name="_difficulty">The difficulty of the mission to be removed.</param>
	public void RemoveMission(Mission.Difficulty _difficulty) {
		// Generate new mission does exactly this :D
		GenerateNewMission(_difficulty);
	}

	/// <summary>
	/// Skip the cooldown of the mission at the given difficulty slot.
	/// The mission will immediately be set to Active state.
	/// Doesn't perform any currency transaction, they must be done prior to calling
	/// this method using the Mission.skipCostPC property.
	/// Nothing will happen if the mission is not on Cooldown state.
	/// </summary>
	/// <param name="_difficulty">The difficulty of the mission to be skipped.</param>
	public void SkipMission(Mission.Difficulty _difficulty) {
		// Get mission and check that is in cooldown state
		Mission m = GetMission(_difficulty);
		if(m == null || m.state != Mission.State.COOLDOWN) return;

		// Change mission to Active state
		m.ChangeState(Mission.State.ACTIVE);
	}

	//------------------------------------------------------------------//
	// INTERNAL METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Create a new mission with the given difficulty. Mission generation is completely
	/// automated.
	/// If a mission already exists at the given difficulty slot, it will be immediately terminated.
	/// </summary>
	/// <returns>The newly created mission.</returns>
	/// <param name="_difficulty">The difficulty slot where to create the new mission.</param>
	private Mission GenerateNewMission(Mission.Difficulty _difficulty) {
		// Terminate any mission at the requested slot
		ClearMission(_difficulty);

		// Generate new mission
		// [AOC] TODO!! Automated generation
		// 		 For now let's pick a new mission definition from the content list matching the requested difficulty
		int idx = m_generationIdx[(int)_difficulty];
		bool loopAllowed = true;	// Allow only one loop through all the definitions - just a security check
		DefinitionNode def = null;
		List<DefinitionNode> defsList = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.MISSIONS);	// [AOC] Order is not trustable, but we don't care since this is temporal
		for(;; idx++) {
			// If reached the last definition but still haven't looped, do it now
			// Otherwise it means there are no definitions for the requested difficulty, throw an exception
			if(idx >= defsList.Count) {
				if(loopAllowed) {
					idx = 0;
					loopAllowed = false;
				} else {
					Debug.Assert(false, "There are no mission definitions for the requested difficulty " + _difficulty);
					return null;
				}
			}

			// Is this mission def of the requested difficulty?
			def = defsList[idx];
			if(def != null && def.GetAsInt("difficulty") == (int)_difficulty) {
				// Found! Break the loop
				break;
			}
		}

		// Create the new mission!
		Mission newMission = new Mission();
		newMission.InitFromDefinition(def);
		m_missions[(int)_difficulty] = newMission;

		// Check whether the new mission should be locked or not
		if(m_ownedDragons < MissionManager.GetDragonsRequiredToUnlickMissionDifficulty(_difficulty)) {
			newMission.ChangeState(Mission.State.LOCKED);
		} else {
			newMission.ChangeState(Mission.State.ACTIVE);	// [AOC] Start active by default, cooldown will be afterwards added if required
		}

		// Increase generation index - loop if last mission is reached
		m_generationIdx[(int)_difficulty] = (idx + 1) % defsList.Count;

		// Return new mission
		return m_missions[(int)_difficulty];
	}

	/// <summary>
	/// Properly delete the mission at the given difficulty slot.
	/// The mission slot will be left empty, be careful with that!
	/// </summary>
	/// <param name="_difficulty">The difficulty slot to be cleared.</param>
	private void ClearMission(Mission.Difficulty _difficulty) {
		// If there is already a mission at the requested slot, terminate it
		if(m_missions[(int)_difficulty] != null) {
			m_missions[(int)_difficulty].Clear();
			m_missions[(int)_difficulty] = null;	// GC will take care of it
		}
	}

	/// <summary>
	/// Clears all missions.
	/// </summary>
	public void ClearAllMissions() {
		for(int i = 0; i < (int)Mission.Difficulty.COUNT; i++)
			ClearMission((Mission.Difficulty)i);
	}

	/// <summary>
	/// Unlock missions based on owned dragons.
	/// Deprecated!
	/// </summary>
	public void UnlockByDragonsNumber() {
		for(int i = 0; i < m_missions.Length; i++) {
			// Is the mission locked?
			if(m_missions[i].state == Mission.State.LOCKED) {
				// Do we have enough dragons?
				if(m_ownedDragons >= MissionManager.GetDragonsRequiredToUnlickMissionDifficulty((Mission.Difficulty)i)) {
					m_missions[i].ChangeState(Mission.State.ACTIVE);
				}
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
	public void Load(SimpleJSON.JSONNode _data) {
		
		// Load generation index BEFORE missions, in case new missions have to be generated
		SimpleJSON.JSONArray array = _data["generationIdx"].AsArray;
		m_generationIdx = new int[(int)Mission.Difficulty.COUNT];
		for(int i = 0; i < (int)Mission.Difficulty.COUNT; i++) {
			// If there is no data for this difficulty, put default value
			if(i >= array.Count) {
				m_generationIdx[i] = 0;
			} else {
				m_generationIdx[i] = array[i].AsInt;
			}
		}

		// Load missions
		SimpleJSON.JSONArray activeMissions = _data["activeMissions"].AsArray;
		for(int i = 0; i < (int)Mission.Difficulty.COUNT; i++) {
			// If there is no data for this mission, generate a new one
			if(i >= activeMissions.Count || activeMissions[i] == null || activeMissions[i]["sku"] == "") {
				GenerateNewMission((Mission.Difficulty)i);
			}
			else {
				// If the mission was not created, create an empty one now and load its data from persistence
				if(m_missions[i] == null) {
					m_missions[i] = new Mission();
				}
				
				// Load data into the target mission
				bool success = m_missions[i].Load(activeMissions[i]);

				// If an error ocurred while loading the mission, generate a new one
				if(!success) {
					GenerateNewMission((Mission.Difficulty)i);
				}
			}
		}        
	}

	/// <summary>
	/// Create and return a persistence save data object initialized with the data.
	/// </summary>
	/// <returns>A new data object to be stored to persistence by the PersistenceManager.</returns>
	public SimpleJSON.JSONNode Save() {
		// Create new object, initialize and return it
		SimpleJSON.JSONClass data = new SimpleJSON.JSONClass();
		
		// Missions
		SimpleJSON.JSONArray missions = new SimpleJSON.JSONArray();
		for(int i = 0; i < (int)Mission.Difficulty.COUNT; i++) {
			missions.Add(GetMission((Mission.Difficulty)i).Save());
		}
		data.Add("activeMissions", missions);
        
		// Generation Index
		SimpleJSON.JSONArray idxs = new SimpleJSON.JSONArray();
		for(int i = 0; i < m_generationIdx.Length; i++)
			idxs.Add(m_generationIdx[i].ToString());

		data.Add("generationIdx", idxs);
		
		return data;
	}
}