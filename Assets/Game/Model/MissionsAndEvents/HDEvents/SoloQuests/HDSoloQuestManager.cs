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

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
[Serializable]
public class HDSoloQuestManager : IBroadcastListener, IQuestManager{
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	// Use a static event id for the solo quests
	public const int EVENT_ID = 1; 

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	TrackerBase m_tracker = new TrackerBase();

	private HDLiveQuestData m_data;
	private HDLiveQuestDefinition m_def;


	private int m_rewardLevel = -1;	// Response to the get_my_rewards request
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public HDSoloQuestManager() {

		m_data = new HDLiveQuestData();
		m_def = m_data.definition as HDLiveQuestDefinition;
		
		// Subscribe to external events
		Messenger.AddListener(MessengerEvents.GAME_STARTED, OnGameStarted);
		Broadcaster.AddListener(BroadcastEventType.GAME_ENDED, this);
        
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~HDSoloQuestManager() {
		m_data = null;
        m_def = null;

	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//


	/// <summary>
	/// The quest has been activated for the first time
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
			
			// Anounce the new quest via broadcast
			Messenger.Broadcast<int, HDLiveDataManager.ComunicationErrorCodes>(MessengerEvents.LIVE_EVENT_NEW_DEFINITION, EVENT_ID,
				HDLiveDataManager.ComunicationErrorCodes.NO_ERROR);
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
		
	}
	
	
	//------------------------------------------------------------------------//
	// EVENTS																  //
	//------------------------------------------------------------------------//

	public void OnGameStarted(){
		if ( m_tracker != null)
		{ 
			// Always track. If not active, contribute wont be called.
			m_tracker.enabled = true;
			m_tracker.InitValue(0);
		}
	}
	public void OnGameEnded(){
		// Save tracker value?
	}
	
	public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
	{
		switch(eventType)
		{
			case BroadcastEventType.GAME_ENDED:
			{
				OnGameEnded();
			}break;
		}	}


	//------------------------------------------------------------------------//
	// IQuestManager INTERFACE												  //
	//------------------------------------------------------------------------//
	
	
	public HDLiveQuestData GetQuestData()
	{
		return m_data;
	}

	public string GetGoalDescription()
	{
		return m_tracker.FormatDescription(m_def.m_goal.m_desc, m_def.m_goal.m_amount);
	}

	public HDLiveQuestDefinition GetQuestDefinition()
	{
		return m_def;
	}

	public long GetRunScore() 
	{
		return m_tracker.currentValue;
	}

	public void UpdateStateFromTimers()
	{
		m_data.UpdateStateFromTimers();
	}

	/// <summary>
	/// Make a call to request the rewards
	/// </summary>
	public void RequestRewards()
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

	/// <summary>
	/// Returns all the rewards achieved in this quest (based on the rewardLevel value)
	/// </summary>
	/// <returns></returns>
	public List<HDLiveData.Reward> GetMyRewards()
	{
		// Create new list
		List<HDLiveData.Reward> rewards = new List<HDLiveData.Reward>();

		// We must have a valid data and definition
		if(m_data != null && m_data.definition != null) {
			// Check reward level
			// In a quest, the reward level tells us in which reward tier have been reached
			// All rewards below it are also given
			HDLiveQuestDefinition def = m_data.definition as HDLiveQuestDefinition;
			for(int i = 0; i < m_rewardLevel; ++i) {	// Reward level is 1-N
				rewards.Add(def.m_rewards[i]);	// Assuming rewards are properly sorted :)
			}
		}

		// Done!
		return rewards;
	}

	public void FinishEvent()
	{
		// Just change the state. For solo quests is as simple as this
		m_data.m_state = HDLiveEventData.State.FINALIZED;

		// Notify everyone via broadcast
		Messenger.Broadcast<int,HDLiveDataManager.ComunicationErrorCodes>(MessengerEvents.LIVE_EVENT_FINISHED, m_data.m_eventId, 
			HDLiveDataManager.ComunicationErrorCodes.NO_ERROR);

	}

	public void ClearEvent()
	{
		// Clear data an definition
		m_data.Clean();
		m_def.Clean();
	}

	public string FormatScore(long _score)
	{
		// Tracker will do it for us
		if(m_tracker != null) {
			return m_tracker.FormatValue(_score);
		}
		return StringUtils.FormatNumber(_score);
	}

	/// <summary>
	/// Notify the quest of the contribution made by the player
	/// </summary>
	/// <param name="_runScore"></param>
	/// <param name="_keysMultiplier"></param>
	/// <param name="_spentHC"></param>
	/// <param name="_viewAD"></param>
	public void Contribute(float _runScore, float _keysMultiplier, bool _spentHC, bool _viewAD)
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

	public bool ShouldRequestDefinition()
	{
		return false;
	}

	public bool RequestDefinition(bool _force = false)
	{
		// Will never be called in solo quest
		return false;
	}

	public bool IsWaitingForNewDefinition()
	{
		return false;
	}


	public bool EventExists()
	{
		bool ret = false;
		if (m_data != null)
		{
			ret = m_data.definition != null;
		}
		return ret;
	}

	public bool IsTeasing()
	{
		// Solo Quests dont have a teasing phase
		return false;
	}


	public bool IsRunning()
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

	public bool IsActive()
	{
		// Doesnt apply to solo quests
		return true;
	}

	public bool IsRewardPending()
	{
		bool ret = false;
		if (m_data != null && m_data.m_eventId > 0 )
		{
			// Just check the state of the quest
			ret = m_data.m_state == HDLiveEventData.State.REWARD_AVAILABLE;
		}
		return ret;
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