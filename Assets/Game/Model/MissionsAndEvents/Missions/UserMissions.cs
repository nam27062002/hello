﻿// MissionManager.cs
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
	// Active missions
	// [AOC] Expose it if you want to see current missions content (alternatively switch to debug inspector)
	private Mission[] m_missions = new Mission[(int)Mission.Difficulty.COUNT];
    
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
				if((GameServerManager.SharedInstance.GetEstimatedServerTime() - m_missions[i].cooldownStartTimestamp).TotalMinutes >= MissionManager.GetCooldownPerDifficulty((Mission.Difficulty)i)) {
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
				HDTrackingManager.Instance.Notify_Missions(m, HDTrackingManager.EActionsMission.done);

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
	/// <param name="_seconds">Time to skip. Use -1 for the whole cooldown duration.</param>
	public void SkipMission(Mission.Difficulty _difficulty, float _seconds, bool _useAd, bool _useHC) {
		// Get mission and check that it is in cooldown state
		Mission m = GetMission(_difficulty);
		if(m == null) return;

		// Let mission handle it
		m.SkipCooldownTimer(_seconds, _useAd, _useHC);
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
	/// <param name="_forceSku">Optional, force a specific mission sku rather than using the procedural engine.</param>
	private Mission GenerateNewMission(Mission.Difficulty _difficulty, string _forceSku = "") {
		Debug.Log("<color=green>GENERATING NEW MISSION " + _difficulty + "</color>");

		// Filter types to prevent repetition
		// Do it before terminating previous mission so we don't repeat the same mission type we just completed/skipped
		List<string> typesToIgnore = new List<string>();
		for(int i = 0; i < m_missions.Length; i++) {
			if(m_missions[i] != null) typesToIgnore.Add(m_missions[i].typeDef.sku);
		}

		// Aux vars
		DefinitionNode selectedTypeDef = null;
		DefinitionNode selectedMissionDef = null;
		float targetValue = 0f;

		// If a mission is forced, skip procedural engine
		if(!string.IsNullOrEmpty(_forceSku)) {
			// Get forced mission def
			selectedMissionDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.MISSIONS, _forceSku);
			selectedTypeDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.MISSION_TYPES, selectedMissionDef.Get("type"));
			Debug.Log("\tSelected Type: <color=yellow>" + selectedTypeDef.sku + "</color>");
			Debug.Log("\tSelected Mission: <color=yellow>" + selectedMissionDef.sku + "</color>");
		} else {
			// Some more aux vars
			List<DefinitionNode> missionDefs = new List<DefinitionNode>();
			List<DefinitionNode> typeDefs = new List<DefinitionNode>();

			// 1. Get available mission types (based on current max dragon tier unlocked and current mission types)
			DragonTier maxTierUnlocked = DragonManager.biggestOwnedDragon.tier;
			typeDefs = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.MISSION_TYPES);
			typeDefs = typeDefs.FindAll(
				(DefinitionNode _def) => { 
					return (_def.GetAsInt("minTier") <= (int)maxTierUnlocked)	// Ignore mission types meant for bigger tiers
						&& (_def.GetAsInt("maxTier") >= (int)maxTierUnlocked)	// Ignore mission types meant for lower tiers
						&& (!typesToIgnore.Contains(_def.sku));							// Prevent repetition
				}
			);
			DebugUtils.Assert(typeDefs.Count > 0, "<color=red>NO VALID MISSION TYPES FOUND!!!!</color>");	// Just in case

			// 2. Select a type based on definitions weights
			// 2.1. Compute total weight
			float totalWeight = 0f;
			List<float> weightsArray = new List<float>(typeDefs.Count);	// Store all weights in an array for optimization (avoid repetaedly calling DefinitionNode.GetAsFloat())
			for(int i = 0; i < typeDefs.Count; i++) {
				weightsArray.Add(typeDefs[i].GetAsFloat("weight"));
				totalWeight += weightsArray[i];
			}

			// 2.2. Select a random value [0..totalWeight]
			// Iterate through elements until the selected value is reached
			// This should match weighted probability distribution
			// Discard the type if it has no valid missions for the current dragon
			int loopCount = 0;	// Failsafe to prevent infinite loops
			while(selectedTypeDef == null && loopCount < 100) {
				loopCount++;
				targetValue = UnityEngine.Random.Range(0f, totalWeight);
				for(int i = 0; i < typeDefs.Count; ++i) {
					targetValue -= weightsArray[i];
					if(targetValue <= 0f) {
						// We reached the target value!
						selectedTypeDef = typeDefs[i];

						// Get all mission definitions matching the selected type
						// Filter out missions based on current max dragon tier unlocked
						missionDefs = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.MISSIONS, "type", selectedTypeDef.sku);
						missionDefs = missionDefs.FindAll(
							(DefinitionNode _def) => { 
								return (_def.GetAsInt("minTier") <= (int)maxTierUnlocked)	// Ignore missions meant for bigger tiers
									&& (_def.GetAsInt("maxTier") >= (int)maxTierUnlocked);	// Ignore missions meant for lower tiers
							}
						);

						// If the selected type has no valid missions, remove it from the candidates list and select a new type
						if(missionDefs.Count == 0) {
							Debug.Log("<color=red>No missions found for type " + selectedTypeDef.sku + ". Choosing a new type.</color>");

							selectedTypeDef = null;
							typeDefs.RemoveAt(i);

							totalWeight -= weightsArray[i];
							weightsArray.RemoveAt(i);
						}
						break;	// Break the type selection loop
					}
				}
			}
			Debug.Log("\tSelected Type: <color=yellow>" + selectedTypeDef.sku + "</color>");

			// 4. Select a random mission based on weight (as we just did with the mission type)
			// 4.1. Compute total weight
			totalWeight = 0f;
			weightsArray.Clear();	// Store all weights in an array for optimization (avoid repetaedly calling DefinitionNode.GetAsFloat())
			for(int i = 0; i < missionDefs.Count; i++) {
				weightsArray.Add(missionDefs[i].GetAsFloat("weight"));
				totalWeight += weightsArray[i];
			}

			// 4.2. Select a random value [0..totalWeight]
			// Iterate through elements until the selected value is reached
			// This should match weighted probability distribution
			targetValue = UnityEngine.Random.Range(0f, totalWeight);
			for(int i = 0; i < missionDefs.Count; i++) {
				targetValue -= weightsArray[i];
				if(targetValue <= 0f) {
					// We reached the target value!
					selectedMissionDef = missionDefs[i];
					break;	// No need to keep looping
				}
			}
			Debug.Log("\tSelected Mission: <color=yellow>" + selectedMissionDef.sku + "</color>");
		}

		// 5. If mission type supports single run mode, choose randomly whether to use it or not
		bool singleRun = false;
		if(selectedTypeDef.GetAsBool("canBeDuringOneRun")) {
			// Single run? 50% chance
			singleRun = UnityEngine.Random.value < 0.3f;	// 30% chance
		}
		Debug.Log("\tSingle run?: <color=yellow>" + singleRun + "</color>");

		// 6. All ready! Generate the mission!
		return GenerateNewMission(_difficulty, selectedMissionDef, selectedTypeDef, DragonManager.biggestOwnedDragon.def.sku, singleRun);
	}

	/// <summary>
	/// Create a new mission with the given parameters.
	/// A new target value will be computed based on algorithm factors.
	/// If a mission already exists at the given difficulty slot, it will be immediately terminated.
	/// </summary>
	/// <returns>The newly created mission.</returns>
	/// <param name="_difficulty">The difficulty slot where to create the new mission.</param>
	/// <param name="_missionDef">The mission to be generated.</param>
	/// <param name="_missionDef">The type of the mission to be generated.</param>
	/// <param name="_dragonModifierSku">The dragon to be used as modifier (biggest owned dragon).</param>
	/// <param name="_singleRun">Single run mission?</param>
	private Mission GenerateNewMission(Mission.Difficulty _difficulty, DefinitionNode _missionDef, DefinitionNode _typeDef, string _dragonModifierSku, bool _singleRun) {
		// 1. Compute target value based on mission min/max range
		float targetValue = 0f;
		targetValue = UnityEngine.Random.Range(
			_missionDef.GetAsFloat("objectiveBaseQuantityMin"),
			_missionDef.GetAsFloat("objectiveBaseQuantityMax")
		);
		Debug.Log("\tTarget Value:  <color=yellow>" + targetValue + "</color> [" + _missionDef.GetAsFloat("objectiveBaseQuantityMin") + ", " + _missionDef.GetAsFloat("objectiveBaseQuantityMax") + "]");

		// 2. Compute and apply modifiers to the target value
		float totalModifier = 0f;

		// 2.1. Dragon modifier - additive
		DefinitionNode dragonModifierDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.MISSION_MODIFIERS, _dragonModifierSku);	// Matching sku
		if(dragonModifierDef != null) {
			totalModifier += dragonModifierDef.GetAsFloat("quantityModifier");
			Debug.Log("\tDragon Modifier " + dragonModifierDef.GetAsFloat("quantityModifier") + "\n\tTotal modifier: " + totalModifier);
		}

		// 2.2. Difficulty modifier - additive
		DefinitionNode difficultyDef = MissionManager.GetDifficultyDef(_difficulty);
		DefinitionNode difficultyModifierDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.MISSION_MODIFIERS, difficultyDef.sku);
		if(difficultyModifierDef != null) {
			totalModifier += difficultyModifierDef.GetAsFloat("quantityModifier");
			Debug.Log("\tDifficulty Modifier " + difficultyModifierDef.GetAsFloat("quantityModifier") + "\n\tTotal modifier: " + totalModifier);
		}

		// 2.3. Single run modifier - multiplicative
		if(_singleRun) {
			DefinitionNode singleRunModifierDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.MISSION_MODIFIERS, "single_run");
			if(singleRunModifierDef != null) {
				totalModifier *= 1f - singleRunModifierDef.GetAsFloat("quantityModifier");
				Debug.Log("\tSingle Run Modifier " + singleRunModifierDef.GetAsFloat("quantityModifier") + "\n\tTotal modifier: " + totalModifier);
			}
		}

		// 2.4. Apply modifier and round final value
		targetValue = Mathf.Round(targetValue * totalModifier);
		Debug.Log("\t<color=lime>Final Target Value: " + targetValue + "</color>");

		// 3. We got everything we need! Create the new mission
		ClearMission(_difficulty);	// Terminate any mission at the requested slot first
		Mission newMission = new Mission();
		newMission.difficulty = _difficulty;
		newMission.InitWithParams(_missionDef, _typeDef, targetValue, _singleRun);
		m_missions[(int)_difficulty] = newMission;

		// Check whether the new mission should be locked or not (deprecated)
		if(UsersManager.currentUser.GetNumOwnedDragons() < MissionManager.GetDragonsRequiredToUnlockMissionDifficulty(_difficulty)) {
			newMission.ChangeState(Mission.State.LOCKED);
		} else {
			newMission.ChangeState(Mission.State.ACTIVE);	// [AOC] Start active by default, cooldown will be afterwards added if required
		}

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
				if(UsersManager.currentUser.GetNumOwnedDragons() >= MissionManager.GetDragonsRequiredToUnlockMissionDifficulty((Mission.Difficulty)i)) {
					m_missions[i].ChangeState(Mission.State.ACTIVE);
				}
			}
		}
	}

	/// <summary>
	/// Create the FTUXP missions and mark tutorial step as completed
	/// </summary>
	private void GenerateTutorialMissions() {
		// One for every difficulty!
		for(int i = 0; i < (int)Mission.Difficulty.COUNT; i++) {
			GenerateNewMission((Mission.Difficulty)i, "ftux" + (i+1).ToString());	// Force sku!
		}

		// Mark tutorial as completed!
		UsersManager.currentUser.SetTutorialStepCompleted(TutorialStep.FIRST_MISSIONS_GENERATED);
	}

	//------------------------------------------------------------------------//
	// DEBUG METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// DEBUG ONLY!
	/// Create a new mission with the given parameters.
	/// A new target value will be computed based on algorithm factors.
	/// If a mission already exists at the given difficulty slot, it will be immediately terminated.
	/// </summary>
	/// <returns>The newly created mission.</returns>
	/// <param name="_difficulty">The difficulty slot where to create the new mission.</param>
	/// <param name="_missionDef">The mission to be generated.</param>
	/// <param name="_missionDef">The type of the mission to be generated.</param>
	/// <param name="_dragonModifierSku">The dragon to be used as modifier (biggest owned dragon).</param>
	/// <param name="_singleRun">Single run mission?</param>
	public Mission DEBUG_GenerateNewMission(Mission.Difficulty _difficulty, DefinitionNode _missionDef, DefinitionNode _typeDef, string _dragonModifierSku, bool _singleRun) {
		Debug.Log("<color=green>GENERATING NEW MISSION (DEBUG) " + _difficulty + "</color>");
		return GenerateNewMission(_difficulty, _missionDef, _typeDef, _dragonModifierSku, _singleRun);
	}

	//------------------------------------------------------------------------//
	// PERSISTENCE															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Load state from a persistence object.
	/// </summary>
	/// <param name="_data">The data object loaded from persistence.</param>
	public void Load(SimpleJSON.JSONNode _data) {
		// Load missions!
		// Override if tutorial step was not completed
		if(!UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.FIRST_MISSIONS_GENERATED)) {
			// Create special missions
			GenerateTutorialMissions();
		} else {
			// Load missions from persistence object
			SimpleJSON.JSONArray activeMissions = _data["activeMissions"].AsArray;
			for(int i = 0; i < (int)Mission.Difficulty.COUNT; i++) {
				// If there is no data for this mission, generate a new one
				if(i >= activeMissions.Count || activeMissions[i] == null || activeMissions[i]["sku"] == "") {
					GenerateNewMission((Mission.Difficulty)i);
				} else {
					// If the mission object was not created, create an empty one now and load its data from persistence
					if(m_missions[i] == null) {
						m_missions[i] = new Mission();
					}

					// Make sure mission has the right difficulty assigned
					m_missions[i].difficulty = (Mission.Difficulty)i;
					
					// Load data into the target mission
					bool success = m_missions[i].Load(activeMissions[i]);

					// If an error ocurred while loading the mission, generate a new one
					if(!success) {
						GenerateNewMission((Mission.Difficulty)i);
					}
				}
			}
		}
	}

	/// <summary>
	/// Create and return a persistence save data object initialized with the data.
	/// </summary>
	/// <returns>A new data object to be stored to persistence by the PersistenceManager.</returns>
	public SimpleJSON.JSONNode Save() {
		// If tutorial step was not completed, generate tutorial missions if not already done!
		if(!UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.FIRST_MISSIONS_GENERATED)) {
			// Create special missions
			GenerateTutorialMissions();
		}

		// Create new object, initialize and return it
		SimpleJSON.JSONClass data = new SimpleJSON.JSONClass();
		
		// Missions
		SimpleJSON.JSONArray missions = new SimpleJSON.JSONArray();
		for(int i = 0; i < (int)Mission.Difficulty.COUNT; i++) {
			missions.Add(GetMission((Mission.Difficulty)i).Save());
		}
		data.Add("activeMissions", missions);
        
		return data;
	}
}