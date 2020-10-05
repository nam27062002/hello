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
		throw new NotImplementedException();
	}

	public HDLiveQuestDefinition GetQuestDefinition()
	{
		return m_def;
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
		throw new NotImplementedException();
	}

	public bool RequestDefinition(bool _force)
	{
		throw new NotImplementedException();
	}

	public bool IsWaitingForNewDefinition()
	{
		throw new NotImplementedException();
	}

	public double GetRemainingTime()
	{
		throw new NotImplementedException();
	}


	public long GetRunScore() 
	{
		return m_tracker.currentValue;
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
		return m_active;
	}

	public bool IsRewardPending()
	{
		throw new NotImplementedException();
	}
	



}