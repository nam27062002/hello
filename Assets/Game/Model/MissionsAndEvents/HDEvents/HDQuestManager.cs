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

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
[Serializable]
public class HDQuestManager : HDLiveEventManager{
	//------------------------------------------------------------------------//
	// CASSES															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	TrackerBase m_tracker = new TrackerBase();
	public HDQuestData m_questData;
	public HDQuestDefinition m_questDefinition;


	int m_lastContribution;
	float m_lastKeysMultiplier;
	bool m_lastContributionViewAd;
	bool m_lastContributionSpentHC;

	bool m_shouldUpdateGlobalScore = false;
	long m_lastProgressTimestamp = 0;
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public HDQuestManager() {
		// Subscribe to external events
		Messenger.AddListener(MessengerEvents.GAME_STARTED, OnGameStarted);
		Messenger.AddListener(MessengerEvents.GAME_ENDED, OnGameEnded);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~HDQuestManager() {
		// Unsubscribe from external events
		Messenger.RemoveListener(MessengerEvents.GAME_STARTED, OnGameStarted);
		Messenger.RemoveListener(MessengerEvents.GAME_ENDED, OnGameEnded);
	}

	public override void BuildData()
    {
		m_data = new HDQuestData();
		m_questData = m_data as HDQuestData;
		m_questDefinition = m_questData.definition as HDQuestDefinition;
    }

	public override void ParseDefinition(SimpleJSON.JSONNode _data)
    {
    	base.ParseDefinition( _data );
		m_tracker.Clear();
    	HDQuestDefinition def = m_data.definition as HDQuestDefinition;
		m_tracker = TrackerBase.CreateTracker( def.m_goal.m_typeDef.sku, def.m_goal.m_params);
    }

    public void OnGameStarted(){
    	if ( m_tracker != null)
    	{
    		m_tracker.SetValue(0, false);
			m_tracker.enabled = m_isActive;
    	}
    }
    public void OnGameEnded(){
    	// Save tracker value?
    }

	public long GetRunScore() 
	{
		return m_tracker.currentValue;
	}


	public virtual string GetGoalDescription() {
		// Use replacements?
		return m_tracker.FormatDescription(m_questDefinition.m_goal.m_desc, m_questDefinition.m_goal.m_amount);
	}


	public float progress {
		get { return Mathf.Clamp01(m_questData.m_globalScore/(float)m_questDefinition.m_goal.m_amount); }
	}

	public bool ShouldRequestProgress()
	{
		long diff = GameServerManager.SharedInstance.GetEstimatedServerTimeAsLong() - m_lastProgressTimestamp;
		return m_shouldUpdateGlobalScore || diff > 300;
	}

    public void RequestProgress()
    {
        if ( HDLiveEventsManager.TEST_CALLS )
        {
            GameServerManager.ServerResponse response = HDLiveEventsManager.CreateTestResponse("quest_get_progress.json");
            RequestProgressResponse(null, response);    
        }
        else
        {
            HDLiveEventData data = GetEventData();
            GameServerManager.SharedInstance.HDEvents_GetMyProgess(data.m_eventId, RequestProgressResponse);            
        }
    }

    protected virtual void RequestProgressResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response)
    {
        if (_error != null)
        {
            // Messenger.Broadcast(MessengerEvents.GLOBAL_EVENT_CUSTOMIZER_ERROR);
            return;
        }

        if (_response != null && _response["response"] != null)
        {
            SimpleJSON.JSONNode responseJson = SimpleJSON.JSONNode.Parse(_response["response"] as string);
            // int eventId = responseJson["code"].AsInt;
            HDQuestData data = GetEventData() as HDQuestData;
            if (data != null /*&& data.m_eventId == eventId*/ )
            {
				data.ParseProgress( responseJson );
            }
			m_shouldUpdateGlobalScore = false;
        }
    }

	public void Contribute(float _runScore, float _bonusDragonMultiplier, float _keysMultiplier, bool _spentHC, bool _viewAD)
	{
		// Get contribution amount and apply multipliers
		int contribution = (int)_runScore;
		contribution = (int)(_bonusDragonMultiplier * contribution);
		contribution = (int)(_keysMultiplier * contribution);
		// Requets to the server!

		m_lastContribution = contribution;
		m_lastKeysMultiplier = _keysMultiplier;
		m_lastContributionSpentHC = _spentHC;
		m_lastContributionViewAd = _viewAD;

		Debug.Log("<color=magenta>REGISTER SCORE</color>");
		if ( HDLiveEventsManager.TEST_CALLS )
        {
			GameServerManager.ServerResponse response = HDLiveEventsManager.CreateTestResponse( "quest_add_progress.json" );
			OnContributeResponse(null, response);
		}
		else
		{
			HDLiveEventData data = GetEventData();
			GameServerManager.SharedInstance.HDEvents_AddProgress(data.m_eventId, contribution, OnContributeResponse);    
		}
	}

	protected virtual void OnContributeResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response)
    {
        if (_error != null)
        {
            // Messenger.Broadcast(MessengerEvents.GLOBAL_EVENT_CUSTOMIZER_ERROR);
            return;
        }

        if (_response != null && _response["response"] != null)
        {
			m_lastProgressTimestamp = GameServerManager.SharedInstance.GetEstimatedServerTimeAsLong();
            SimpleJSON.JSONNode responseJson = SimpleJSON.JSONNode.Parse(_response["response"] as string);
            // int eventId = responseJson["code"].AsInt;
            HDQuestData data = m_data as HDQuestData;
            if (data != null /*&& data.m_eventId == eventId*/)
            {
				data.ParseProgress( responseJson );

				int contribution = m_lastContribution;
				float _keysMultiplier = m_lastKeysMultiplier;
				bool _spentHC = m_lastContributionSpentHC;
				bool _viewAD = m_lastContributionViewAd;

				// Track here!
				HDTrackingManager.EEventMultiplier mult = HDTrackingManager.EEventMultiplier.none;
				if (_keysMultiplier > 1) {
					if (_spentHC) 		mult = HDTrackingManager.EEventMultiplier.hc_payment;
					else if (_viewAD)	mult = HDTrackingManager.EEventMultiplier.ad;
					else 		  		mult = HDTrackingManager.EEventMultiplier.golden_key;
				}

				HDTrackingManager.Instance.Notify_GlobalEventRunDone(data.m_eventId, m_questDefinition.m_goal.m_type , (int)GetRunScore(), contribution, mult);	// TODO: we have no player score anymore!
            }
			m_shouldUpdateGlobalScore = true;
        }
    }


	
}