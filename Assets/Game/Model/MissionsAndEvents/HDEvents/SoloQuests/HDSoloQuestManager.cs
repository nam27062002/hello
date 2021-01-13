// HDQuestManager.cs
// Hungry Dragon
// 
// Created by Miguel Ángel Linares on 23/05/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Collections.Generic;
using SimpleJSON;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// This class represents a quest that can be ployed alone, and where the player is the
/// only one contributing to the general score.
/// It works as a regular quest but it has been stripped off all the connections with the server.
/// All the information is stored locally in the user persistence. 
/// </summary>
[Serializable]
public class HDSoloQuestManager : BaseQuestManager {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	// Use a static event id for the solo quests
	public const int EVENT_ID = 1; 
    
    // Manager will try to find this def in the server response. Wont be there as this quest is local.
    // For the sake of consistency only. 
    public const string TYPE_CODE = "soloQuest";
    public const int NUMERIC_TYPE_CODE = 3;

	
    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Default constructor.
    /// </summary>
    public HDSoloQuestManager() : base() {
        m_type = TYPE_CODE;
        m_numericType = NUMERIC_TYPE_CODE;
    }

    
	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//


	/// <summary>
	/// The solo quest has been activated for the first time
	/// </summary>
	public void StartQuest()
	{
		
		// Initialize the quest from content
		InitFromDefinition();
        

		// Did we find any quest in the content?
		if (EventExists())
		{
			// Calculate the ending timestamp
			m_def.m_startTimestamp = GameServerManager.GetEstimatedServerTime();
			m_def.m_endTimestamp = m_def.m_startTimestamp.AddMinutes(m_def.duration);

			// Set the quest state to NOT_JOINED
			m_data.m_state = HDLiveEventData.State.NOT_JOINED;

            // Activate quest
            Activate();
            
			// Anounce the new quest via broadcast
			Messenger.Broadcast<int, HDLiveDataManager.ComunicationErrorCodes>(MessengerEvents.LIVE_EVENT_NEW_DEFINITION, EVENT_ID,
				HDLiveDataManager.ComunicationErrorCodes.NO_ERROR);
  
            // Refresh the game mode, so the live quest (if any) is deactivated
            HDLiveDataManager.instance.SwitchToGameMode();

		}
	}

	/// <summary>
	/// Forces the quest to end without giving any reward. Useful for testing purposes.
	/// </summary>
	public void DestroyQuest()
	{
		m_data.Clean();
		m_data.definition.Clean();

		m_def = m_data.definition as HDLiveQuestDefinition;
        
        // Anounce it via broadcast
        Messenger.Broadcast<int, HDLiveDataManager.ComunicationErrorCodes>(MessengerEvents.LIVE_EVENT_FINISHED, EVENT_ID,
            HDLiveDataManager.ComunicationErrorCodes.NO_ERROR);
	}
	
	public void InitFromDefinition()
	{
		// Find the first solo quest definition active
		List<DefinitionNode> defs =
			DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.SOLO_QUESTS, "enabled",
				"true");

		
		if (defs.Count == 0)
		{
			// Make sure there is valid solo quest definition in the xml
			Debug.LogError("Couldnt find any active soloQuest definition in the content");

			return;
		} else if (defs.Count > 1)
		{

			Debug.LogWarning("There are more than one active definitions for soloQuests. " +
			                 "It should be just one.");
		}
		
		// Use the first definition found
		DefinitionNode def = defs[0];

		m_data = new HDLiveQuestData();
		m_data.m_eventId = EVENT_ID; // Any number higher than zero
		
		m_def = (HDLiveQuestDefinition) m_data.definition;
		m_def.InitFromDefinition(def);
		m_def.m_eventId = m_data.m_eventId;
        
        // Create tracker
        m_tracker = TrackerBase.CreateTracker(m_def.m_goal.m_typeDef.sku, m_def.m_goal.m_params);
		
	}
	

	//------------------------------------------------------------------------//
	// PARENT OVERRIDING                									  //
	//------------------------------------------------------------------------//

	public override bool IsTeasing()
	{
		// Solo Quests dont have a teasing phase
		return false;
	}

    /// <summary>
    /// Whether this event is should be active according to its state
    /// </summary>
    /// <returns></returns>
	public override bool IsRunning()
	{
		// Basically we want to know if we need to show this event in the quest screen
		// or check the live event instead
		
		bool ret = false;
		if (m_data != null)
		{
			ret = m_data.m_state == HDLiveEventData.State.NOT_JOINED || 
			      m_data.m_state == HDLiveEventData.State.JOINED ||
			      m_data.m_state == HDLiveEventData.State.REWARD_AVAILABLE;
		}
		return ret;
	}
	
	public override void OnLiveDataResponse()
	{
		// Nothing
	}
    

	/// <summary>
	/// Make a call to request the rewards
	/// </summary>
	public override void RequestRewards()
	{
		// Calculate reward level according to score
		int newRewardLevel = 0;
		
		// Iterate all rewards until we reach that one not achieved
		foreach  (HDLiveData.Reward reward in m_def.m_rewards)
		{
			if (m_data.m_globalScore < reward.target)
			{
				break;
			}
			newRewardLevel++;
		}

		// Save it to use it later
		m_rewardLevel = newRewardLevel;
		
		// Fake the live event answer, so the quests screen can go on and show the rewards
		Messenger.Broadcast<int,HDLiveDataManager.ComunicationErrorCodes>(MessengerEvents.LIVE_EVENT_REWARDS_RECEIVED, 
			m_data.m_eventId, HDLiveDataManager.ComunicationErrorCodes.NO_ERROR);

	}


	public override void FinishEvent()
	{
		// Just change the state. For solo quests is as simple as this
		m_data.m_state = HDLiveEventData.State.FINALIZED;

		// Notify everyone via broadcast
		Messenger.Broadcast<int,HDLiveDataManager.ComunicationErrorCodes>(MessengerEvents.LIVE_EVENT_FINISHED, m_data.m_eventId, 
			HDLiveDataManager.ComunicationErrorCodes.NO_ERROR);
        
        // Refresh the game mode, so the live quest (if any) is activated
        HDLiveDataManager.instance.SwitchToGameMode();

	}


	/// <summary>
	/// Notify the quest of the contribution made by the player
	/// </summary>
	/// <param name="_runScore"></param>
	/// <param name="_keysMultiplier"></param>
	/// <param name="_spentHC"></param>
	/// <param name="_viewAD"></param>
	public override void Contribute(float _runScore, float _keysMultiplier, bool _spentHC, bool _viewAD)
	{
		Debug.Log("<color=magenta>REGISTER SOLO QUEST SCORE:</color> "+ _runScore);
		
		// Get contribution amount and apply multipliers
		int contribution = (int)_runScore;
		contribution = (int)(_keysMultiplier * contribution);
	
		// Track here!
		HDTrackingManager.EEventMultiplier mult = HDTrackingManager.EEventMultiplier.none;
		if (_keysMultiplier > 1) {
			if (_spentHC) 		mult = HDTrackingManager.EEventMultiplier.hc_payment;
			else if (_viewAD)	mult = HDTrackingManager.EEventMultiplier.ad;
			else 		  		mult = HDTrackingManager.EEventMultiplier.golden_key;
		}
		HDTrackingManager.Instance.Notify_GlobalEventRunDone(m_data.m_eventId, m_def.m_goal.m_type , (int)GetRunScore(), contribution, mult);	

		if (m_data.m_state == HDLiveEventData.State.NOT_JOINED)
		{
			m_data.m_state = HDLiveEventData.State.JOINED;
		}
		
		// Apply contribution to the quest
		m_data.m_globalScore += contribution;

		// Send the QUEST_SCORE_SENT message, otherwise the result screen wont advance to the next step
		Messenger.Broadcast<HDLiveDataManager.ComunicationErrorCodes>(MessengerEvents.QUEST_SCORE_SENT, 
			HDLiveDataManager.ComunicationErrorCodes.NO_ERROR);
		Messenger.Broadcast(MessengerEvents.QUEST_SCORE_UPDATED);
	}

	public override bool ShouldRequestDefinition()
	{
        // Will never be called in solo quest
		return false;
	}

	public override bool RequestDefinition(bool _force = false)
	{
		// Will never be called in solo quest
		return false;
	}

	public override bool IsWaitingForNewDefinition()
	{
        // Will never be called in solo quest
		return false;
	}

	public override void LoadDataFromCache()
	{
		// Nothing. Local events are stored in the persistence.
	}

	/// <summary>
	/// This method determines if the event should be saved in the cache
	/// </summary>
	/// <returns></returns>
	public override bool ShouldSaveData()
	{
		// Nope. Local events are stored in the persistence, not in cache.
		return false;
	}

	//------------------------------------------------------------------------//
	// PERSISTENCE															  //
	//------------------------------------------------------------------------//

	/// <summary>
	/// Constructor from json data.
	/// </summary>
	/// <param name="_data">Data to be parsed.</param>
	public void ParseJson(SimpleJSON.JSONNode _data)
	{
		m_data.Clean();
		m_data.ParseState(_data["data"]);
		
		m_def = m_data.definition as HDLiveQuestDefinition;

		// Load tracker
		m_tracker.Clear();
		if (m_def.m_goal != null && m_def.m_goal.m_typeDef != null)
		{
			m_tracker = TrackerBase.CreateTracker(m_def.m_goal.m_typeDef.sku, m_def.m_goal.m_params);
		}
	}

	/// <summary>
	/// Serialize into json.
	/// </summary>
	/// <returns>The json.</returns>
	public SimpleJSON.JSONClass ToJson()
	{
		SimpleJSON.JSONClass data = new SimpleJSON.JSONClass();

		// Add data
		
		data.Add("data", m_data.ToJson());

		return data;
	}
	
}