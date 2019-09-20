// MissionManager.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 30/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.


//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
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
public class MissionManager : UbiBCN.SingletonMonoBehaviour<MissionManager>, IBroadcastListener {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

    [System.Serializable]
    private class Data {
        [Comment("Mission Required Dragons To Unlock")]
        public int[] dragonsToUnlock = new int[(int)Mission.Difficulty.COUNT];

        [Comment("Mission Cooldowns (minutes)")]
        public int[] cooldownPerDifficulty = new int[(int)Mission.Difficulty.COUNT];  // minutes

        [Comment("Mission Reward Formula")]
        public int[] maxRewardPerDifficulty = new int[(int)Mission.Difficulty.COUNT];

        [Comment("Remove Mission PC Cost Formula")]
        public float removeMissionPCCoefA = 0.5f;
        public float removeMissionPCCoefB = 1f;
    }


    //------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES											//
    //------------------------------------------------------------------//
    [SerializeField] private Data m_missionData = new Data();
    [SerializeField] private Data m_specialMissionData = new Data();

	// Internal
	private UserProfile m_user;
    public IUserMissions currentModeMissions { 
        get {
            if (m_user != null) {
                switch (SceneController.mode) {
                    case SceneController.Mode.DEFAULT:          return m_user.userMissions; 
                }
            }

            return null;
        }}

    private static float sm_powerUpSCMultiplier = 0f; // Soft currency modifier multiplier
    public static float powerUpSCMultiplier { get { return sm_powerUpSCMultiplier; } }

    private static float sm_powerUpGFMultiplier = 0f; // Soft currency modifier multiplier
    public static float powerUpGFMultiplier { get { return sm_powerUpGFMultiplier; } }

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
        InitFromDefinitions(ref m_missionData, DefinitionsCategory.MISSION_DIFFICULTIES);
        InitFromDefinitions(ref m_specialMissionData, DefinitionsCategory.MISSION_SPECIAL_DIFFICULTIES);
	}

    private void InitFromDefinitions(ref Data _data, string _defCategory) {
        // Initialize internal values from content
        List<DefinitionNode> difficultyDefs = DefinitionsManager.SharedInstance.GetDefinitionsList(_defCategory);
        for (int i = 0; i < difficultyDefs.Count; i++) {
            DefinitionNode definition = difficultyDefs[i];
            int difficultyIdx = definition.GetAsInt("index");

            _data.dragonsToUnlock[difficultyIdx] = definition.GetAsInt("dragonsToUnlock", 0);
            _data.cooldownPerDifficulty[difficultyIdx] = definition.GetAsInt("cooldownMinutes");

            if (definition.Has("maxRewardCoins")) {
                _data.maxRewardPerDifficulty[difficultyIdx] = definition.GetAsInt("maxRewardCoins");
            } else if (definition.Has("maxRewardGoldenFragments")) {
                _data.maxRewardPerDifficulty[difficultyIdx] = definition.GetAsInt("maxRewardGoldenFragments");
            }

            // For now all difficulties share the same coefs
            _data.removeMissionPCCoefA = definition.GetAsFloat("removeMissionPCCoefA");
            _data.removeMissionPCCoefB = definition.GetAsFloat("removeMissionPCCoefB");
        }
    }

	/// <summary>
	/// Scriptable object has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<IDragonData>(MessengerEvents.DRAGON_ACQUIRED, OnDragonAcquired);
        Messenger.AddListener(MessengerEvents.GAME_STARTED, OnGameStarted);
        Broadcaster.AddListener(BroadcastEventType.GAME_ENDED, this);
	}

	/// <summary>
	/// Scriptable object has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<IDragonData>(MessengerEvents.DRAGON_ACQUIRED, OnDragonAcquired);
        Messenger.RemoveListener(MessengerEvents.GAME_STARTED, OnGameStarted);
        Broadcaster.RemoveListener(BroadcastEventType.GAME_ENDED, this);
	}

    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch(eventType)
        {
            case BroadcastEventType.GAME_ENDED:
            {
                OnGameEnded();
            }break;
        }
    }
    
	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
		bool gaming = InstanceManager.gameSceneController != null;
        if (currentModeMissions != null) 
            currentModeMissions.CheckActivation(!gaming);
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
        return GetDifficultyDef(SceneController.mode, _difficulty);
	}

    public static DefinitionNode GetDifficultyDef(SceneController.Mode _mode, Mission.Difficulty _difficulty) {
        switch (_mode) {
            case SceneController.Mode.DEFAULT:          return DefinitionsManager.SharedInstance.GetDefinitionByVariable(DefinitionsCategory.MISSION_DIFFICULTIES, "index", ((int)(_difficulty)).ToString());
        }
        return null;
    }
	
    public static int GetDragonsToUnlock(Mission.Difficulty _difficulty) {
        return GetDragonsToUnlock(SceneController.mode, _difficulty);
    }

    public static int GetDragonsToUnlock(SceneController.Mode _mode, Mission.Difficulty _difficulty) {
        switch (_mode) {
            case SceneController.Mode.DEFAULT:          return instance.m_missionData.dragonsToUnlock[(int)(_difficulty)];
        }
        return 0;
    }

    public static int GetCooldownPerDifficulty(Mission.Difficulty _difficulty) {
        return GetCooldownPerDifficulty(SceneController.mode, _difficulty);
    }

    public static int GetCooldownPerDifficulty(SceneController.Mode _mode, Mission.Difficulty _difficulty) {
        switch (_mode) {
            case SceneController.Mode.DEFAULT: return instance.m_missionData.cooldownPerDifficulty[(int)(_difficulty)];
        }
        return 0;
    }

    public static int GetMaxRewardPerDifficulty(Mission.Difficulty _difficulty) {
        return GetMaxRewardPerDifficulty(SceneController.mode, _difficulty);
    }

    public static int GetMaxRewardPerDifficulty(SceneController.Mode _mode, Mission.Difficulty _difficulty) {
        switch (_mode) {
            case SceneController.Mode.DEFAULT: return instance.m_missionData.maxRewardPerDifficulty[(int)(_difficulty)];
        }
        return 0;
    }

    public static float GetRemoveMissionPCCoefA(Mission.Difficulty _difficulty) {
        return GetRemoveMissionPCCoefA(SceneController.mode, _difficulty);
    }

    public static float GetRemoveMissionPCCoefA(SceneController.Mode _mode, Mission.Difficulty _difficulty) {
        switch (_mode) {
            case SceneController.Mode.DEFAULT: return instance.m_missionData.removeMissionPCCoefA;
        }
        return 0;
    }

    public static float GetRemoveMissionPCCoefB(Mission.Difficulty _difficulty) {
        return GetRemoveMissionPCCoefB(SceneController.mode, _difficulty);
    }

    public static float GetRemoveMissionPCCoefB(SceneController.Mode _mode, Mission.Difficulty _difficulty) {
        switch (_mode) {
            case SceneController.Mode.DEFAULT: return instance.m_missionData.removeMissionPCCoefB;
        }
        return 0;
    }

    /// <summary>
    /// Get a reference to the active mission with the given difficulty.
    /// If there is no mission at the requested difficulty, a new one will be generated.
    /// </summary>
    /// <returns>The mission with the given difficulty.</returns>
    /// <param name="_difficulty">The difficulty of the mission to be returned.</param>
    public static Mission GetMission(Mission.Difficulty _difficulty) {
        if (instance.currentModeMissions == null) return null;

        return instance.currentModeMissions.GetMission(_difficulty);
    }
    
    /// <summary>
    /// Returns if the mission is from the lab
    /// </summary>
    /// <returns><c>true</c>, if special mission, <c>false</c> otherwise.</returns>
    /// <param name="_mission">Mission.</param>
    public static bool IsSpecial(Mission _mission)
    {
        bool ret = instance.m_user.userSpecialMissions.GetMission(_mission.difficulty) == _mission;
        return ret;
    }

	//------------------------------------------------------------------//
	// PUBLIC SINGLETON METHODS											//
    //------------------------------------------------------------------//
    /// <summary>
    /// Setup current user.
    /// </summary>
    /// <param name="user">User.</param>
    public static void SetupUser(UserProfile user) {
        instance.m_user = user;    
    }

	/// <summary>
	/// Process active missions:
	/// Give rewards for those completed and replace them by newly generated missions.
	/// </summary>
	public static void ProcessMissions() {
        if (instance.currentModeMissions == null) return;

		// Check all missions
        instance.currentModeMissions.ProcessMissions();
	}

	/// <summary>
	/// Removes the mission at the given difficulty slot and replaces it by a new
	/// one of equivalent difficulty.
	/// Doesn't perform any currency transaction, they must be done prior to calling
	/// this method using the Mission.removeCostPC property.
	/// </summary>
	/// <param name="_difficulty">The difficulty of the mission to be removed.</param>
	public static void RemoveMission(Mission.Difficulty _difficulty) {
        if (instance.currentModeMissions == null) return;

        instance.currentModeMissions.RemoveMission(_difficulty);

		// Dispatch global event
		Messenger.Broadcast<Mission>(MessengerEvents.MISSION_REMOVED, GetMission(_difficulty));
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
	public static void SkipMission(Mission.Difficulty _difficulty, float _seconds, bool _useAd, bool _useHC) {
		// Nothing to do if not initialized
        if(instance.currentModeMissions == null) return;

		// UserMissions will take care of it
        instance.currentModeMissions.SkipMission(_difficulty, _seconds, _useAd, _useHC);

		// Instantly check if mission has changed state
		bool gaming = InstanceManager.gameSceneController != null;
        instance.currentModeMissions.CheckActivation(!gaming);

		// Dispatch global event
		Messenger.Broadcast<Mission>(MessengerEvents.MISSION_SKIPPED, GetMission(_difficulty));
	}

    // Modifiers
    public static void AddSCMultiplier(float value) {
        sm_powerUpSCMultiplier += value;
        instance.UpdateMissionRewards();
    }

    //------------------------------------------------------------------//
    // INTERNAL METHODS                                                 //
    //------------------------------------------------------------------//

    private void UpdateMissionRewards() {
        if (m_user != null) {
            m_user.userMissions.UpdateRewards();
            m_user.userSpecialMissions.UpdateRewards();
        }
    }

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// A new dragon has been acquired.
	/// </summary>
	/// <param name="_dragon">The dragon that has just been acquired.</param>
	private void OnDragonAcquired(IDragonData _dragon) {
        if (currentModeMissions != null) {
            int ownedDragons = UsersManager.currentUser.GetNumOwnedDragons();
            currentModeMissions.UnlockByDragonsNumber();
        }
	}

    private void OnGameStarted() {
        if (m_user != null) {
            switch (SceneController.mode) {
                case SceneController.Mode.DEFAULT:
                m_user.userMissions.EnableTracker(UsersManager.currentUser.gamesPlayed >= GameSettings.ENABLE_MISSIONS_AT_RUN);
                m_user.userSpecialMissions.EnableTracker(false);
                break;

                case SceneController.Mode.TOURNAMENT:
                m_user.userMissions.EnableTracker(false);
                m_user.userSpecialMissions.EnableTracker(false);
                break;
            }
        }
    }

    private void OnGameEnded() {
        if (m_user != null) {
            m_user.userMissions.EnableTracker(false);
            m_user.userSpecialMissions.EnableTracker(false);
        }
    }
}