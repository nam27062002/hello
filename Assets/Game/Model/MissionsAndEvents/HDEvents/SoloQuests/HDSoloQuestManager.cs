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


	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	TrackerBase m_tracker = new TrackerBase();

	private HDLiveQuestData m_data;
	private HDLiveQuestDefinition m_def;


	private bool m_active;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public HDSoloQuestManager() {

		m_data = new HDLiveQuestData();
		m_def = m_data.definition as HDLiveQuestDefinition;
        
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
			
			m_active = false;
			
			return;
		} else if (defs.Count > 1)
		{

			Debug.LogWarning("There are more than one active definitions for soloQuests. " +
			                 "It should be just one.");
		}
		
		// Use the first definition found
		DefinitionNode def = defs[0];

		m_data = new HDLiveQuestData();
		m_data.m_eventId = 1; // Any number higher than zero
		
		m_def = (HDLiveQuestDefinition) m_data.definition;
		m_def.InitFromDefinition(def);
		m_def.m_eventId = m_data.m_eventId;
		
	}
	
	
	//------------------------------------------------------------------------//
	// EVENTS																  //
	//------------------------------------------------------------------------//

	public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
	{
		throw new NotImplementedException();
	}


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

	public void RequestRewards()
	{
		throw new NotImplementedException();
	}

	public List<HDLiveData.Reward> GetMyRewards()
	{
		throw new NotImplementedException();
	}

	public void FinishEvent()
	{
		throw new NotImplementedException();
	}

	public void ClearEvent()
	{
		throw new NotImplementedException();
	}

	public string FormatScore(long _score)
	{
		throw new NotImplementedException();
	}

	public void Contribute(float _runScore, float _keysMultiplier, bool _spentHC, bool _viewAD)
	{
		throw new NotImplementedException();
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
		bool ret = false;
		if (m_data != null)
		{
			ret = m_data.m_state == HDLiveEventData.State.NOT_JOINED || m_data.m_state == HDLiveEventData.State.JOINED;
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
		throw new NotImplementedException();
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