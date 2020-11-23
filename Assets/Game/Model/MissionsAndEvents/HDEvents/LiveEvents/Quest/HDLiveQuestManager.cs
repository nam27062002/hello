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
public class HDLiveQuestManager : BaseQuestManager{
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string TYPE_CODE = "quest";
	public const int NUMERIC_TYPE_CODE = 1;

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//

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
	public HDLiveQuestManager() : base() {
		m_type = TYPE_CODE;
		m_numericType = NUMERIC_TYPE_CODE;
    }

    
    //------------------------------------------------------------------------//
    // METHODS														          //
    //------------------------------------------------------------------------//
    private bool ShouldRequestProgress()
    {
        long diff = GameServerManager.GetEstimatedServerTimeAsLong() - m_lastProgressTimestamp;
        return diff > m_requestProgressMinTim;	// 5 min timeout
    }
    
    
    private void RequestProgressResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response)
    {
        HDLiveDataManager.ComunicationErrorCodes outErr = HDLiveDataManager.ComunicationErrorCodes.NO_ERROR;
        HDLiveDataManager.ResponseLog("RequestProgress", _error, _response);
        SimpleJSON.JSONNode responseJson = HDLiveDataManager.ResponseErrorCheck(_error, _response, out outErr);
        if ( outErr == HDLiveDataManager.ComunicationErrorCodes.NO_ERROR )
        {
            HDLiveQuestData liveQuestData = data as HDLiveQuestData;
            if (liveQuestData != null )
            {
                liveQuestData.ParseProgress( responseJson );
            }
            Messenger.Broadcast(MessengerEvents.QUEST_SCORE_UPDATED);
        }
    }
   
    
    private void OnContributeResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response)
    {
        HDLiveDataManager.ComunicationErrorCodes outErr = HDLiveDataManager.ComunicationErrorCodes.NO_ERROR;
        HDLiveDataManager.ResponseLog("Contribute", _error, _response);
        SimpleJSON.JSONNode responseJson = HDLiveDataManager.ResponseErrorCheck(_error, _response, out outErr);
        if ( outErr == HDLiveDataManager.ComunicationErrorCodes.NO_ERROR )
        {
            HDLiveQuestData liveQuestData = data as HDLiveQuestData;
            if (liveQuestData != null )
            {
                liveQuestData.ParseProgress( responseJson );

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

                HDTrackingManager.Instance.Notify_GlobalEventRunDone(liveQuestData.m_eventId, m_def.m_goal.m_type , (int)GetRunScore(), contribution, mult);	// TODO: we have no player score anymore!

                if ( liveQuestData.m_state == HDLiveEventData.State.NOT_JOINED )
                    liveQuestData.m_state = HDLiveEventData.State.JOINED;
            }	
        }

        Messenger.Broadcast<HDLiveDataManager.ComunicationErrorCodes>(MessengerEvents.QUEST_SCORE_SENT, outErr);
        Messenger.Broadcast(MessengerEvents.QUEST_SCORE_UPDATED);

    }

    
    private bool RequestProgress( bool _force = false )
    {
        bool ret = false;
        if ( _force || ShouldRequestProgress() )	
        {
            m_lastProgressTimestamp = GameServerManager.GetEstimatedServerTimeAsLong();
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
    
    
    //------------------------------------------------------------------------//
    // PARENT OVERRIDING													  //
    //------------------------------------------------------------------------//
    
	public override void ParseDefinition(SimpleJSON.JSONNode _data)
    {
    	base.ParseDefinition( _data );
		m_tracker.Clear();
    	HDLiveQuestDefinition def = m_data.definition as HDLiveQuestDefinition;

		if (def.m_goal != null && def.m_goal.m_typeDef != null)
		{
			m_tracker = TrackerBase.CreateTracker(def.m_goal.m_typeDef.sku, def.m_goal.m_params);
		}
    }

    public override void OnLiveDataResponse() {
        if (EventExists() && ShouldRequestProgress())
            RequestProgress();
    }

    protected override void OnEventIdChanged()
	{
		base.OnEventIdChanged();
		m_lastProgressTimestamp = 0;
	}


    protected override void RequestEventDefinitionResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response) {
        base.RequestEventDefinitionResponse(_error, _response);
        if (ShouldRequestProgress())
            RequestProgress();
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

	public override void Contribute(float _runScore,float _keysMultiplier, bool _spentHC, bool _viewAD)
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


    public override bool IsWaitingForNewDefinition()
    {
        // Will never be called in solo quest
        return isWaitingForNewDefinition;
    }

}