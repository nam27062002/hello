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
public class HdSoloQuestManager : IBroadcastListener, IQuestManager{
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
	public HdSoloQuestManager() {

		m_data = new HDLiveQuestData();
		m_def = m_data.definition as HDLiveQuestDefinition;
        
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~HdSoloQuestManager() {
		m_data = null;
        m_def = null;

	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	/// <summary>
	/// The quest has been activated for the first time
	/// </summary>
	public void Start()
	{
		// Calculate the end timestamp
		
		// Initialize the quest from content
		InitFromDefinition();
		
		// Set the quest state to NOT_JOINED
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
		((HDLiveQuestDefinition)m_data.definition).InitFromDefinition(def);
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

	public double GetRemainingTime()
	{
		throw new NotImplementedException();
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
		throw new NotImplementedException();
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
		throw new NotImplementedException();
	}

	public bool IsRunning()
	{
		throw new NotImplementedException();
	}

	public bool IsActive()
	{
		throw new NotImplementedException();
	}

	public bool IsRewardPending()
	{
		throw new NotImplementedException();
	}
}