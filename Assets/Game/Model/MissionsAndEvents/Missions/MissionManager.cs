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
/// <summary>
/// Global singleton manager for missions.
/// There will always be one mission of every difficulty active.
/// Has its own asset in the Resources/Singletons folder, all content must be
/// initialized there.
/// </summary>
public class MissionManager : UbiBCN.SingletonMonoBehaviour<MissionManager> {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Exposed Setup
	[Comment("Mission Required Dragons To Unlock")]
	[SerializeField] private int[] m_dragonsToUnlock = new int[(int)Mission.Difficulty.COUNT];
	public static int[] dragonsToUnlock { get { return instance.m_dragonsToUnlock; } }

	[Comment("Mission Cooldowns (minutes)")]
	[SerializeField] private int[] m_cooldownPerDifficulty = new int[(int)Mission.Difficulty.COUNT];	// minutes

	[Comment("Mission Reward Formula")]
	[SerializeField] private int[] m_maxRewardPerDifficulty = new int[(int)Mission.Difficulty.COUNT];
	public static int[] maxRewardPerDifficulty { get { return instance.m_maxRewardPerDifficulty; } }

	[Comment("Remove Mission PC Cost Formula")]
	[SerializeField] private float m_removeMissionPCCoefA = 0.5f;
	public static float removeMissionPCCoefA { get { return instance.m_removeMissionPCCoefA; } }

	[SerializeField] private float m_removeMissionPCCoefB = 1f;
	public static float removeMissionPCCoefB { get { return instance.m_removeMissionPCCoefB; } }

	// Internal
	private UserProfile m_user;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Initialize internal values from content
		List<DefinitionNode> difficultyDefs = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.MISSION_DIFFICULTIES);
		for(int i = 0; i < difficultyDefs.Count; i++) {
			int difficultyIdx = difficultyDefs[i].GetAsInt("index");

			m_dragonsToUnlock[difficultyIdx] = difficultyDefs[i].GetAsInt("dragonsToUnlock");
			m_cooldownPerDifficulty[difficultyIdx] = difficultyDefs[i].GetAsInt("cooldownMinutes");
			m_maxRewardPerDifficulty[difficultyIdx] = difficultyDefs[i].GetAsInt("maxRewardCoins");

			// For now all difficulties share the same coefs
			m_removeMissionPCCoefA = difficultyDefs[i].GetAsFloat("removeMissionPCCoefA");
			m_removeMissionPCCoefB = difficultyDefs[i].GetAsFloat("removeMissionPCCoefB");
		}
	}

	/// <summary>
	/// Scriptable object has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<DragonData>(GameEvents.DRAGON_ACQUIRED, OnDragonAcquired);
	}

	/// <summary>
	/// Scriptable object has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<DragonData>(GameEvents.DRAGON_ACQUIRED, OnDragonAcquired);
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
		bool gaming = InstanceManager.gameSceneController != null;
		if(m_user != null) m_user.userMissions.CheckActivation(!gaming);
	}

	//------------------------------------------------------------------//
	// SINGLETON STATIC GETTERS											//
	//------------------------------------------------------------------//
	/// <summary>
	/// Get a definition of a mission.
	/// </summary>
	/// <returns>The definition of a mission with the given sku. <c>null</c> if not found.</returns>
	/// <param name="_sku">The sku of the wanted definition.</param>
	public static DefinitionNode GetDef(string _sku) {
		return DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.MISSIONS, _sku);
	}

	/// <summary>
	/// Given a mission difficulty, get its definition.
	/// </summary>
	/// <returns>The definition of the requested difficulty.</returns>
	/// <param name="_difficulty">The difficulty whose definition we want.</param>
	public static DefinitionNode GetDifficultyDef(Mission.Difficulty _difficulty) {
		// Int representation of the difficulty should match the "index" field of the definition
		return DefinitionsManager.SharedInstance.GetDefinitionByVariable(DefinitionsCategory.MISSION_DIFFICULTIES, "index", ((int)(_difficulty)).ToString());
	}

	/// <summary>
	/// Gets the cooldown per difficulty in minutes
	/// </summary>
	/// <returns>The cooldown per difficulty.</returns>
	public static int GetCooldownPerDifficulty(Mission.Difficulty _difficulty) {
		// No cooldown during PlayTest
		if(DebugSettings.isPlayTest) {
			return 0;
		} else {
			return instance.m_cooldownPerDifficulty[(int)_difficulty];
		}
	}

	public static int GetDragonsRequiredToUnlickMissionDifficulty(Mission.Difficulty _difficulty) {
		return instance.m_dragonsToUnlock[(int)_difficulty];
	}

	/// <summary>
	/// Get a reference to the active mission with the given difficulty.
	/// If there is no mission at the requested difficulty, a new one will be generated.
	/// </summary>
	/// <returns>The mission with the given difficulty.</returns>
	/// <param name="_difficulty">The difficulty of the mission to be returned.</param>
	public static Mission GetMission(Mission.Difficulty _difficulty) {
		return instance.m_user.userMissions.GetMission(_difficulty);
	}

	//------------------------------------------------------------------//
	// PUBLIC SINGLETON METHODS											//
	//------------------------------------------------------------------//
	/// <summary>
	/// Process active missions:
	/// Give rewards for those completed and replace them by newly generated missions.
	/// </summary>
	public static void ProcessMissions() {
		// Check all missions
		int coins = instance.m_user.userMissions.ProcessMissions();
		instance.m_user.AddCoins(coins);
	}

	/// <summary>
	/// Removes the mission at the given difficulty slot and replaces it by a new
	/// one of equivalent difficulty.
	/// Doesn't perform any currency transaction, they must be done prior to calling
	/// this method using the Mission.removeCostPC property.
	/// </summary>
	/// <param name="_difficulty">The difficulty of the mission to be removed.</param>
	public static void RemoveMission(Mission.Difficulty _difficulty) {
		instance.m_user.userMissions.RemoveMission(_difficulty);

		// Dispatch global event
		Messenger.Broadcast<Mission>(GameEvents.MISSION_REMOVED, GetMission(_difficulty));
	}

	/// <summary>
	/// Skip the cooldown of the mission at the given difficulty slot.
	/// The mission will immediately be set to Active state.
	/// Doesn't perform any currency transaction, they must be done prior to calling
	/// this method using the Mission.skipCostPC property.
	/// Nothing will happen if the mission is not on Cooldown state.
	/// </summary>
	/// <param name="_difficulty">The difficulty of the mission to be skipped.</param>
	public static void SkipMission(Mission.Difficulty _difficulty) {
		instance.m_user.userMissions.SkipMission(_difficulty);
		// Dispatch global event
		Messenger.Broadcast<Mission>(GameEvents.MISSION_SKIPPED, GetMission(_difficulty));
	}

	/// <summary>
	/// Setup current user.
	/// </summary>
	/// <param name="user">User.</param>
	public static void SetupUser(UserProfile user) {
		instance.m_user = user;
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// A new dragon has been acquired.
	/// </summary>
	/// <param name="_dragon">The dragon that has just been acquired.</param>
	private void OnDragonAcquired(DragonData _dragon) {
		int ownedDragons = UsersManager.currentUser.GetNumOwnedDragons();
		UsersManager.currentUser.userMissions.ownedDragons = ownedDragons;
		UsersManager.currentUser.userMissions.UnlockByDragonsNumber();
	}
}