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
public class HDQuestManager : HDLiveEventManager, IBroadcastListener{
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string TYPE_CODE = "quest";
	public const int NUMERIC_TYPE_CODE = 1;

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

	long m_lastProgressTimestamp = 0;
	protected long m_requestProgressMinTim = 1000 * 60 * 5;	// 5 min timeout

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public HDQuestManager() {
		m_type = TYPE_CODE;
		m_numericType = NUMERIC_TYPE_CODE;

        m_data = new HDQuestData();
        m_questData = m_data as HDQuestData;
        m_questDefinition = m_questData.definition as HDQuestDefinition;
        
		// Subscribe to external events
		Messenger.AddListener(MessengerEvents.GAME_STARTED, OnGameStarted);
		Broadcaster.AddListener(BroadcastEventType.GAME_ENDED, this);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~HDQuestManager() {
        m_data = null;
        m_questData = null;
        m_questDefinition = null;
		// Unsubscribe from external events
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

	public override void ParseDefinition(SimpleJSON.JSONNode _data)
    {
    	base.ParseDefinition( _data );
		m_tracker.Clear();
    	HDQuestDefinition def = m_data.definition as HDQuestDefinition;
		m_tracker = TrackerBase.CreateTracker( def.m_goal.m_typeDef.sku, def.m_goal.m_params);
    }

    public override void OnLiveDataResponse() {
        if (EventExists() && ShouldRequestProgress())
            RequestProgress();
    }

    override protected void OnEventIdChanged()
	{
		base.OnEventIdChanged();
		m_lastProgressTimestamp = 0;
	}

    public void OnGameStarted(){
    	if ( m_tracker != null)
    	{
            m_tracker.enabled = m_active;
			m_tracker.InitValue(0);
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

    #region server_comunication
    protected override void RequestEventDefinitionResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response) {
        base.RequestEventDefinitionResponse(_error, _response);
        if (ShouldRequestProgress())
            RequestProgress();
    }

	public bool ShouldRequestProgress()
	{
		long diff = GameServerManager.SharedInstance.GetEstimatedServerTimeAsLong() - m_lastProgressTimestamp;
		return diff > m_requestProgressMinTim;	// 5 min timeout
	}

    public bool RequestProgress( bool _force = false )
    {
    	bool ret = false;
    	if ( _force || ShouldRequestProgress() )	
    	{
			m_lastProgressTimestamp = GameServerManager.SharedInstance.GetEstimatedServerTimeAsLong();
	        if ( HDLiveDataManager.TEST_CALLS )
	        {
				ApplicationManager.instance.StartCoroutine( DelayedCall("quest_get_progress.json", RequestProgressResponse));
	        }
	        else
	        {
	            GameServerManager.SharedInstance.HDEvents_GetMyProgess(data.m_eventId, RequestProgressResponse);            
	        }
	        ret = true;
        }
        return ret;
    }

    protected virtual void RequestProgressResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response)
    {
		HDLiveDataManager.ComunicationErrorCodes outErr = HDLiveDataManager.ComunicationErrorCodes.NO_ERROR;
		HDLiveDataManager.ResponseLog("RequestProgress", _error, _response);
		SimpleJSON.JSONNode responseJson = HDLiveDataManager.ResponseErrorCheck(_error, _response, out outErr);
		if ( outErr == HDLiveDataManager.ComunicationErrorCodes.NO_ERROR )
		{
            HDQuestData questData = data as HDQuestData;
            if (questData != null )
            {
				questData.ParseProgress( responseJson );
            }
			Messenger.Broadcast(MessengerEvents.QUEST_SCORE_UPDATED);
		}
    }

	public override void RequestRewards() {
		if (!m_requestingRewards && m_data.m_state < HDLiveEventData.State.REWARD_COLLECTED)
		{
			m_requestingRewards = true;
			if ( HDLiveDataManager.TEST_CALLS )
	        {
				ApplicationManager.instance.StartCoroutine( DelayedCall(m_type + "_rewards.json", RequestRewardsResponse));
	        }
	        else
	        {
				GameServerManager.SharedInstance.HDEvents_Quest_GetMyReward(data.m_eventId, RequestRewardsResponse);    
	        }
        }
	}

	public void Contribute(float _runScore,float _keysMultiplier, bool _spentHC, bool _viewAD)
	{

		// Get contribution amount and apply multipliers
		int contribution = (int)_runScore;
		contribution = (int)(_keysMultiplier * contribution);
		// Requets to the server!

		m_lastContribution = contribution;
		m_lastKeysMultiplier = _keysMultiplier;
		m_lastContributionSpentHC = _spentHC;
		m_lastContributionViewAd = _viewAD;

		Debug.Log("<color=magenta>REGISTER SCORE</color>");
		if ( HDLiveDataManager.TEST_CALLS )
        {
			ApplicationManager.instance.StartCoroutine( DelayedCall("quest_add_progress.json", OnContributeResponse));
		}
		else
		{
			GameServerManager.SharedInstance.HDEvents_Quest_AddProgress(data.m_eventId, contribution, OnContributeResponse);    
		}
	}

	protected virtual void OnContributeResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response)
    {
		HDLiveDataManager.ComunicationErrorCodes outErr = HDLiveDataManager.ComunicationErrorCodes.NO_ERROR;
		HDLiveDataManager.ResponseLog("Contribute", _error, _response);
		SimpleJSON.JSONNode responseJson = HDLiveDataManager.ResponseErrorCheck(_error, _response, out outErr);
		if ( outErr == HDLiveDataManager.ComunicationErrorCodes.NO_ERROR )
		{
            HDQuestData questData = data as HDQuestData;
            if (questData != null )
            {
				questData.ParseProgress( responseJson );

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

				HDTrackingManager.Instance.Notify_GlobalEventRunDone(questData.m_eventId, m_questDefinition.m_goal.m_type , (int)GetRunScore(), contribution, mult);	// TODO: we have no player score anymore!

				if ( questData.m_state == HDLiveEventData.State.NOT_JOINED )
					questData.m_state = HDLiveEventData.State.JOINED;
            }	
		}

		Messenger.Broadcast<HDLiveDataManager.ComunicationErrorCodes>(MessengerEvents.QUEST_SCORE_SENT, outErr);
		Messenger.Broadcast(MessengerEvents.QUEST_SCORE_UPDATED);

    }

#endregion

	public override List<HDLiveData.Reward> GetMyRewards() {
		// Create new list
		List<HDLiveData.Reward> rewards = new List<HDLiveData.Reward>();

		// We must have a valid data and definition
		if(data != null && data.definition != null) {
			// Check reward level
			// In a quest, the reward level tells us in which reward tier have been reached
			// All rewards below it are also given
			HDQuestDefinition def = data.definition as HDQuestDefinition;
			for(int i = 0; i < m_rewardLevel; ++i) {	// Reward level is 1-N
				rewards.Add(def.m_rewards[i]);	// Assuming rewards are properly sorted :)
			}
		}

		// Done!
		return rewards;
	}

	//------------------------------------------------------------------------//
	// UI HELPER METHODS													  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Given a score, format it based on quest type
	/// </summary>
	public string FormatScore(long _score) {
		// Tracker will do it for us
		if(m_tracker != null) {
			return m_tracker.FormatValue(_score);
		}
		return StringUtils.FormatNumber(_score);
	}
	
}